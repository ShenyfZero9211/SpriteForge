using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 复选框 Widget。点击切换勾选状态。
/// </summary>
public class UICheckbox : UIElement
{
    public bool IsChecked { get; set; }
    public string Text { get; set; } = "";

    /// <summary>勾选状态变化时触发</summary>
    public event Action<bool>? OnValueChanged;

    public float BoxSize { get; set; } = 16;
    public float CheckPadding { get; set; } = 3;

    /// <summary>鼠标是否悬停在复选框上</summary>
    public bool IsHovered { get; private set; }

    public UICheckbox()
    {
        Focusable = true;
    }

    public override void Measure()
    {
        var style = ResolvedStyle;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        float textW = Text.Length * fontSize * 0.6f;
        DesiredWidth = BoxSize + 6 + textW;
        DesiredHeight = Math.Max(BoxSize, fontSize * 1.4f);
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        float boxX = AbsoluteX;
        float boxY = AbsoluteY + (Height - BoxSize) / 2;

        // 方框背景
        var bgColor = IsHovered ? style.HoverColor : style.BackgroundColor;
        drawList.AddRectFilled(boxX, boxY, BoxSize, BoxSize, bgColor, 2);

        // 方框边框
        if (style.BorderThickness > 0)
        {
            drawList.AddRect(boxX, boxY, BoxSize, BoxSize,
                style.BorderColor, style.BorderThickness, 2);
        }

        // 勾选标记
        if (IsChecked)
        {
            float pad = CheckPadding;
            float cx = boxX + pad;
            float cy = boxY + pad;
            float cw = BoxSize - pad * 2;
            float ch = BoxSize - pad * 2;
            drawList.AddRectFilled(cx, cy, cw, ch, style.ForegroundColor, 1);
        }

        // 文本
        if (!string.IsNullOrEmpty(Text))
        {
            float fontSize = style.FontSize > 0 ? style.FontSize : 14;
            float tx = boxX + BoxSize + 6;
            float ty = AbsoluteY + Height / 2 + fontSize * 0.35f;
            drawList.AddText(Text, tx, ty, style.TextColor, fontSize);
        }
    }

    public override bool OnEvent(UIEvent evt)
    {
        if (!Enabled) return false;

        switch (evt.Type)
        {
            case UIEventType.MouseMoved:
                bool nowHovered = ContainsPoint(evt.MouseX, evt.MouseY);
                if (nowHovered != IsHovered)
                    IsHovered = nowHovered;
                return nowHovered;

            case UIEventType.MousePressed:
                if (ContainsPoint(evt.MouseX, evt.MouseY))
                {
                    return true;
                }
                break;

            case UIEventType.MouseReleased:
                if (ContainsPoint(evt.MouseX, evt.MouseY))
                {
                    Toggle();
                    return true;
                }
                break;

            case UIEventType.KeyPressed:
                if (evt.KeyCode == (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE ||
                    evt.KeyCode == (int)SDL2.SDL.SDL_Keycode.SDLK_RETURN ||
                    evt.KeyCode == (int)SDL2.SDL.SDL_Keycode.SDLK_KP_ENTER)
                {
                    Toggle();
                    return true;
                }
                break;
        }

        return false;
    }

    private void Toggle()
    {
        IsChecked = !IsChecked;
        OnValueChanged?.Invoke(IsChecked);
    }
}
