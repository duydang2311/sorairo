using System.Numerics;
using FftFlat;
using MathNet.Numerics.IntegralTransforms;

namespace Sorairo.Common.Services;

public sealed class VisualizerService
{
    public const int FFT_SIZE = 1024;
    public const int BAR_COUNT = 8;

    public static float[] buffer = new float[8192];

    static readonly float[] fftBuffer = new float[FFT_SIZE];
    static readonly Complex[] complex = new Complex[FFT_SIZE];
    static readonly float[] magnitudes = new float[FFT_SIZE / 2];
    static readonly float[] bars = new float[BAR_COUNT];
    static readonly float[] smoothBars = new float[BAR_COUNT];
    public static readonly float[] targetBars = new float[BAR_COUNT];
    public static readonly float[] animatedBars = new float[BAR_COUNT];

    static readonly float[] window = BuildHannWindow();
    static readonly FastFourierTransform fft = new FastFourierTransform(FFT_SIZE);
    static int fftIndex = 0;

    static float[] BuildHannWindow()
    {
        var w = new float[FFT_SIZE];
        for (int i = 0; i < FFT_SIZE; i++)
            w[i] = 0.5f * (1 - MathF.Cos(2 * MathF.PI * i / (FFT_SIZE - 1)));
        return w;
    }

    public static void ProcessBuffer(int sampleCount)
    {
        for (int i = 0; i < sampleCount; i += 2)
        {
            float mono = (buffer[i] + buffer[i + 1]) * 0.5f;
            fftBuffer[fftIndex++] = mono;
            if (fftIndex >= FFT_SIZE)
            {
                fftIndex = 0;
                ProcessFFT();
            }
        }
    }

    static void ProcessFFT()
    {
        for (int i = 0; i < FFT_SIZE; i++)
            complex[i] = new Complex(fftBuffer[i] * window[i], 0);

        fft.Forward(complex);

        for (int i = 0; i < magnitudes.Length; i++)
            magnitudes[i] = (float)complex[i].Magnitude;

        BuildBars();
    }

    static void BuildBars()
    {
        int fftBins = magnitudes.Length;

        for (int i = 0; i < BAR_COUNT; i++)
        {
            float t0 = (float)i / BAR_COUNT;
            float t1 = (float)(i + 1) / BAR_COUNT;
            int start = (int)(MathF.Pow(t0, 2.2f) * fftBins);
            int end = (int)(MathF.Pow(t1, 2.2f) * fftBins);
            if (end <= start)
                end = start + 1;

            float sum = 0;
            for (int j = start; j < end && j < fftBins; j++)
                sum += magnitudes[j];

            targetBars[i] = MathF.Log10(sum / (end - start) + 1f);
        }
    }
}
