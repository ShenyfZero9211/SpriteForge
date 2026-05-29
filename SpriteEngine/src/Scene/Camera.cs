using SpriteCore.Graphics;
using SpriteCore.Math;

namespace SpriteEngine.Scenes;

/// <summary>
/// 2D 相机组件。控制场景的观察视角（位置、旋转、缩放）。
/// 每个场景通常有一个主相机。
/// </summary>
public class Camera : Component
{
    /// <summary>相机在世界空间中的中心位置</summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>相机旋转（角度，顺时针）</summary>
    public float Rotation { get; set; } = 0;

    /// <summary>缩放倍数（1 = 原始大小，2 = 放大 2 倍）</summary>
    public float Zoom { get; set; } = 1f;

    /// <summary>视口宽度</summary>
    public float ViewportWidth => SP5.Width;

    /// <summary>视口高度</summary>
    public float ViewportHeight => SP5.Height;

    /// <summary>
    /// 将世界坐标转换为屏幕坐标。
    /// 计算公式：screen = ((world - Position) * R(-Rotation) * S(Zoom)) + viewportCenter
    /// </summary>
    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        float dx = worldPos.X - Position.X;
        float dy = worldPos.Y - Position.Y;

        float rad = MathUtils.Radians(-Rotation);
        float cos = MathUtils.Cos(rad);
        float sin = MathUtils.Sin(rad);

        float rx = dx * cos - dy * sin;
        float ry = dx * sin + dy * cos;

        return new Vector2(
            rx * Zoom + ViewportWidth / 2,
            ry * Zoom + ViewportHeight / 2);
    }

    /// <summary>
    /// 将屏幕坐标转换为世界坐标（逆变换）。
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        float localX = (screenPos.X - ViewportWidth / 2) / Zoom;
        float localY = (screenPos.Y - ViewportHeight / 2) / Zoom;

        float rad = MathUtils.Radians(Rotation);
        float cos = MathUtils.Cos(rad);
        float sin = MathUtils.Sin(rad);

        float wx = localX * cos - localY * sin;
        float wy = localX * sin + localY * cos;

        return new Vector2(wx + Position.X, wy + Position.Y);
    }

    /// <summary>
    /// 在 SPGraphics 上应用相机变换。
    /// 必须在 Scene.Render() 之前调用。
    /// </summary>
    public void Apply(SPGraphics g)
    {
        if (Transform != null)
        {
            Position = Transform.Position;
            Rotation = Transform.Rotation;
            // 使用 Transform.Scale.X 作为统一缩放（2D 相机通常各向同性缩放）
            Zoom = Transform.Scale.X;
        }

        g.PushMatrix();
        // 行向量系统下：v' = v * T(center) * S(zoom) * R(-rot) * T(-pos)
        // PreConcat 使矩阵从左到右累积，所以调用顺序是：
        // Translate(-pos) → Rotate(-rot) → Scale(zoom) → Translate(center)
        g.Translate(-Position.X, -Position.Y);
        g.Rotate(-Rotation);
        g.Scale(Zoom, Zoom);
        g.Translate(ViewportWidth / 2, ViewportHeight / 2);
    }

    /// <summary>
    /// 恢复相机变换（PopMatrix）。
    /// 必须在 Scene.Render() 之后调用。
    /// </summary>
    public void Restore(SPGraphics g)
    {
        g.PopMatrix();
    }

    /// <summary>
    /// 平滑跟随目标位置（每帧调用）。
    /// </summary>
    /// <param name="target">目标世界坐标</param>
    /// <param name="smoothTime">平滑时间（秒，越小越灵敏）</param>
    public void Follow(Vector2 target, float smoothTime)
    {
        float t = MathUtils.Clamp01(1f - MathUtils.Pow(0.001f, smoothTime));
        Position = new Vector2(
            MathUtils.Lerp(Position.X, target.X, t),
            MathUtils.Lerp(Position.Y, target.Y, t));
    }

    /// <summary>
    /// 将相机位置直接设置为目标（无平滑）。</summary>
    public void LookAt(Vector2 target)
    {
        Position = target;
    }
}
