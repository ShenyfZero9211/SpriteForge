using SkiaSharp;
using SpriteCore.Graphics;

namespace SpriteEngine.Scenes;

/// <summary>
/// 精灵渲染器：在 2D 场景中绘制彩色矩形。
/// 附加到 GameObject 上，随 Transform 的位置/旋转/缩放自动变换。
/// </summary>
public class SpriteRenderer : Component
{
    /// <summary>填充颜色</summary>
    public SKColor Color { get; set; } = SKColors.White;

    /// <summary>宽度（像素）</summary>
    public float Width { get; set; } = 32;

    /// <summary>高度（像素）</summary>
    public float Height { get; set; } = 32;

    /// <summary>是否绘制边框</summary>
    public bool DrawStroke { get; set; } = false;

    /// <summary>边框颜色</summary>
    public SKColor StrokeColor { get; set; } = SKColors.Black;

    /// <summary>边框粗细</summary>
    public float StrokeWeight { get; set; } = 1;

    /// <summary>
    /// 渲染到指定的 SPGraphics 上。
    /// 由 Scene.Render() 或外部渲染循环调用。
    /// </summary>
    public void Render(SPGraphics g)
    {
        if (!Enabled || Transform == null) return;

        var pos = Transform.Position;
        var rot = Transform.Rotation;
        var scale = Transform.Scale;

        g.PushMatrix();
        g.Translate(pos.X, pos.Y);
        g.Rotate(rot);
        g.Scale(scale.X, scale.Y);

        // 居中绘制
        float halfW = Width / 2;
        float halfH = Height / 2;

        if (DrawStroke)
        {
            g.Stroke(StrokeColor.Red, StrokeColor.Green, StrokeColor.Blue);
            g.StrokeWeight(StrokeWeight);
        }
        else
        {
            g.NoStroke();
        }

        g.Fill(Color.Red, Color.Green, Color.Blue);
        g.Rect(-halfW, -halfH, Width, Height);

        g.PopMatrix();
    }
}
