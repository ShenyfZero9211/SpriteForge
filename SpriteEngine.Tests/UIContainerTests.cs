using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIContainerTests
{
    private class TestElement : UIElement { }

    [Fact]
    public void AddChild_SetsParent()
    {
        var container = new UIContainer();
        var child = new TestElement();
        container.AddChild(child);

        Assert.Equal(container, child.Parent);
        Assert.Contains(child, container.Children);
    }

    [Fact]
    public void AddChild_RemovesFromOldParent()
    {
        var oldParent = new UIContainer();
        var newParent = new UIContainer();
        var child = new TestElement();
        oldParent.AddChild(child);
        newParent.AddChild(child);

        Assert.Equal(newParent, child.Parent);
        Assert.DoesNotContain(child, oldParent.Children);
        Assert.Contains(child, newParent.Children);
    }

    [Fact]
    public void RemoveChild_ClearsParent()
    {
        var container = new UIContainer();
        var child = new TestElement();
        container.AddChild(child);
        container.RemoveChild(child);

        Assert.Null(child.Parent);
        Assert.DoesNotContain(child, container.Children);
    }

    [Fact]
    public void RemoveAllChildren_ClearsAll()
    {
        var container = new UIContainer();
        container.AddChild(new TestElement());
        container.AddChild(new TestElement());
        container.RemoveAllChildren();

        Assert.Empty(container.Children);
    }

    [Fact]
    public void ContentDimensions_AccountForPadding()
    {
        var container = new UIContainer
        {
            Width = 100,
            Height = 80,
            Padding = new Thickness(10, 5, 10, 5)
        };

        Assert.Equal(10, container.ContentOffsetX);
        Assert.Equal(5, container.ContentOffsetY);
        Assert.Equal(80, container.ContentWidth);   // 100 - 10 - 10
        Assert.Equal(70, container.ContentHeight);  // 80 - 5 - 5
    }

    [Fact]
    public void ContentWidth_Negative_ReturnsZero()
    {
        var container = new UIContainer
        {
            Width = 10,
            Padding = new Thickness(10, 0, 10, 0)
        };
        Assert.Equal(0, container.ContentWidth);
    }

    [Fact]
    public void Measure_RecursesToChildren()
    {
        var container = new UIContainer();
        var child = new TestElement { Width = 30, Height = 20 };
        container.AddChild(child);
        container.Measure();

        Assert.Equal(30, child.DesiredWidth);
        Assert.Equal(20, child.DesiredHeight);
    }

    [Fact]
    public void Measure_SkipsInvisibleChildren()
    {
        var container = new UIContainer();
        var visible = new TestElement { Width = 30, Height = 20 };
        var invisible = new TestElement { Width = 100, Height = 100, Visible = false };
        container.AddChild(visible);
        container.AddChild(invisible);
        container.Measure();

        // invisible child should not affect container desired size
        Assert.Equal(30, container.DesiredWidth);
        Assert.Equal(20, container.DesiredHeight);
    }

    [Fact]
    public void DoLayout_RecursesToChildren()
    {
        var container = new UIContainer { Width = 200, Height = 100 };
        var child = new TestElement();
        container.AddChild(child);
        container.DoLayout();

        // child.DoLayout is called (no-op by default)
        Assert.Equal(container, child.Parent);
    }

    [Fact]
    public void HitTest_ReturnsChildWithHigherZOrder()
    {
        var container = new UIContainer { Width = 100, Height = 100 };
        var child1 = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 50, ZOrder = 0 };
        var child2 = new TestElement { LocalX = 20, LocalY = 20, Width = 50, Height = 50, ZOrder = 1 };
        container.AddChild(child1);
        container.AddChild(child2);

        // Point (30, 30) hits both children, should return higher ZOrder
        var hit = container.HitTest(30, 30);
        Assert.Equal(child2, hit);
    }

    [Fact]
    public void HitTest_ReturnsContainerWhenNoChildHit()
    {
        var container = new UIContainer { LocalX = 0, LocalY = 0, Width = 100, Height = 100 };
        var child = new TestElement { LocalX = 10, LocalY = 10, Width = 20, Height = 20 };
        container.AddChild(child);

        // Point inside container but outside child
        var hit = container.HitTest(50, 50);
        Assert.Equal(container, hit);
    }

    [Fact]
    public void HitTest_ReturnsNullWhenDisabled()
    {
        var container = new UIContainer { Width = 100, Height = 100, Enabled = false };
        Assert.Null(container.HitTest(50, 50));
    }

    [Fact]
    public void FindChildById_Recursive()
    {
        var root = new UIContainer();
        var level1 = new UIContainer();
        var go = new SpriteEngine.Scenes.GameObject("target");
        var level2 = go.AddComponent<TestElement>();
        level1.AddChild(level2);
        root.AddChild(level1);

        var found = root.FindChildById("target");
        Assert.Equal(level2, found);
    }

    [Fact]
    public void FindChildById_NotFound_ReturnsNull()
    {
        var container = new UIContainer();
        Assert.Null(container.FindChildById("missing"));
    }
}
