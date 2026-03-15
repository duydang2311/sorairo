using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using MiniAudioEx.Core.AdvancedAPI;
using MiniAudioEx.Native;
using OneOf;
using OneOf.Types;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;

namespace Sorairo.Common.Services;

public sealed class MiniAudioService : IAudioService
{
    private static ma_sound_flags soundInitFlags =>
        ma_sound_flags.stream
        | ma_sound_flags.no_pitch
        | ma_sound_flags.no_spatialization
        | ma_sound_flags.asynchronous;

    private readonly AudioState audioState;
    private MaEngine? maEngine;
    private ma_sound_ptr maSoundHandle;
    private GCHandle selfHandle;
    private bool isSoundLoaded;
    private CancellationTokenSource? cts;

    public event Action? SoundEnded;
    public bool IsPlaying => audioState.Status == AudioPlaybackStatus.Playing;
    public bool IsPaused => audioState.Status == AudioPlaybackStatus.Paused;

    public MiniAudioService(AudioState audioState)
    {
        this.audioState = audioState;
        audioState.PropertyChanged += OnAudioStatePropertyChanged;
    }

    public OneOf<AudioError, Success> Play2(Uri path)
    {
        if (maEngine is null)
        {
            maEngine = new MaEngine();
            if (maEngine.Initialize() != ma_result.success)
            {
                maEngine.Dispose();
                maEngine = null;
                return new AudioError(
                    AudioErrorKind.EngineInitFailed,
                    "Failed to initialize audio engine"
                );
            }
        }

        if (maSoundHandle.pointer == IntPtr.Zero)
        {
            maSoundHandle.Allocate();
        }

        UnloadSound();
        var soundInitResult = OperatingSystem.IsWindows()
            ? MiniAudioNative.ma_sound_init_from_file_w(
                maEngine.Handle,
                path.LocalPath,
                soundInitFlags,
                default,
                default,
                maSoundHandle
            )
            : MiniAudioNative.ma_sound_init_from_file(
                maEngine.Handle,
                path.LocalPath,
                soundInitFlags,
                default,
                default,
                maSoundHandle
            );
        if (soundInitResult != ma_result.success)
        {
            maSoundHandle.Free();
            return new AudioError(
                AudioErrorKind.SoundInitFailed,
                "Failed to initialize audio from file"
            );
        }

        isSoundLoaded = true;
        MiniAudioNative.ma_sound_set_volume(maSoundHandle, (float)audioState.Volume);
        if (MiniAudioNative.ma_sound_start(maSoundHandle) != ma_result.success)
        {
            UnloadSound();
            FreeMaSoundHandle();
            return new AudioError(AudioErrorKind.SoundStartFailed, "Failed to start audio");
        }

        FreeSelfHandle();
        selfHandle = GCHandle.Alloc(this);
        if (
            MiniAudioNative.ma_sound_set_end_callback(
                maSoundHandle,
                OnSoundEnd,
                GCHandle.ToIntPtr(selfHandle)
            ) != ma_result.success
        )
        {
            UnloadSound();
            FreeMaSoundHandle();
            FreeSelfHandle();
            return new AudioError(
                AudioErrorKind.SetEndCallbackFailed,
                "Failed to set end callback"
            );
        }
        audioState.Status = AudioPlaybackStatus.Playing;
        audioState.TotalTime = GetTotalTime();
        audioState.ElapsedTime = GetElapsedTime();

        _ = StartTrackingElapsedSecondsAsync();
        return new Success();
    }

    private ma_device_ptr deviceHandle;
    private ma_decoder_ptr decoderHandle;

    public OneOf<AudioError, Success> Play(Uri path)
    {
        var decoderConfig = MiniAudioNative.ma_decoder_config_init(ma_format.f32, 0, 0);
        decoderHandle = new ma_decoder_ptr(allocate: true);
        if (
            ma_decoder_init_file_w(path.LocalPath, ref decoderConfig, decoderHandle)
            != ma_result.success
        )
        {
            decoderHandle.Free();
            return new AudioError(AudioErrorKind.EngineInitFailed, "1");
        }
        ma_device_config config = MiniAudioNative.ma_device_config_init(ma_device_type.playback);
        config.playback.format = ma_format.f32;
        config.playback.channels = 0;
        config.sampleRate = 0;
        config.SetDataCallback(OnDeviceData);
        var gcHandle = GCHandle.Alloc((audioState, decoderHandle.pointer));
        config.pUserData = GCHandle.ToIntPtr(gcHandle);

        deviceHandle = new ma_device_ptr(allocate: true);
        if (MiniAudioNative.ma_device_init(default, ref config, deviceHandle) != ma_result.success)
        {
            decoderHandle.Free();
            deviceHandle.Free();
            return new AudioError(AudioErrorKind.EngineInitFailed, "1");
        }

        isSoundLoaded = true;
        MiniAudioNative.ma_device_start(deviceHandle);
        // _ = StartTrackingElapsedSecondsAsync();
        return new Success();
    }

    public void Seek(TimeSpan time)
    {
        audioState.ElapsedTime = time;
        MiniAudioNative.ma_sound_seek_to_second(maSoundHandle, (float)time.TotalSeconds);
    }

    private async Task StartTrackingElapsedSecondsAsync()
    {
        if (cts is not null)
        {
            cts.Cancel();
            cts.Dispose();
        }
        cts = new CancellationTokenSource();
        var ct = cts.Token;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                Dispatcher.UIThread.Post(
                    static (state) =>
                    {
                        var (audioService, audioState) = ((IAudioService, AudioState))state!;
                        if (audioState.Status == AudioPlaybackStatus.Playing)
                        {
                            audioState.ElapsedTime = audioService.GetElapsedTime();
                        }
                    },
                    (this as IAudioService, audioState)
                );
            }
        }
        catch (OperationCanceledException) { }
    }

    public void Stop()
    {
        if (cts is not null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
        audioState.Status = AudioPlaybackStatus.None;
        UnloadSound();
        FreeMaSoundHandle();
        FreeSelfHandle();
    }

    public void Pause()
    {
        audioState.Status = AudioPlaybackStatus.Paused;
        MiniAudioNative.ma_sound_stop(maSoundHandle);
    }

    public void Resume()
    {
        audioState.Status = AudioPlaybackStatus.Playing;
        MiniAudioNative.ma_sound_start(maSoundHandle);
    }

    public TimeSpan GetTotalTime()
    {
        ArgumentNullException.ThrowIfNull(maEngine);
        if (
            MiniAudioNative.ma_sound_get_length_in_seconds(maSoundHandle, out var seconds)
            != ma_result.success
        )
        {
            return TimeSpan.Zero;
        }
        return TimeSpan.FromSeconds(seconds);
    }

    public TimeSpan GetElapsedTime()
    {
        ArgumentNullException.ThrowIfNull(maEngine);
        if (
            MiniAudioNative.ma_sound_get_cursor_in_seconds(maSoundHandle, out var seconds)
            != ma_result.success
        )
        {
            return TimeSpan.Zero;
        }
        return TimeSpan.FromSeconds(seconds);
    }

    public void Dispose()
    {
        audioState.PropertyChanged -= OnAudioStatePropertyChanged;
        Stop();
        if (maEngine is not null)
        {
            maEngine.Dispose();
            maEngine = null;
        }
    }

    private void FreeMaSoundHandle()
    {
        if (maSoundHandle.pointer != IntPtr.Zero)
        {
            maSoundHandle.Free();
        }
    }

    private void FreeSelfHandle()
    {
        if (selfHandle.IsAllocated)
        {
            selfHandle.Free();
            selfHandle = default;
        }
    }

    private void UnloadSound()
    {
        if (isSoundLoaded)
        {
            MiniAudioNative.ma_sound_stop(maSoundHandle);
            MiniAudioNative.ma_sound_uninit(maSoundHandle);
            isSoundLoaded = false;
        }
    }

    private static void OnSoundEnd(IntPtr pUserData, ma_sound_ptr handle)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var selfHandle = GCHandle.FromIntPtr(pUserData);
            var self = (MiniAudioService)selfHandle.Target!;
            self.audioState.ElapsedTime = self.audioState.TotalTime;
            self.Stop();
            if (self.SoundEnded is not null)
            {
                self.SoundEnded();
            }
        });
    }

    private void OnAudioStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(audioState.Volume))
        {
            var state = (AudioState)sender!;
            MiniAudioNative.ma_sound_set_volume(maSoundHandle, (float)state.Volume);
        }
    }

    [DllImport("miniaudioex", CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_fence_init(ma_fence_ptr ptr);

    [DllImport("miniaudioex", CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_fence_wait(ma_fence_ptr ptr);

    private static unsafe void OnDeviceData(
        ma_device_ptr pDevice,
        IntPtr pOutput,
        IntPtr pInput,
        uint frameCount
    )
    {
        var device = pDevice.Get();
        if (device == null)
        {
            return;
        }

        var selfHandle = GCHandle.FromIntPtr(device->pUserData);
        var data = ((AudioState state, IntPtr decoderPtr))selfHandle.Target!;
        var decoder = new ma_decoder_ptr(data.decoderPtr);

        if (
            MiniAudioNative.ma_decoder_read_pcm_frames(decoder, pOutput, frameCount, IntPtr.Zero)
            == ma_result.success
        )
        {
            unsafe
            {
                int sampleCount = (int)(frameCount * decoder.Get()->outputChannels);
                var output = (float*)pOutput;

                if (data.state.Volume < 1)
                {
                    for (var i = 0; i < sampleCount; i++)
                    {
                        output[i] *= (float)data.state.Volume;
                    }
                }
                // var output = new NativeArray<float>(pOutput, sampleCount);

                var buffer = VisualizerService.buffer;
                fixed (float* dst = buffer)
                {
                    Buffer.MemoryCopy(
                        output,
                        dst,
                        buffer.Length * sizeof(float),
                        sampleCount * sizeof(float)
                    );
                }
            }
        }
    }

    [DllImport("miniaudioex", CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_decoder_init_file_w(
        [MarshalAs(UnmanagedType.LPWStr)] string pFilePath,
        ref ma_decoder_config pConfig,
        ma_decoder_ptr pDecoder
    );
}
