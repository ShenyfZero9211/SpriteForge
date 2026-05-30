using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class AbsoluteLayoutTests
{
    [Fact]
    public void Measure_RecursesToChildren()
    {
        var container = new UIContainer { LayoutEngine = new AbsoluteLayout() };
        var child = new TestElement { Width = 50, Height = 30 };
        container.AddChild(child);

        container.LayoutEngine.Measure(container);

        Assert.Equal(50, child.DesiredWidth);
        Assert.Equal(30, child.DesiredHeight);
    }

    [Fact]
    public void Layout_DoesNotModifyChildPositions()
    {
        var container = new UIContainer
        {
            Width = 200,
            Height = 100,
            LayoutEngine = new AbsoluteLayout()
        };
        var child = new TestElement { LocalX = 10, LocalY = 20, Width = 50, Height = 30 };
        container.AddChild(child);

        container.LayoutEngine.Layout(container);

        Assert.Equal(10, child.LocalX);
        Assert.Equal(20, child.LocalY);
        Assert.Equal(50, child.Width);
        Assert.Equal(30, child.Height);
    }

    [Fact]
    public void Layout_SkipsInvisibleChildren()
    {
        var container = new UIContainer
        {
            Width = 200,
            Height = 100,
            LayoutEngine = new AbsoluteLayout()
        };
        var visible = new TestElement { LocalX = 10, Width = 50 };
        var invisible = new TestElement { LocalX = 20, Width = 50, Visible = false };
        container.AddChild(visible);
        container.AddChild(invisible);

        container.LayoutEngine.Layout(container);

        Assert.Equal(10, visible.LocalX);
        Assert.Equal(20, invisible.LocalX); // unchanged
    }

    private class TestElement : UIElement { }
}
