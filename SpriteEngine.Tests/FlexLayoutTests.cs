using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class FlexLayoutTests
{
    [Fact]
    public void Measure_Row_SumsWidths()
    {
        var container = new UIContainer { Padding = new Thickness(0) };
        container.AddChild(MakeElement(50, 20));
        container.AddChild(MakeElement(30, 20));
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Row };

        container.LayoutEngine.Measure(container);

        Assert.Equal(80, container.DesiredWidth);
        Assert.Equal(20, container.DesiredHeight);
    }

    [Fact]
    public void Measure_Column_SumsHeights()
    {
        var container = new UIContainer { Padding = new Thickness(0) };
        container.AddChild(MakeElement(50, 20));
        container.AddChild(MakeElement(50, 30));
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Column };

        container.LayoutEngine.Measure(container);

        Assert.Equal(50, container.DesiredWidth);
        Assert.Equal(50, container.DesiredHeight);
    }

    [Fact]
    public void Measure_WithGap_AddsGap()
    {
        var container = new UIContainer { Padding = new Thickness(0) };
        container.AddChild(MakeElement(50, 20));
        container.AddChild(MakeElement(50, 20));
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Row, Gap = 10 };

        container.LayoutEngine.Measure(container);

        Assert.Equal(110, container.DesiredWidth); // 50 + 50 + 10
    }

    [Fact]
    public void Measure_WithPadding_AddsPadding()
    {
        var container = new UIContainer { Padding = new Thickness(5, 5, 5, 5) };
        container.AddChild(MakeElement(50, 20));
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Row };

        container.LayoutEngine.Measure(container);

        Assert.Equal(60, container.DesiredWidth);  // 50 + 5 + 5
        Assert.Equal(30, container.DesiredHeight); // 20 + 5 + 5
    }

    [Fact]
    public void Layout_Row_SetsChildPositions()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child1 = MakeElement(50, 20);
        var child2 = MakeElement(50, 20);
        container.AddChild(child1);
        container.AddChild(child2);
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Row };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(0, child1.LocalX);
        Assert.Equal(0, child1.LocalY);
        Assert.Equal(50, child2.LocalX);
        Assert.Equal(0, child2.LocalY);
    }

    [Fact]
    public void Layout_Column_SetsChildPositions()
    {
        var container = new UIContainer { Width = 200, Height = 200, Padding = new Thickness(0) };
        var child1 = MakeElement(50, 20);
        var child2 = MakeElement(50, 30);
        container.AddChild(child1);
        container.AddChild(child2);
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Column };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(0, child1.LocalX);
        Assert.Equal(0, child1.LocalY);
        Assert.Equal(0, child2.LocalX);
        Assert.Equal(20, child2.LocalY);
    }

    [Fact]
    public void Layout_JustifyCenter_CentersChildren()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child = MakeElement(50, 20);
        container.AddChild(child);
        container.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustify.Center
        };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(75, child.LocalX); // (200 - 50) / 2
    }

    [Fact]
    public void Layout_JustifyEnd_AlignsToEnd()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child = MakeElement(50, 20);
        container.AddChild(child);
        container.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustify.End
        };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(150, child.LocalX); // 200 - 50
    }

    [Fact]
    public void Layout_AlignItemsCenter_CentersCrossAxis()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child = MakeElement(50, 20);
        container.AddChild(child);
        container.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Row,
            AlignItems = FlexAlign.Center
        };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(40, child.LocalY); // (100 - 20) / 2
    }

    [Fact]
    public void Layout_AlignItemsEnd_AlignsCrossAxisEnd()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child = MakeElement(50, 20);
        container.AddChild(child);
        container.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Row,
            AlignItems = FlexAlign.End
        };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(80, child.LocalY); // 100 - 20
    }

    [Fact]
    public void Layout_FlexGrow_DistributesExtraSpace()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child1 = MakeElement(50, 20);
        var child2 = MakeElement(50, 20);
        child2.Layout.FlexGrow = 1;
        container.AddChild(child1);
        container.AddChild(child2);
        container.LayoutEngine = new FlexLayout { Direction = FlexDirection.Row };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(50, child1.Width);
        Assert.Equal(150, child2.Width); // 50 + (200 - 100) * 1
    }

    [Fact]
    public void Layout_AlignSelf_OverridesContainer()
    {
        var container = new UIContainer { Width = 200, Height = 100, Padding = new Thickness(0) };
        var child = MakeElement(50, 20);
        child.Layout.AlignSelf = FlexAlign.End;
        container.AddChild(child);
        container.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Row,
            AlignItems = FlexAlign.Start
        };

        container.LayoutEngine.Measure(container);
        container.LayoutEngine.Layout(container);

        Assert.Equal(80, child.LocalY); // AlignSelf=End overrides AlignItems=Start
    }

    private static UIElement MakeElement(float w, float h)
    {
        return new TestElement { Width = w, Height = h };
    }

    private class TestElement : UIElement { }
}
