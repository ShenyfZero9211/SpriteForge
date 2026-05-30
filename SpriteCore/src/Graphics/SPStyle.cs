using SkiaSharp;

namespace SpriteCore.Graphics;

/// <summary>
/// 绘图样式数据类。保存 fill/stroke/strokeWeight 等状态，支持 Clone 保存/恢复。
/// 对应 Processing 中的 PStyle。
/// </summary>
public class SPStyle
{
    // Fill
    public bool Fill { get; set; } = true;
    public SKColor FillColor { get; set; } = SKColors.White;

    // Stroke
    public bool Stroke { get; set; } = true;
    public SKColor StrokeColor { get; set; } = SKColors.Black;
    public float StrokeWeight { get; set; } = 1;
    public SKStrokeCap StrokeCap { get; set; } = SKStrokeCap.Round;
    public SKStrokeJoin StrokeJoin { get; set; } = SKStrokeJoin.Miter;

    // Text
    public float TextSize { get; set; } = 16;
    public SKTextAlign TextAlign { get; set; } = SKTextAlign.Left;

    // Color mode (预留：将来支持 HSB)
    public int ColorMode { get; set; } = 0; // 0=RGB, 1=HSB
    public float ColorModeX { get; set; } = 255;
    public float ColorModeY { get; set; } = 255;
    public float ColorModeZ { get; set; } = 255;
    public float ColorModeA { get; set; } = 255;

    // Shape mode (预留：将来支持 rectMode/ellipseMode)
    public int RectMode { get; set; } = 0;   // 0=CORNER
    public int EllipseMode { get; set; } = 0; // 0=CENTER

    // Tint (对应 Processing tint())
    public bool IsTinted { get; set; } = false;
    public SKColor TintColor { get; set; } = SKColors.White;

    public SPStyle Clone()
    {
        return new SPStyle
        {
            Fill = Fill,
            FillColor = FillColor,
            Stroke = Stroke,
            StrokeColor = StrokeColor,
            StrokeWeight = StrokeWeight,
            StrokeCap = StrokeCap,
            StrokeJoin = StrokeJoin,
            TextSize = TextSize,
            TextAlign = TextAlign,
            ColorMode = ColorMode,
            ColorModeX = ColorModeX,
            ColorModeY = ColorModeY,
            ColorModeZ = ColorModeZ,
            ColorModeA = ColorModeA,
            RectMode = RectMode,
            EllipseMode = EllipseMode,
            IsTinted = IsTinted,
            TintColor = TintColor,
        };
    }
}
