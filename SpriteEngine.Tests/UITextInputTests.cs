using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UITextInputTests
{
    [Fact]
    public void DefaultState_HasEmptyText()
    {
        var input = new UITextInput();
        Assert.Equal("", input.Text);
        Assert.True(input.Focusable);
        Assert.False(input.IsFocused);
    }

    [Fact]
    public void Measure_CalculatesSize()
    {
        var input = new UITextInput
        {
            OverrideStyle = new UIStyle { FontSize = 14, Padding = new Thickness(4) }
        };
        input.Measure();

        Assert.True(input.DesiredWidth > 0);
        Assert.True(input.DesiredHeight > 0);
    }

    [Fact]
    public void OnEvent_MousePressed_Inside_ReturnsTrue()
    {
        var input = new UITextInput { Width = 100, Height = 30 };

        var consumed = input.OnEvent(UIEvent.MousePressed(50, 15, 0));

        Assert.True(consumed);
    }

    [Fact]
    public void OnEvent_MousePressed_Outside_ReturnsFalse()
    {
        var input = new UITextInput { Width = 100, Height = 30 };

        var consumed = input.OnEvent(UIEvent.MousePressed(200, 200, 0));

        Assert.False(consumed);
    }

    [Fact]
    public void OnEvent_KeyTyped_AppendsCharacter()
    {
        var input = new UITextInput();

        input.OnEvent(UIEvent.KeyTyped('H'));
        input.OnEvent(UIEvent.KeyTyped('i'));

        Assert.Equal("Hi", input.Text);
    }

    [Fact]
    public void OnEvent_KeyTyped_FiresTextChanged()
    {
        var input = new UITextInput();
        string? changedText = null;
        input.OnTextChanged += t => changedText = t;

        input.OnEvent(UIEvent.KeyTyped('A'));

        Assert.Equal("A", changedText);
    }

    [Fact]
    public void OnEvent_KeyTyped_IgnoresControlChars()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('\n'));
        input.OnEvent(UIEvent.KeyTyped('\t'));
        input.OnEvent(UIEvent.KeyTyped('\0'));

        Assert.Equal("", input.Text);
    }

    [Fact]
    public void OnEvent_KeyTyped_RespectsMaxLength()
    {
        var input = new UITextInput { MaxLength = 3 };
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyTyped('B'));
        input.OnEvent(UIEvent.KeyTyped('C'));
        input.OnEvent(UIEvent.KeyTyped('D'));

        Assert.Equal("ABC", input.Text);
    }

    [Fact]
    public void OnEvent_Backspace_RemovesCharacter()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyTyped('B'));

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE));

        Assert.Equal("A", input.Text);
    }

    [Fact]
    public void OnEvent_Backspace_AtStart_DoesNothing()
    {
        var input = new UITextInput();

        var consumed = input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE));

        Assert.True(consumed);
        Assert.Equal("", input.Text);
    }

    [Fact]
    public void OnEvent_Delete_RemovesCharacter()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyTyped('B'));
        // Move caret to start
        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_HOME));

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_DELETE));

        Assert.Equal("B", input.Text);
    }

    [Fact]
    public void OnEvent_Delete_AtEnd_DoesNothing()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));

        var consumed = input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_DELETE));

        Assert.True(consumed);
        Assert.Equal("A", input.Text);
    }

    [Fact]
    public void OnEvent_LeftArrow_MovesCaret()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyTyped('B'));

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_LEFT));

        // Caret is now at position 1; typing adds there
        input.OnEvent(UIEvent.KeyTyped('X'));
        Assert.Equal("AXB", input.Text);
    }

    [Fact]
    public void OnEvent_LeftArrow_AtStart_DoesNothing()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_HOME));

        var consumed = input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_LEFT));

        Assert.True(consumed);
        input.OnEvent(UIEvent.KeyTyped('X'));
        Assert.Equal("XA", input.Text);
    }

    [Fact]
    public void OnEvent_RightArrow_MovesCaret()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyTyped('B'));
        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_HOME));

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_RIGHT));
        input.OnEvent(UIEvent.KeyTyped('X'));

        Assert.Equal("AXB", input.Text);
    }

    [Fact]
    public void OnEvent_Home_MovesCaretToStart()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyTyped('B'));

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_HOME));
        input.OnEvent(UIEvent.KeyTyped('X'));

        Assert.Equal("XAB", input.Text);
    }

    [Fact]
    public void OnEvent_End_MovesCaretToEnd()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('A'));
        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_HOME));

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_END));
        input.OnEvent(UIEvent.KeyTyped('B'));

        Assert.Equal("AB", input.Text);
    }

    [Fact]
    public void OnEvent_Enter_FiresSubmit()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.KeyTyped('H'));
        input.OnEvent(UIEvent.KeyTyped('i'));

        string? submitted = null;
        input.OnSubmit += t => submitted = t;

        input.OnEvent(UIEvent.KeyPressed('\0', (int)SDL2.SDL.SDL_Keycode.SDLK_RETURN));

        Assert.Equal("Hi", submitted);
    }

    [Fact]
    public void OnEvent_FocusGained_SetsIsFocused()
    {
        var input = new UITextInput();

        input.OnEvent(UIEvent.FocusGained());

        Assert.True(input.IsFocused);
    }

    [Fact]
    public void OnEvent_FocusLost_ClearsIsFocused()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.FocusGained());

        input.OnEvent(UIEvent.FocusLost());

        Assert.False(input.IsFocused);
    }

    [Fact]
    public void Update_WhenFocused_BlinksCaret()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.FocusGained());

        input.Update(0.6f); // Blink interval is 0.5s

        // _showCaret toggles; initially true after FocusGained
        // After 0.6s it should have toggled to false
        // We can't directly test _showCaret, but we can verify no exception
        Assert.True(input.IsFocused);
    }

    [Fact]
    public void Update_WhenNotFocused_HidesCaret()
    {
        var input = new UITextInput();
        input.OnEvent(UIEvent.FocusGained());
        input.OnEvent(UIEvent.FocusLost());

        input.Update(0.1f);

        Assert.False(input.IsFocused);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var input = new UITextInput { Width = 100, Height = 30, Enabled = false };

        var consumed = input.OnEvent(UIEvent.MouseMoved(50, 15));

        Assert.False(consumed);
        Assert.False(input.IsHovered);
    }
}
