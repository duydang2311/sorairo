using System.Runtime.InteropServices;
using MiniAudioEx.Core.AdvancedAPI;
using MiniAudioEx.Native;

namespace Sorairo.Infras.Audio;

public sealed class MiniAudioService : IAudioService
{
    private MaEngine? maEngine;
    private ma_sound_ptr maSoundHandle;

    public void Play(Uri path)
    {
        if (maEngine is null)
        {
            maEngine = new MaEngine();
            if (maEngine.Initialize() != ma_result.success)
            {
                maEngine.Dispose();
                maEngine = null;
                return;
            }
        }

        DisposeMaSoundHandle();
        maSoundHandle = new(allocate: true);
        if (
            ma_sound_init_from_file_w(
                maEngine.Handle,
                path.LocalPath,
                ma_sound_flags.stream,
                default,
                default,
                maSoundHandle
            ) != ma_result.success
        )
        {
            DisposeMaSoundHandle();
            return;
        }
        if (MiniAudioNative.ma_sound_start(maSoundHandle) != ma_result.success)
        {
            DisposeMaSoundHandle();
            return;
        }
    }

    public void Dispose()
    {
        DisposeMaSoundHandle();
        if (maEngine is not null)
        {
            maEngine.Dispose();
            maEngine = null;
        }
    }

    [DllImport("miniaudioex", CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_sound_init_from_file_w(
        ma_engine_ptr pEngine,
        [MarshalAs(UnmanagedType.LPWStr)] string pFilePath,
        ma_sound_flags flags,
        ma_sound_group_ptr pGroup,
        ma_fence_ptr pDoneFence,
        ma_sound_ptr pSound
    );

    public void DisposeMaSoundHandle()
    {
        if (maSoundHandle.pointer != IntPtr.Zero)
        {
            MiniAudioNative.ma_sound_stop(maSoundHandle);
            MiniAudioNative.ma_sound_uninit(maSoundHandle);
            maSoundHandle.Free();
        }
    }
}
