using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UICheckboxTests
{
    [Fact]
    public void DefaultState_IsNotChecked()
    {
        var cb = new UICheckbox();
        Assert.False(cb.IsChecked);
        Assert.True(cb.Focusable);
    }

    [Fact]
    public void Measure_CalculatesSize()
    {
        var cb = new UICheckbox
        {
            Text = "Option",
            OverrideStyle = new UIStyle { FontSize = 14 }
        };
        cb.Measure();

        Assert.True(cb.DesiredWidth > 0);
        Assert.True(cb.DesiredHeight > 0);
    }

    [Fact]
    public void OnEvent_MousePressed_DoesNotToggleYet()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        var consumed = cb.OnEvent(UIEvent.MousePressed(10, 10, 0));

        Assert.True(consumed);
        Assert.False(cb.IsChecked); // 仅在释放时切换
    }

    [Fact]
    public void OnEvent_MouseRelease_TogglesChecked()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        cb.OnEvent(UIEvent.MouseReleased(10, 10, 0));

        Assert.True(cb.IsChecked);
    }

    [Fact]
    public void OnEvent_MouseRelease_TogglesBack()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };
        cb.OnEvent(UIEvent.MouseReleased(10, 10, 0));
        Assert.True(cb.IsChecked);

        cb.OnEvent(UIEvent.MouseReleased(10, 10, 0));

        Assert.False(cb.IsChecked);
    }

    [Fact]
    public void OnEvent_MouseRelease_FiresEvent()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };
        bool? newValue = null;
        cb.OnValueChanged += v => newValue = v;

        cb.OnEvent(UIEvent.MouseReleased(10, 10, 0));

        Assert.True(newValue.HasValue);
        Assert.True(newValue.Value);
    }

    [Fact]
    public void OnEvent_MouseRelease_Outside_DoesNotToggle()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        var consumed = cb.OnEvent(UIEvent.MouseReleased(200, 200, 0));

        Assert.False(consumed);
        Assert.False(cb.IsChecked);
    }

    [Fact]
    public void OnEvent_KeyPressed_Space_Toggles()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        var consumed = cb.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE));

        Assert.True(consumed);
        Assert.True(cb.IsChecked);
    }

    [Fact]
    public void OnEvent_KeyPressed_Return_Toggles()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        var consumed = cb.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_RETURN));

        Assert.True(consumed);
        Assert.True(cb.IsChecked);
    }

    [Fact]
    public void OnEvent_KeyPressed_OtherKey_DoesNotToggle()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        var consumed = cb.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_a));

        Assert.False(consumed);
        Assert.False(cb.IsChecked);
    }

    [Fact]
    public void OnEvent_MouseMove_SetsHover()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };

        cb.OnEvent(UIEvent.MouseMoved(10, 10));

        Assert.True(cb.IsHovered);
    }

    [Fact]
    public void OnEvent_MouseMove_LeavesHover()
    {
        var cb = new UICheckbox { Width = 20, Height = 20 };
        cb.OnEvent(UIEvent.MouseMoved(10, 10));

        cb.OnEvent(UIEvent.MouseMoved(200, 200));

        Assert.False(cb.IsHovered);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var cb = new UICheckbox { Width = 20, Height = 20, Enabled = false };

        var consumed = cb.OnEvent(UIEvent.MouseMoved(10, 10));

        Assert.False(consumed);
        Assert.False(cb.IsHovered);
    }
}
