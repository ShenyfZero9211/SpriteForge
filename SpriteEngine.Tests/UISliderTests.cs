using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UISliderTests
{
    [Fact]
    public void DefaultState_HasDefaultValues()
    {
        var slider = new UISlider();
        Assert.Equal(0, slider.MinValue);
        Assert.Equal(100, slider.MaxValue);
        Assert.Equal(50, slider.Value);
        Assert.Equal(SliderDirection.Horizontal, slider.Direction);
        Assert.False(slider.IsHovered);
    }

    [Fact]
    public void Measure_Horizontal_ReturnsReasonableSize()
    {
        var slider = new UISlider
        {
            Direction = SliderDirection.Horizontal,
            Width = 200,
            HandleRadius = 8,
            TrackThickness = 4
        };
        slider.Measure();

        Assert.Equal(200, slider.DesiredWidth);
        Assert.True(slider.DesiredHeight >= 20); // handleRadius * 2 + 4
    }

    [Fact]
    public void Measure_Vertical_ReturnsReasonableSize()
    {
        var slider = new UISlider
        {
            Direction = SliderDirection.Vertical,
            Height = 150,
            HandleRadius = 8,
            TrackThickness = 4
        };
        slider.Measure();

        Assert.True(slider.DesiredWidth >= 20);
        Assert.Equal(150, slider.DesiredHeight);
    }

    [Fact]
    public void OnEvent_MousePressed_SetsValue()
    {
        var slider = new UISlider
        {
            Width = 100, Height = 20,
            LocalX = 0, LocalY = 0,
            Value = 0
        };
        float? changedValue = null;
        slider.OnValueChanged += v => changedValue = v;

        var consumed = slider.OnEvent(UIEvent.MousePressed(50, 10, 0));

        Assert.True(consumed);
        Assert.True(changedValue.HasValue);
        Assert.True(slider.Value > slider.MinValue);
    }

    [Fact]
    public void OnEvent_MousePressed_Outside_DoesNotConsume()
    {
        var slider = new UISlider { Width = 100, Height = 20 };

        var consumed = slider.OnEvent(UIEvent.MousePressed(200, 200, 0));

        Assert.False(consumed);
    }

    [Fact]
    public void OnEvent_Drag_UpdatesValue()
    {
        var slider = new UISlider
        {
            Width = 100, Height = 20,
            LocalX = 0, LocalY = 0,
            Value = 0
        };

        slider.OnEvent(UIEvent.MousePressed(10, 10, 0));
        float firstValue = slider.Value;

        slider.OnEvent(UIEvent.MouseDragged(90, 10, 0));
        float secondValue = slider.Value;

        Assert.True(secondValue > firstValue);
    }

    [Fact]
    public void OnEvent_Drag_Horizontal_ClampedToBounds()
    {
        var slider = new UISlider
        {
            Width = 100, Height = 20,
            LocalX = 0, LocalY = 0,
            Value = 50
        };

        slider.OnEvent(UIEvent.MousePressed(0, 10, 0));
        Assert.Equal(slider.MinValue, slider.Value);

        slider.OnEvent(UIEvent.MouseDragged(200, 10, 0));
        Assert.Equal(slider.MaxValue, slider.Value);
    }

    [Fact]
    public void OnEvent_Drag_Vertical_ClampedToBounds()
    {
        var slider = new UISlider
        {
            Direction = SliderDirection.Vertical,
            Width = 20, Height = 100,
            LocalX = 0, LocalY = 0,
            Value = 50
        };

        slider.OnEvent(UIEvent.MousePressed(10, 99, 0));
        Assert.True(slider.Value < 1); // near bottom ~= min

        slider.OnEvent(UIEvent.MouseDragged(10, -1, 0));
        Assert.True(slider.Value > 99); // near top ~= max
    }

    [Fact]
    public void OnEvent_MouseMove_SetsHover()
    {
        var slider = new UISlider { Width = 100, Height = 20 };

        slider.OnEvent(UIEvent.MouseMoved(50, 10));

        Assert.True(slider.IsHovered);
    }

    [Fact]
    public void OnEvent_MouseMove_LeavesHover()
    {
        var slider = new UISlider { Width = 100, Height = 20 };
        slider.OnEvent(UIEvent.MouseMoved(50, 10));

        slider.OnEvent(UIEvent.MouseMoved(200, 200));

        Assert.False(slider.IsHovered);
    }

    [Fact]
    public void OnEvent_ValueChanged_FiresEvent()
    {
        var slider = new UISlider
        {
            Width = 100, Height = 20,
            LocalX = 0, LocalY = 0,
            Value = 0
        };
        int eventCount = 0;
        slider.OnValueChanged += _ => eventCount++;

        slider.OnEvent(UIEvent.MousePressed(50, 10, 0));

        Assert.True(eventCount > 0);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var slider = new UISlider { Width = 100, Height = 20, Enabled = false };

        var consumed = slider.OnEvent(UIEvent.MouseMoved(50, 10));

        Assert.False(consumed);
        Assert.False(slider.IsHovered);
    }
}
