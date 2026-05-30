using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 滑动条 Widget。支持水平/垂直方向，拖拽调整数值。
/// </summary>
public class UISlider : UIElement
{
    public float MinValue { get; set; } = 0;
    public float MaxValue { get; set; } = 100;
    public float Value { get; set; } = 50;

    /// <summary>值变化时触发。参数为新的 Value。</summary>
    public event Action<float>? OnValueChanged;

    /// <summary>方向</summary>
    public SliderDirection Direction { get; set; } = SliderDirection.Horizontal;

    /// <summary>滑块半径</summary>
    public float HandleRadius { get; set; } = 8;

    /// <summary>轨道厚度</summary>
    public float TrackThickness { get; set; } = 4;

    private bool _isDragging;

    /// <summary>鼠标是否悬停在滑动条上</summary>
    public bool IsHovered { get; private set; }

    public override void Measure()
    {
        if (Direction == SliderDirection.Horizontal)
        {
            DesiredWidth = Width > 0 ? Width : 120;
            DesiredHeight = Math.Max(HandleRadius * 2 + 4, TrackThickness + 4);
        }
        else
        {
            DesiredWidth = Math.Max(HandleRadius * 2 + 4, TrackThickness + 4);
            DesiredHeight = Height > 0 ? Height : 120;
        }
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        float radius = TrackThickness / 2;
        float trackLen = Direction == SliderDirection.Horizontal
            ? Width : Height;
        float ratio = (Value - MinValue) / Math.Max(MaxValue - MinValue, 0.0001f);

        if (Direction == SliderDirection.Horizontal)
        {
            float trackY = AbsoluteY + Height / 2;
            float filledLen = trackLen * ratio;

            // 轨道背景
            drawList.AddRectFilled(AbsoluteX, trackY - radius, trackLen, TrackThickness,
                style.BackgroundColor, radius);

            // 已填充部分
            if (filledLen > 0)
            {
                drawList.AddRectFilled(AbsoluteX, trackY - radius, filledLen, TrackThickness,
                    style.ForegroundColor, radius);
            }

            // 滑块
            float handleX = AbsoluteX + filledLen;
            drawList.AddRectFilled(handleX - HandleRadius, trackY - HandleRadius,
                HandleRadius * 2, HandleRadius * 2,
                IsHovered || _isDragging ? style.HoverColor : style.ForegroundColor,
                HandleRadius);
        }
        else
        {
            float trackX = AbsoluteX + Width / 2;
            float filledLen = trackLen * ratio;

            // 轨道背景
            drawList.AddRectFilled(trackX - radius, AbsoluteY + Height - trackLen,
                TrackThickness, trackLen, style.BackgroundColor, radius);

            // 已填充部分（从底部向上）
            if (filledLen > 0)
            {
                drawList.AddRectFilled(trackX - radius, AbsoluteY + Height - filledLen,
                    TrackThickness, filledLen, style.ForegroundColor, radius);
            }

            // 滑块
            float handleY = AbsoluteY + Height - filledLen;
            drawList.AddRectFilled(trackX - HandleRadius, handleY - HandleRadius,
                HandleRadius * 2, HandleRadius * 2,
                IsHovered || _isDragging ? style.HoverColor : style.ForegroundColor,
                HandleRadius);
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
                {
                    IsHovered = nowHovered;
                }
                if (_isDragging)
                {
                    UpdateValueFromMouse(evt.MouseX, evt.MouseY);
                    return true;
                }
                return nowHovered;

            case UIEventType.MousePressed:
                if (ContainsPoint(evt.MouseX, evt.MouseY))
                {
                    _isDragging = true;
                    UpdateValueFromMouse(evt.MouseX, evt.MouseY);
                    return true;
                }
                break;

            case UIEventType.MouseDragged:
                if (_isDragging)
                {
                    UpdateValueFromMouse(evt.MouseX, evt.MouseY);
                    return true;
                }
                break;

            case UIEventType.MouseReleased:
                if (_isDragging)
                {
                    _isDragging = false;
                    return true;
                }
                break;
        }

        return false;
    }

    private void UpdateValueFromMouse(float mx, float my)
    {
        float ratio;
        if (Direction == SliderDirection.Horizontal)
        {
            ratio = (mx - AbsoluteX) / Math.Max(Width, 0.0001f);
        }
        else
        {
            ratio = 1 - (my - AbsoluteY) / Math.Max(Height, 0.0001f);
        }
        ratio = Math.Clamp(ratio, 0, 1);
        float newValue = MinValue + ratio * (MaxValue - MinValue);
        if (Math.Abs(newValue - Value) > 0.001f)
        {
            Value = newValue;
            OnValueChanged?.Invoke(Value);
        }
    }
}

public enum SliderDirection
{
    Horizontal,
    Vertical
}
