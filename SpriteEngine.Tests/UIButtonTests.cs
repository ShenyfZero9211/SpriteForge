using SkiaSharp;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIButtonTests
{
    [Fact]
    public void DefaultState_IsNotHoveredOrPressed()
    {
        var btn = new UIButton { Text = "Click" };
        Assert.False(btn.IsHovered);
        Assert.False(btn.IsPressed);
    }

    [Fact]
    public void Measure_CalculatesSize()
    {
        var btn = new UIButton
        {
            Text = "Hello",
            OverrideStyle = new UIStyle { FontSize = 14, Padding = new Thickness(8) }
        };
        btn.Measure();

        Assert.True(btn.DesiredWidth > 0);
        Assert.True(btn.DesiredHeight > 0);
    }

    [Fact]
    public void OnEvent_MouseMove_EntersHover()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        bool entered = false;
        btn.OnHoverEnter += () => entered = true;

        var consumed = btn.OnEvent(UIEvent.MouseMoved(50, 20));

        Assert.True(btn.IsHovered);
        Assert.True(entered);
        Assert.True(consumed);
    }

    [Fact]
    public void OnEvent_MouseMove_LeavesHover()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // enter

        bool exited = false;
        btn.OnHoverExit += () => exited = true;

        var consumed = btn.OnEvent(UIEvent.MouseMoved(200, 200)); // leave

        Assert.False(btn.IsHovered);
        Assert.True(exited);
        Assert.False(consumed);
    }

    [Fact]
    public void OnEvent_MousePress_SetsPressed()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // hover
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0));

        Assert.True(btn.IsPressed);
    }

    [Fact]
    public void OnEvent_MouseRelease_Outside_DoesNotClick()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        bool clicked = false;
        btn.OnClick += () => clicked = true;

        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // hover
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0)); // press
        btn.OnEvent(UIEvent.MouseDragged(200, 200, 0)); // drag outside
        btn.OnEvent(UIEvent.MouseReleased(200, 200, 0)); // release outside

        Assert.False(clicked);
        Assert.False(btn.IsPressed);
    }

    [Fact]
    public void OnEvent_MouseRelease_Inside_TriggersClick()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        bool clicked = false;
        btn.OnClick += () => clicked = true;

        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // hover
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0));
        btn.OnEvent(UIEvent.MouseReleased(50, 20, 0));

        Assert.True(clicked);
        Assert.False(btn.IsPressed);
    }

    [Fact]
    public void OnEvent_MouseDrag_Outside_KeepsPressed()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        bool exited = false;
        btn.OnHoverExit += () => exited = true;

        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // hover
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0)); // press
        Assert.True(btn.IsPressed);

        btn.OnEvent(UIEvent.MouseDragged(200, 200, 0)); // drag outside

        Assert.False(btn.IsHovered);
        Assert.True(btn.IsPressed); // pressed 保持
        Assert.True(exited);
    }

    [Fact]
    public void OnEvent_MouseDrag_BackInside_RestoresHover()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        bool entered = false;
        btn.OnHoverEnter += () => entered = true;

        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // hover
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0)); // press
        btn.OnEvent(UIEvent.MouseDragged(200, 200, 0)); // drag outside
        entered = false;

        btn.OnEvent(UIEvent.MouseDragged(50, 20, 0)); // drag back inside

        Assert.True(btn.IsHovered);
        Assert.True(btn.IsPressed); // pressed 保持
        Assert.True(entered);
    }

    [Fact]
    public void OnEvent_MouseRelease_OutsideAfterDrag_DoesNotClick()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40 };
        bool clicked = false;
        btn.OnClick += () => clicked = true;

        btn.OnEvent(UIEvent.MouseMoved(50, 20)); // hover
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0)); // press
        btn.OnEvent(UIEvent.MouseDragged(200, 200, 0)); // drag outside
        btn.OnEvent(UIEvent.MouseReleased(200, 200, 0)); // release outside

        Assert.False(btn.IsPressed);
        Assert.False(clicked);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40, Enabled = false };
        var consumed = btn.OnEvent(UIEvent.MouseMoved(50, 20));
        Assert.False(consumed);
        Assert.False(btn.IsHovered);
    }

    [Fact]
    public void Disabled_VisualUsesDisabledColor()
    {
        var btn = new UIButton
        {
            Text = "Test",
            Enabled = false,
            OverrideStyle = new UIStyle { DisabledColor = new SKColor(100, 100, 100) }
        };

        var method = typeof(UIButton).GetMethod("ResolveBackgroundColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var color = (SKColor)method!.Invoke(btn, null)!;
        Assert.Equal(new SKColor(100, 100, 100), color);
    }

    [Fact]
    public void OnEvent_Disabled_ReturnsFalse()
    {
        var btn = new UIButton { Text = "Test", Width = 100, Height = 40, Enabled = false };
        var consumed = btn.OnEvent(UIEvent.MouseMoved(50, 20));
        Assert.False(consumed);
        Assert.False(btn.IsHovered);
    }

    [Fact]
    public void ResolveBackgroundColor_Normal_UsesBackgroundColor()
    {
        var btn = new UIButton
        {
            OverrideStyle = new UIStyle { BackgroundColor = new SKColor(10, 20, 30) }
        };
        // Use reflection to test private method
        var method = typeof(UIButton).GetMethod("ResolveBackgroundColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var color = (SKColor)method!.Invoke(btn, null)!;
        Assert.Equal(new SKColor(10, 20, 30), color);
    }

    [Fact]
    public void ResolveBackgroundColor_Hover_UsesHoverColor()
    {
        var btn = new UIButton
        {
            Width = 100, Height = 40,
            OverrideStyle = new UIStyle { HoverColor = new SKColor(50, 60, 70) }
        };
        btn.OnEvent(UIEvent.MouseMoved(50, 20));

        var method = typeof(UIButton).GetMethod("ResolveBackgroundColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var color = (SKColor)method!.Invoke(btn, null)!;
        Assert.Equal(new SKColor(50, 60, 70), color);
    }

    [Fact]
    public void ResolveBackgroundColor_Pressed_UsesPressedColor()
    {
        var btn = new UIButton
        {
            Width = 100, Height = 40,
            OverrideStyle = new UIStyle { PressedColor = new SKColor(80, 90, 100) }
        };
        btn.OnEvent(UIEvent.MouseMoved(50, 20));
        btn.OnEvent(UIEvent.MousePressed(50, 20, 0));

        var method = typeof(UIButton).GetMethod("ResolveBackgroundColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var color = (SKColor)method!.Invoke(btn, null)!;
        Assert.Equal(new SKColor(80, 90, 100), color);
    }

    [Fact]
    public void ResolveBackgroundColor_Disabled_UsesDisabledColor()
    {
        var btn = new UIButton
        {
            Enabled = false,
            OverrideStyle = new UIStyle { DisabledColor = new SKColor(100, 100, 100) }
        };

        var method = typeof(UIButton).GetMethod("ResolveBackgroundColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var color = (SKColor)method!.Invoke(btn, null)!;
        Assert.Equal(new SKColor(100, 100, 100), color);
    }

    [Fact]
    public void CustomColors_OverrideResolvedStyle()
    {
        var btn = new UIButton
        {
            NormalColor = new SKColor(255, 0, 0),
            OverrideStyle = new UIStyle { BackgroundColor = new SKColor(0, 0, 0) }
        };

        var method = typeof(UIButton).GetMethod("ResolveBackgroundColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var color = (SKColor)method!.Invoke(btn, null)!;
        Assert.Equal(new SKColor(255, 0, 0), color);
    }

    [Fact]
    public void Default_Focusable_IsTrue()
    {
        var btn = new UIButton();
        Assert.True(btn.Focusable);
    }

    [Fact]
    public void FocusGained_SetsIsFocused()
    {
        var btn = new UIButton();
        btn.OnEvent(UIEvent.FocusGained());
        Assert.True(btn.IsFocused);
    }

    [Fact]
    public void FocusLost_ClearsIsFocused()
    {
        var btn = new UIButton();
        btn.OnEvent(UIEvent.FocusGained());
        btn.OnEvent(UIEvent.FocusLost());
        Assert.False(btn.IsFocused);
    }

    [Fact]
    public void KeyPressed_Space_SetsPressed()
    {
        var btn = new UIButton { Text = "OK" };
        btn.OnEvent(UIEvent.FocusGained());

        var consumed = btn.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE));

        Assert.True(consumed);
        Assert.True(btn.IsPressed);
    }

    [Fact]
    public void KeyReleased_Space_TriggersClick()
    {
        var btn = new UIButton { Text = "OK" };
        bool clicked = false;
        btn.OnClick += () => clicked = true;
        btn.OnEvent(UIEvent.FocusGained());
        btn.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE));

        var consumed = btn.OnEvent(UIEvent.KeyReleased('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE));

        Assert.True(consumed);
        Assert.True(clicked);
        Assert.False(btn.IsPressed);
    }

    [Fact]
    public void KeyPressed_Return_SetsPressed()
    {
        var btn = new UIButton { Text = "OK" };
        btn.OnEvent(UIEvent.FocusGained());

        var consumed = btn.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_RETURN));

        Assert.True(consumed);
        Assert.True(btn.IsPressed);
    }

    [Fact]
    public void FocusLost_ClearsPressedState()
    {
        var btn = new UIButton { Text = "OK" };
        btn.OnEvent(UIEvent.FocusGained());
        btn.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE));
        Assert.True(btn.IsPressed);

        btn.OnEvent(UIEvent.FocusLost());

        Assert.False(btn.IsPressed);
        Assert.False(btn.IsFocused);
    }
}
