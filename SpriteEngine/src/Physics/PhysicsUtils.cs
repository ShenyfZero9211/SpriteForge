using SpriteCore.Math;

namespace SpriteEngine.Physics;

/// <summary>
/// 物理单位与像素单位之间的转换工具。
/// Aether.Physics2D 使用米（MKS 单位制），屏幕使用像素。
/// </summary>
public static class PhysicsUtils
{
    /// <summary>每米对应的像素数。默认 32px = 1m。</summary>
    public static float PixelsPerMeter { get; set; } = 32f;

    /// <summary>像素 → 物理世界（米）</summary>
    public static float ToPhysics(float pixels) => pixels / PixelsPerMeter;

    /// <summary>物理世界（米） → 像素</summary>
    public static float ToPixels(float meters) => meters * PixelsPerMeter;

    /// <summary>像素向量 → 物理世界向量（米）</summary>
    public static nkast.Aether.Physics2D.Common.Vector2 ToPhysics(Vector2 pixels)
        => new(pixels.X / PixelsPerMeter, pixels.Y / PixelsPerMeter);

    /// <summary>物理世界向量（米） → 像素向量</summary>
    public static Vector2 ToPixels(nkast.Aether.Physics2D.Common.Vector2 meters)
        => new(meters.X * PixelsPerMeter, meters.Y * PixelsPerMeter);
}
