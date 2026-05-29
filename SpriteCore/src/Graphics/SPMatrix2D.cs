using SkiaSharp;

namespace SpriteCore.Graphics;

/// <summary>
/// 2D 变换矩阵。独立管理平移/旋转/缩放，不直接依赖底层图形库。
/// 对应 Processing 中的 PMatrix2D。
/// </summary>
public class SPMatrix2D
{
    private SKMatrix _matrix = SKMatrix.CreateIdentity();

    public void Reset() => _matrix = SKMatrix.CreateIdentity();

    public void Translate(float x, float y)
        => _matrix = _matrix.PreConcat(SKMatrix.CreateTranslation(x, y));

    public void Rotate(float angle)
        => _matrix = _matrix.PreConcat(SKMatrix.CreateRotationDegrees(angle));

    public void Scale(float x, float y)
        => _matrix = _matrix.PreConcat(SKMatrix.CreateScale(x, y));

    public void Skew(float x, float y)
        => _matrix = _matrix.PreConcat(SKMatrix.CreateSkew(x, y));

    public SKMatrix ToSkMatrix() => _matrix;

    public void SetMatrix(SKMatrix m) => _matrix = m;

    public SPMatrix2D Clone() => new() { _matrix = _matrix };
}
