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
    public SPStrokeCap StrokeCap { get; set; } = SPStrokeCap.ROUND;
    public SPStrokeJoin StrokeJoin { get; set; } = SPStrokeJoin.MITER;

    // Text
    public float TextSize { get; set; } = 16;
    public SPTextAlignH TextAlignH { get; set; } = SPTextAlignH.LEFT;
    public SPTextAlignV TextAlignV { get; set; } = SPTextAlignV.BASELINE;

    // Color mode (预留：将来支持 HSB)
    public SPColorMode ColorMode { get; set; } = SPColorMode.RGB;
    public float ColorModeX { get; set; } = 255;
    public float ColorModeY { get; set; } = 255;
    public float ColorModeZ { get; set; } = 255;
    public float ColorModeA { get; set; } = 255;

    // Shape mode
    public SPRectMode RectMode { get; set; } = SPRectMode.CORNER;
    public SPEllipseMode EllipseMode { get; set; } = SPEllipseMode.CENTER;
    public SPImageMode ImageMode { get; set; } = SPImageMode.CORNER;

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
            TextAlignH = TextAlignH,
            TextAlignV = TextAlignV,
            ColorMode = ColorMode,
            ColorModeX = ColorModeX,
            ColorModeY = ColorModeY,
            ColorModeZ = ColorModeZ,
            ColorModeA = ColorModeA,
            RectMode = RectMode,
            EllipseMode = EllipseMode,
            ImageMode = ImageMode,
            IsTinted = IsTinted,
            TintColor = TintColor,
        };
    }
}
