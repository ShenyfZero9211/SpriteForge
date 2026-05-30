using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIFocusManagerTests
{
    private class FocusableElement : UIElement
    {
        public bool FocusGainedCalled = false;
        public bool FocusLostCalled = false;

        public FocusableElement()
        {
            Focusable = true;
        }

        public override bool OnEvent(UIEvent evt)
        {
            if (evt.Type == UIEventType.FocusGained) FocusGainedCalled = true;
            if (evt.Type == UIEventType.FocusLost) FocusLostCalled = true;
            return base.OnEvent(evt);
        }
    }

    [Fact]
    public void SetFocus_SendsEvents()
    {
        var mgr = new UIFocusManager();
        var el = new FocusableElement();

        mgr.SetFocused(el);

        Assert.True(el.FocusGainedCalled);
        Assert.Equal(el, mgr.Focused);
    }

    [Fact]
    public void SetFocus_SameElement_NoEvent()
    {
        var mgr = new UIFocusManager();
        var el = new FocusableElement();
        mgr.SetFocused(el);
        el.FocusGainedCalled = false;

        mgr.SetFocused(el);

        Assert.False(el.FocusGainedCalled);
    }

    [Fact]
    public void SetFocus_SwitchesFocus_SendsLostAndGained()
    {
        var mgr = new UIFocusManager();
        var el1 = new FocusableElement();
        var el2 = new FocusableElement();

        mgr.SetFocused(el1);
        mgr.SetFocused(el2);

        Assert.True(el1.FocusLostCalled);
        Assert.True(el2.FocusGainedCalled);
        Assert.Equal(el2, mgr.Focused);
    }

    [Fact]
    public void ClearFocus_SendsLost()
    {
        var mgr = new UIFocusManager();
        var el = new FocusableElement();
        mgr.SetFocused(el);

        mgr.ClearFocus();

        Assert.True(el.FocusLostCalled);
        Assert.Null(mgr.Focused);
    }

    [Fact]
    public void FocusNext_CyclesForward()
    {
        var root = new UIContainer();
        var el1 = new FocusableElement();
        var el2 = new FocusableElement();
        root.AddChild(el1);
        root.AddChild(el2);

        var mgr = new UIFocusManager();
        mgr.FocusNext(root);
        Assert.Equal(el1, mgr.Focused);

        mgr.FocusNext(root);
        Assert.Equal(el2, mgr.Focused);

        mgr.FocusNext(root);
        Assert.Equal(el1, mgr.Focused); // cycles back
    }

    [Fact]
    public void FocusPrevious_CyclesBackward()
    {
        var root = new UIContainer();
        var el1 = new FocusableElement();
        var el2 = new FocusableElement();
        root.AddChild(el1);
        root.AddChild(el2);

        var mgr = new UIFocusManager();
        mgr.FocusPrevious(root);
        Assert.Equal(el2, mgr.Focused); // starts from last when no focus

        mgr.FocusPrevious(root);
        Assert.Equal(el1, mgr.Focused);
    }

    [Fact]
    public void FocusNext_SkipsInvisible()
    {
        var root = new UIContainer();
        var visible = new FocusableElement();
        var invisible = new FocusableElement { Visible = false };
        root.AddChild(visible);
        root.AddChild(invisible);

        var mgr = new UIFocusManager();
        mgr.FocusNext(root);
        Assert.Equal(visible, mgr.Focused);

        mgr.FocusNext(root);
        Assert.Equal(visible, mgr.Focused); // invisible skipped, cycles back
    }

    [Fact]
    public void FocusNext_SkipsDisabled()
    {
        var root = new UIContainer();
        var enabled = new FocusableElement();
        var disabled = new FocusableElement { Enabled = false };
        root.AddChild(enabled);
        root.AddChild(disabled);

        var mgr = new UIFocusManager();
        mgr.FocusNext(root);
        Assert.Equal(enabled, mgr.Focused);

        mgr.FocusNext(root);
        Assert.Equal(enabled, mgr.Focused); // disabled skipped
    }

    [Fact]
    public void FocusDirection_Right_FindsRightmost()
    {
        var root = new UIContainer();
        var left = new FocusableElement { LocalX = 0, LocalY = 0, Width = 10, Height = 10 };
        var right = new FocusableElement { LocalX = 100, LocalY = 0, Width = 10, Height = 10 };
        root.AddChild(left);
        root.AddChild(right);

        var mgr = new UIFocusManager();
        mgr.SetFocused(left);
        mgr.FocusDirection(root, UINavigationDirection.Right);

        Assert.Equal(right, mgr.Focused);
    }

    [Fact]
    public void FocusDirection_Down_FindsBelow()
    {
        var root = new UIContainer();
        var top = new FocusableElement { LocalX = 0, LocalY = 0, Width = 10, Height = 10 };
        var bottom = new FocusableElement { LocalX = 0, LocalY = 100, Width = 10, Height = 10 };
        root.AddChild(top);
        root.AddChild(bottom);

        var mgr = new UIFocusManager();
        mgr.SetFocused(top);
        mgr.FocusDirection(root, UINavigationDirection.Down);

        Assert.Equal(bottom, mgr.Focused);
    }

    [Fact]
    public void FocusDirection_NoFocus_CallsFocusNext()
    {
        var root = new UIContainer();
        var el = new FocusableElement();
        root.AddChild(el);

        var mgr = new UIFocusManager();
        mgr.FocusDirection(root, UINavigationDirection.Right);

        Assert.Equal(el, mgr.Focused);
    }

    [Fact]
    public void FocusDirection_WrongDirection_KeepsFocus()
    {
        var root = new UIContainer();
        var left = new FocusableElement { LocalX = 0, LocalY = 0, Width = 10, Height = 10 };
        var right = new FocusableElement { LocalX = 100, LocalY = 0, Width = 10, Height = 10 };
        root.AddChild(left);
        root.AddChild(right);

        var mgr = new UIFocusManager();
        mgr.SetFocused(right);
        mgr.FocusDirection(root, UINavigationDirection.Right); // nothing to the right of right

        Assert.Equal(right, mgr.Focused);
    }
}
