using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 进度条 Widget。展示 0~1（或自定义范围）的进度比例，无交互。
/// </summary>
public class UIProgressBar : UIElement
{
    public float MinValue { get; set; } = 0;
    public float MaxValue { get; set; } = 1;
    public float Value { get; set; } = 0;

    /// <summary>方向</summary>
    public SliderDirection Direction { get; set; } = SliderDirection.Horizontal;

    /// <summary>可选：自定义填充颜色（null 时使用 ResolvedStyle.ForegroundColor）</summary>
    public SKColor? FillColor { get; set; }

    /// <summary>可选：自定义轨道颜色（null 时使用 ResolvedStyle.BackgroundColor）</summary>
    public SKColor? TrackColor { get; set; }

    /// <summary>轨道圆角半径</summary>
    public float TrackRadius { get; set; } = 4;

    public override void Measure()
    {
        if (Direction == SliderDirection.Horizontal)
        {
            DesiredWidth = Width > 0 ? Width : 120;
            DesiredHeight = Height > 0 ? Height : 8;
        }
        else
        {
            DesiredWidth = Width > 0 ? Width : 8;
            DesiredHeight = Height > 0 ? Height : 120;
        }
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        var trackColor = TrackColor ?? style.BackgroundColor;
        var fillColor = FillColor ?? style.ForegroundColor;

        float ratio = (Value - MinValue) / Math.Max(MaxValue - MinValue, 0.0001f);
        ratio = Math.Clamp(ratio, 0, 1);

        // 轨道背景
        drawList.AddRectFilled(AbsoluteX, AbsoluteY, Width, Height, trackColor, TrackRadius);

        // 填充部分
        if (ratio > 0)
        {
            if (Direction == SliderDirection.Horizontal)
            {
                float fillW = Width * ratio;
                drawList.AddRectFilled(AbsoluteX, AbsoluteY, fillW, Height, fillColor, TrackRadius);
            }
            else
            {
                float fillH = Height * ratio;
                drawList.AddRectFilled(AbsoluteX, AbsoluteY + Height - fillH, Width, fillH, fillColor, TrackRadius);
            }
        }
    }
}
