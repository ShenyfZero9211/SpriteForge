using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 单行文本输入框 Widget。支持键盘输入、Backspace、Delete、左右箭头。
/// 需要 Focusable = true 才能接收键盘事件。
/// </summary>
public class UITextInput : UIElement
{
    public string Text { get; set; } = "";
    public string Placeholder { get; set; } = "";

    /// <summary>是否为密码模式。为 true 时显示掩码字符。</summary>
    public bool IsPasswordMode { get; set; }

    /// <summary>密码掩码字符，默认 '•'</summary>
    public char PasswordChar { get; set; } = '•';

    /// <summary>文本变化时触发</summary>
    public event Action<string>? OnTextChanged;

    /// <summary>提交时触发（按回车）</summary>
    public event Action<string>? OnSubmit;

    public int MaxLength { get; set; } = 256;

    private int _caretPosition;
    private float _blinkTimer;
    private bool _showCaret;

    /// <summary>鼠标是否悬停在输入框上</summary>
    public bool IsHovered { get; private set; }

    /// <summary>是否拥有键盘焦点</summary>
    public bool IsFocused { get; private set; }

    public UITextInput()
    {
        Focusable = true;
    }

    public override void Measure()
    {
        var style = ResolvedStyle;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        DesiredWidth = Width > 0 ? Width : 120;
        DesiredHeight = fontSize * 1.8f + style.Padding.Vertical;
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        float radius = style.CornerRadius;

        // 背景（聚焦时高亮边框颜色）
        var bgColor = IsHovered || _showCaret
            ? new SKColor(
                (byte)Math.Min(style.BackgroundColor.Red + 10, 255),
                (byte)Math.Min(style.BackgroundColor.Green + 10, 255),
                (byte)Math.Min(style.BackgroundColor.Blue + 10, 255),
                style.BackgroundColor.Alpha)
            : style.BackgroundColor;

        drawList.AddRectFilled(AbsoluteX, AbsoluteY, Width, Height, bgColor, radius);

        if (style.BorderThickness > 0)
        {
            var borderColor = IsFocused
                ? new SKColor(100, 160, 255)
                : style.BorderColor;
            drawList.AddRect(AbsoluteX, AbsoluteY, Width, Height,
                borderColor, style.BorderThickness, radius);
        }

        // 文本区域
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        float padL = style.Padding.Left;
        float padT = style.Padding.Top;
        float contentH = Height - style.Padding.Vertical;
        float textY = AbsoluteY + padT + contentH / 2 + fontSize * 0.35f;

        string displayText = IsPasswordMode ? new string(PasswordChar, Text.Length) : Text;
        if (string.IsNullOrEmpty(displayText) && !string.IsNullOrEmpty(Placeholder))
        {
            // Placeholder 用半透明颜色
            var placeholderColor = new SKColor(
                style.TextColor.Red, style.TextColor.Green, style.TextColor.Blue,
                (byte)(style.TextColor.Alpha / 2));
            drawList.AddText(Placeholder, AbsoluteX + padL, textY, placeholderColor, fontSize);
        }
        else if (!string.IsNullOrEmpty(displayText))
        {
            drawList.AddText(displayText, AbsoluteX + padL, textY, style.TextColor, fontSize);

            // 光标（仅当聚焦且闪烁显示时）
            if (IsFocused && _showCaret)
            {
                float caretX = AbsoluteX + padL + _caretPosition * fontSize * 0.6f;
                float caretY1 = AbsoluteY + padT + 2;
                float caretY2 = AbsoluteY + Height - padT - 2;
                drawList.AddLine(caretX, caretY1, caretX, caretY2, style.TextColor, 1);
            }
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
                    _showCaret = true;
                    _blinkTimer = 0;
                    return true;
                }
                break;

            case UIEventType.FocusGained:
                IsFocused = true;
                _showCaret = true;
                _blinkTimer = 0;
                return true;

            case UIEventType.FocusLost:
                IsFocused = false;
                _showCaret = false;
                return true;

            case UIEventType.KeyPressed:
                return HandleKeyPress(evt);

            case UIEventType.KeyTyped:
                return HandleCharInput(evt.KeyChar);
        }

        return false;
    }

    public override void Update(float dt)
    {
        // 光标闪烁
        if (IsFocused)
        {
            _blinkTimer += dt;
            if (_blinkTimer > 0.5f)
            {
                _blinkTimer = 0;
                _showCaret = !_showCaret;
            }
        }
        else
        {
            _showCaret = false;
        }
    }

    private bool HandleKeyPress(UIEvent evt)
    {
        switch (evt.KeyCode)
        {
            case (int)SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE:
                if (_caretPosition > 0 && Text.Length > 0)
                {
                    Text = Text.Remove(_caretPosition - 1, 1);
                    _caretPosition--;
                    OnTextChanged?.Invoke(Text);
                }
                return true;

            case (int)SDL2.SDL.SDL_Keycode.SDLK_DELETE:
                if (_caretPosition < Text.Length)
                {
                    Text = Text.Remove(_caretPosition, 1);
                    OnTextChanged?.Invoke(Text);
                }
                return true;

            case (int)SDL2.SDL.SDL_Keycode.SDLK_LEFT:
                if (_caretPosition > 0) _caretPosition--;
                return true;

            case (int)SDL2.SDL.SDL_Keycode.SDLK_RIGHT:
                if (_caretPosition < Text.Length) _caretPosition++;
                return true;

            case (int)SDL2.SDL.SDL_Keycode.SDLK_HOME:
                _caretPosition = 0;
                return true;

            case (int)SDL2.SDL.SDL_Keycode.SDLK_END:
                _caretPosition = Text.Length;
                return true;

            case (int)SDL2.SDL.SDL_Keycode.SDLK_RETURN:
            case (int)SDL2.SDL.SDL_Keycode.SDLK_KP_ENTER:
                OnSubmit?.Invoke(Text);
                return true;
        }

        return false;
    }

    private bool HandleCharInput(char c)
    {
        // 忽略控制字符
        if (char.IsControl(c)) return false;
        if (Text.Length >= MaxLength) return false;

        Text = Text.Insert(_caretPosition, c.ToString());
        _caretPosition++;
        OnTextChanged?.Invoke(Text);
        return true;
    }
}
