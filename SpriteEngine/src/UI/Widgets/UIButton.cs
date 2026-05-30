using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 按钮 Widget。支持 Normal / Hover / Pressed / Disabled 四种视觉状态，
/// 以及 OnClick / OnHoverEnter / OnHoverExit 事件回调。
/// </summary>
public class UIButton : UIElement
{
    public string Text { get; set; } = "";

    // ── 事件 ──
    public event Action? OnClick;
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;

    // ── 状态（只读，由事件系统驱动）──
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }

    // ── 可选：自定义各状态颜色（null 时使用 ResolvedStyle）──
    public SKColor? NormalColor { get; set; }
    public SKColor? HoverColor { get; set; }
    public SKColor? PressedColor { get; set; }
    public SKColor? DisabledColor { get; set; }
    public SKColor? TextColor { get; set; }

    public override void Measure()
    {
        var style = ResolvedStyle;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        // 简单估算：字符数 × 字体大小 × 0.6，加上内边距
        float padH = style.Padding.Horizontal;
        float padV = style.Padding.Vertical;
        float desiredW = Text.Length * fontSize * 0.6f + padH;
        float desiredH = fontSize * 1.4f + padV;
        // 尊重显式尺寸（取较大值）
        DesiredWidth = Width > 0 ? Math.Max(Width, desiredW) : desiredW;
        DesiredHeight = Height > 0 ? Math.Max(Height, desiredH) : desiredH;
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var bgColor = ResolveBackgroundColor();
        var txtColor = TextColor ?? ResolvedStyle.TextColor;
        float radius = ResolvedStyle.CornerRadius;

        // 绘制背景
        drawList.AddRectFilled(AbsoluteX, AbsoluteY, Width, Height, bgColor, radius);

        // 绘制边框（可选）
        var style = ResolvedStyle;
        if (style.BorderThickness > 0)
        {
            drawList.AddRect(AbsoluteX, AbsoluteY, Width, Height, style.BorderColor,
                style.BorderThickness, radius);
        }

        // 绘制文本（在 Padding 区域内居中）
        if (!string.IsNullOrEmpty(Text))
        {
            float fontSize = style.FontSize > 0 ? style.FontSize : 14;
            // 简单估算文本尺寸
            float textW = Text.Length * fontSize * 0.6f;
            float textH = fontSize * 1.2f;

            // 内容区域（扣除 Padding）
            float contentL = AbsoluteX + style.Padding.Left;
            float contentT = AbsoluteY + style.Padding.Top;
            float contentW = Math.Max(0, Width - style.Padding.Horizontal);
            float contentH = Math.Max(0, Height - style.Padding.Vertical);

            float tx = contentL + (contentW - textW) / 2;
            // SkiaSharp DrawText 的 y 是基线位置，需要向下偏移约 0.75 倍字号让文本视觉居中
            float ty = contentT + contentH / 2 + fontSize * 0.35f;

            // 按下时轻微位移，提供触觉反馈感
            if (IsPressed)
            {
                tx += 1;
                ty += 1;
            }

            drawList.AddText(Text, tx, ty, txtColor, fontSize);
        }
    }

    public override bool OnEvent(UIEvent evt)
    {
        if (!Enabled) return false;

        switch (evt.Type)
        {
            case UIEventType.MouseMoved:
                // 未按下时：正常的 hover 进出
                if (!IsPressed)
                {
                    bool nowHovered = ContainsPoint(evt.MouseX, evt.MouseY);
                    if (nowHovered && !IsHovered)
                    {
                        IsHovered = true;
                        OnHoverEnter?.Invoke();
                    }
                    else if (!nowHovered && IsHovered)
                    {
                        IsHovered = false;
                        OnHoverExit?.Invoke();
                    }
                    return nowHovered;
                }
                // 按下时：MouseMoved 不分发给按钮（由 MouseDragged 跟踪）
                return false;

            case UIEventType.MousePressed:
                if (IsHovered)
                {
                    IsPressed = true;
                    return true;
                }
                break;

            case UIEventType.MouseDragged:
                // 按下并拖拽时：只更新 hover 状态，不清除 pressed
                bool dragHovered = ContainsPoint(evt.MouseX, evt.MouseY);
                if (dragHovered != IsHovered)
                {
                    IsHovered = dragHovered;
                    if (!IsHovered)
                        OnHoverExit?.Invoke();
                    else
                        OnHoverEnter?.Invoke();
                }
                return true;

            case UIEventType.MouseReleased:
                if (IsPressed)
                {
                    IsPressed = false;
                    if (IsHovered)
                    {
                        OnClick?.Invoke();
                    }
                    return true;
                }
                break;
        }

        return false;
    }

    private SKColor ResolveBackgroundColor()
    {
        if (!Enabled)
            return DisabledColor ?? ResolvedStyle.DisabledColor;
        if (IsPressed)
            return PressedColor ?? ResolvedStyle.PressedColor;
        if (IsHovered)
            return HoverColor ?? ResolvedStyle.HoverColor;
        return NormalColor ?? ResolvedStyle.BackgroundColor;
    }
}
