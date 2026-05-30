using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 面板容器。自动绘制背景、边框和圆角，常用于信息卡片、对话框背景。
/// </summary>
public class UIPanel : UIContainer
{
    /// <summary>是否绘制背景</summary>
    public bool DrawBackground { get; set; } = true;

    /// <summary>是否绘制边框</summary>
    public bool DrawBorder { get; set; } = true;

    protected override void BuildSelfDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;

        if (DrawBackground && style.BackgroundColor.Alpha > 0)
        {
            drawList.AddRectFilled(
                AbsoluteX, AbsoluteY, Width, Height,
                style.BackgroundColor, style.CornerRadius);
        }

        if (DrawBorder && style.BorderThickness > 0)
        {
            drawList.AddRect(
                AbsoluteX, AbsoluteY, Width, Height,
                style.BorderColor, style.BorderThickness, style.CornerRadius);
        }
    }
}
