using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class AnchorLayoutTests
{
    [Fact]
    public void Layout_LeftTop_KeepsPosition()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(AnchorFlags.Top | AnchorFlags.Left, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(0, child.LocalX);   // Padding.Left
        Assert.Equal(0, child.LocalY);   // Padding.Top
    }

    [Fact]
    public void Layout_Right_Bottom_AlignsToEdge()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(AnchorFlags.Right | AnchorFlags.Bottom, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(150, child.LocalX); // 200 - 50
        Assert.Equal(70, child.LocalY);  // 100 - 30
    }

    [Fact]
    public void Layout_HCenter_VCenter_CentersChild()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(AnchorFlags.HCenter | AnchorFlags.VCenter, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(75, child.LocalX);  // (200 - 50) / 2
        Assert.Equal(35, child.LocalY);  // (100 - 30) / 2
    }

    [Fact]
    public void Layout_Stretch_ExpandsToFill()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(AnchorFlags.Stretch, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(0, child.LocalX);
        Assert.Equal(0, child.LocalY);
        Assert.Equal(200, child.Width);
        Assert.Equal(100, child.Height);
    }

    [Fact]
    public void Layout_LeftRight_StretchWidth()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(AnchorFlags.Left | AnchorFlags.Right, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(0, child.LocalX);
        Assert.Equal(200, child.Width);
    }

    [Fact]
    public void Layout_TopBottom_StretchHeight()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(AnchorFlags.Top | AnchorFlags.Bottom, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(0, child.LocalY);
        Assert.Equal(100, child.Height);
    }

    [Fact]
    public void Layout_NoAnchor_KeepsOriginal()
    {
        var container = MakeContainer(200, 100);
        var child = MakeChild(0, 50, 30);
        child.LocalX = 25;
        child.LocalY = 15;
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(25, child.LocalX);
        Assert.Equal(15, child.LocalY);
    }

    [Fact]
    public void Layout_SkipsInvisibleChildren()
    {
        var container = MakeContainer(200, 100);
        var visible = MakeChild(AnchorFlags.Left, 50, 30);
        var invisible = MakeChild(AnchorFlags.Right, 50, 30);
        invisible.Visible = false;
        container.AddChild(visible);
        container.AddChild(invisible);

        container.LayoutEngine!.Layout(container);

        Assert.Equal(0, visible.LocalX);
        Assert.Equal(0, invisible.LocalX); // unchanged (default)
    }

    [Fact]
    public void Layout_WithPadding_OffsetsCorrectly()
    {
        var container = new UIContainer
        {
            Width = 200,
            Height = 100,
            Padding = new Thickness(10, 5, 10, 5),
            LayoutEngine = new AnchorLayout()
        };
        var child = MakeChild(AnchorFlags.Right | AnchorFlags.Bottom, 50, 30);
        container.AddChild(child);

        container.LayoutEngine!.Layout(container);

        // ContentWidth = 200 - 20 = 180, ContentHeight = 100 - 10 = 90
        // cx = Padding.Left = 10, cy = Padding.Top = 5
        Assert.Equal(140, child.LocalX); // 10 + 180 - 50
        Assert.Equal(65, child.LocalY);  // 5 + 90 - 30
    }

    private static UIContainer MakeContainer(float w, float h)
    {
        return new UIContainer
        {
            Width = w,
            Height = h,
            Padding = new Thickness(0),
            LayoutEngine = new AnchorLayout()
        };
    }

    private static UIElement MakeChild(AnchorFlags anchor, float w, float h)
    {
        return new TestElement { Anchor = anchor, Width = w, Height = h };
    }

    private class TestElement : UIElement { }
}
