namespace SpriteCore.Math;

public static class MathUtils
{
    public static float Lerp(float a, float b, float t)
        => a + (b - a) * Clamp01(t);

    public static float Clamp(float value, float min, float max)
        => value < min ? min : value > max ? max : value;

    public static float Clamp01(float value)
        => value < 0 ? 0 : value > 1 ? 1 : value;

    public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (inMax - inMin == 0) return outMin;
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    public static float RandomRange(float min, float max)
        => min + (float)System.Random.Shared.NextDouble() * (max - min);

    public static int RandomRange(int min, int max)
        => System.Random.Shared.Next(min, max);

    public static float Degrees(float radians)
        => radians * (180f / MathF.PI);

    public static float Radians(float degrees)
        => degrees * (MathF.PI / 180f);

    public static float Abs(float value) => MathF.Abs(value);
    public static float Min(float a, float b) => a < b ? a : b;
    public static float Max(float a, float b) => a > b ? a : b;
    public static float Floor(float value) => MathF.Floor(value);
    public static float Ceil(float value) => MathF.Ceiling(value);
    public static float Round(float value) => MathF.Round(value);
    public static float Sqrt(float value) => MathF.Sqrt(value);
    public static float Sin(float radians) => MathF.Sin(radians);
    public static float Cos(float radians) => MathF.Cos(radians);
    public static float Tan(float radians) => MathF.Tan(radians);
    public static float Atan2(float y, float x) => MathF.Atan2(y, x);
    public static float Pow(float x, float y) => MathF.Pow(x, y);
    public static float Exp(float x) => MathF.Exp(x);
    public static float Log(float x) => MathF.Log(x);

    public static float Distance(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    // Simplex/Perlin Noise placeholder - returns smooth random-ish values
    private static readonly int[] _noisePerm = new int[512];
    private static readonly float[] _noiseValues = new float[512];
    private static bool _noiseInitialized = false;

    public static float Noise(float x)
    {
        if (!_noiseInitialized) InitNoise();
        int ix = (int)MathF.Floor(x) & 255;
        float fx = x - MathF.Floor(x);
        int i0 = _noisePerm[ix];
        int i1 = _noisePerm[ix + 1];
        return Lerp(_noiseValues[i0], _noiseValues[i1], fx);
    }

    public static float Noise(float x, float y)
    {
        if (!_noiseInitialized) InitNoise();
        int ix = (int)MathF.Floor(x) & 255;
        int iy = (int)MathF.Floor(y) & 255;
        float fx = x - MathF.Floor(x);
        float fy = y - MathF.Floor(y);
        int i00 = _noisePerm[_noisePerm[ix] + iy];
        int i10 = _noisePerm[_noisePerm[ix + 1] + iy];
        int i01 = _noisePerm[_noisePerm[ix] + iy + 1];
        int i11 = _noisePerm[_noisePerm[ix + 1] + iy + 1];
        float a = Lerp(_noiseValues[i00], _noiseValues[i10], fx);
        float b = Lerp(_noiseValues[i01], _noiseValues[i11], fx);
        return Lerp(a, b, fy);
    }

    private static void InitNoise()
    {
        if (_noiseInitialized) return;
        var rand = new System.Random(0);
        for (int i = 0; i < 256; i++)
        {
            _noisePerm[i] = i;
            _noiseValues[i] = (float)rand.NextDouble();
        }
        // Shuffle permutation table
        for (int i = 255; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (_noisePerm[i], _noisePerm[j]) = (_noisePerm[j], _noisePerm[i]);
        }
        for (int i = 0; i < 256; i++)
            _noisePerm[256 + i] = _noisePerm[i];
        _noiseInitialized = true;
    }
}
