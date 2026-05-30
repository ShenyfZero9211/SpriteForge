using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 文本显示 Widget。支持 Measure 和 BuildDrawList。
/// </summary>
public class UILabel : UIElement
{
    public string Text { get; set; } = "";
    public SKTextAlign TextAlign { get; set; } = SKTextAlign.Left;

    public override void Measure()
    {
        var style = ResolvedStyle;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        // 简单估算：后续可替换为 Skia 精确测量
        DesiredWidth = Text.Length * fontSize * 0.6f;
        DesiredHeight = fontSize * 1.2f;
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        if (string.IsNullOrEmpty(Text)) return;
        var style = ResolvedStyle;
        float x = AbsoluteX;
        float y = AbsoluteY;

        // 根据对齐调整 x
        if (TextAlign == SKTextAlign.Center)
            x += Width / 2;
        else if (TextAlign == SKTextAlign.Right)
            x += Width;

        drawList.AddText(Text, x, y, style.TextColor, style.FontSize);
    }
}
