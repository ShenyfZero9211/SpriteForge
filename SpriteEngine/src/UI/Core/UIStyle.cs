using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// UI 样式结构体。扁平字段设计，轻量可复制。
/// 受 egui Style 启发，避免 Theme 接口的多方法实现。
/// </summary>
public struct UIStyle
{
    // ── 颜色 ──
    public SKColor BackgroundColor;
    public SKColor ForegroundColor;
    public SKColor BorderColor;
    public SKColor HoverColor;
    public SKColor PressedColor;
    public SKColor DisabledColor;
    public SKColor TextColor;

    // ── 尺寸 ──
    public float FontSize;
    public float BorderThickness;
    public float CornerRadius;
    public Thickness Padding;
    public float Gap;              // Flex 子项间距

    // ── 行为 ──
    public float TransitionDuration;  // 颜色过渡时长（0 = 即时）

    /// <summary>创建默认浅色主题样式</summary>
    public static UIStyle DefaultLight() => new()
    {
        BackgroundColor = new SKColor(240, 240, 240),
        ForegroundColor = new SKColor(220, 220, 220),
        BorderColor = new SKColor(180, 180, 180),
        HoverColor = new SKColor(200, 200, 255),
        PressedColor = new SKColor(160, 160, 240),
        DisabledColor = new SKColor(200, 200, 200),
        TextColor = new SKColor(30, 30, 30),
        FontSize = 14,
        BorderThickness = 1,
        CornerRadius = 4,
        Padding = new Thickness(8, 8, 8, 8),
        Gap = 4,
        TransitionDuration = 0,
    };

    /// <summary>创建默认深色主题样式</summary>
    public static UIStyle DefaultDark() => new()
    {
        BackgroundColor = new SKColor(40, 40, 45),
        ForegroundColor = new SKColor(60, 60, 65),
        BorderColor = new SKColor(80, 80, 85),
        HoverColor = new SKColor(70, 70, 90),
        PressedColor = new SKColor(90, 90, 120),
        DisabledColor = new SKColor(60, 60, 60),
        TextColor = new SKColor(220, 220, 220),
        FontSize = 14,
        BorderThickness = 1,
        CornerRadius = 4,
        Padding = new Thickness(8, 8, 8, 8),
        Gap = 4,
        TransitionDuration = 0,
    };
}

/// <summary>四边边距结构体</summary>
public struct Thickness
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;

    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }
    public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public Thickness(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}
