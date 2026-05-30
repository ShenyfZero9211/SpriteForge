using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

/// <summary>
/// UIManager 键盘事件和焦点导航测试。
/// </summary>
public class UIManagerKeyboardTests
{
    [Fact]
    public void ProcessKeyEvent_Tab_FocusesNext()
    {
        var mgr = new UIManager { IsShiftKeyDown = () => false };
        var canvas = MakeCanvas();
        var btn1 = MakeFocusable("btn1");
        var btn2 = MakeFocusable("btn2");
        canvas.AddChild(btn1);
        canvas.AddChild(btn2);
        mgr.RegisterCanvas(canvas);

        mgr.ProcessKeyEvent(UIEvent.KeyPressed('\t', (int)SDL2.SDL.SDL_Keycode.SDLK_TAB));
        Assert.Equal(btn1, mgr.FocusManager.Focused);

        mgr.ProcessKeyEvent(UIEvent.KeyPressed('\t', (int)SDL2.SDL.SDL_Keycode.SDLK_TAB));
        Assert.Equal(btn2, mgr.FocusManager.Focused);
    }

    [Fact]
    public void ProcessKeyEvent_ShiftTab_FocusesPrevious()
    {
        var mgr = new UIManager { IsShiftKeyDown = () => true };
        var canvas = MakeCanvas();
        var btn1 = MakeFocusable("btn1");
        var btn2 = MakeFocusable("btn2");
        canvas.AddChild(btn1);
        canvas.AddChild(btn2);
        mgr.RegisterCanvas(canvas);

        mgr.FocusManager.SetFocused(btn2);
        Assert.Equal(btn2, mgr.FocusManager.Focused);

        mgr.ProcessKeyEvent(UIEvent.KeyPressed('\t', (int)SDL2.SDL.SDL_Keycode.SDLK_TAB));
        Assert.Equal(btn1, mgr.FocusManager.Focused);
    }

    [Fact]
    public void ProcessKeyEvent_ArrowKey_NavigatesDirection()
    {
        var mgr = new UIManager { IsShiftKeyDown = () => false };
        var canvas = MakeCanvas();
        var top = MakeFocusable("top");
        top.LocalX = 50; top.LocalY = 10; top.Width = 20; top.Height = 20;
        var bottom = MakeFocusable("bottom");
        bottom.LocalX = 50; bottom.LocalY = 100; bottom.Width = 20; bottom.Height = 20;
        canvas.AddChild(top);
        canvas.AddChild(bottom);
        mgr.RegisterCanvas(canvas);

        mgr.FocusManager.SetFocused(top);

        mgr.ProcessKeyEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_DOWN));

        Assert.Equal(bottom, mgr.FocusManager.Focused);
    }

    [Fact]
    public void ProcessKeyEvent_NonNavigation_ForwardsToFocused()
    {
        var mgr = new UIManager { IsShiftKeyDown = () => false };
        var canvas = MakeCanvas();
        var btn = MakeFocusable("btn");
        canvas.AddChild(btn);
        mgr.RegisterCanvas(canvas);

        mgr.FocusManager.SetFocused(btn);

        // Space key (non-navigation) - should reach focused element without crashing
        mgr.ProcessKeyEvent(UIEvent.KeyPressed(' ', (int)SDL2.SDL.SDL_Keycode.SDLK_SPACE));

        Assert.Equal(btn, mgr.FocusManager.Focused);
    }

    [Fact]
    public void ProcessKeyEvent_NoCanvas_DoesNotThrow()
    {
        var mgr = new UIManager { IsShiftKeyDown = () => false };
        mgr.ProcessKeyEvent(UIEvent.KeyPressed('\t', (int)SDL2.SDL.SDL_Keycode.SDLK_TAB));
        Assert.Null(mgr.FocusManager.Focused);
    }

    private static UICanvas MakeCanvas()
    {
        return new UICanvas
        {
            Width = 800,
            Height = 600,
            Space = CanvasSpace.Screen
        };
    }

    private static UIElement MakeFocusable(string name)
    {
        return new TestElement
        {
            Focusable = true,
            Width = 50,
            Height = 30
        };
    }

    private class TestElement : UIElement { }
}
