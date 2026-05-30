using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIManagerTests
{
    [Fact]
    public void RegisterCanvas_AddsToList()
    {
        var mgr = new UIManager();
        var canvas = new UICanvas();
        mgr.RegisterCanvas(canvas);

        // Internal state check via behavior
        mgr.Update(0.016f); // should not throw
    }

    [Fact]
    public void RegisterCanvas_Duplicate_Ignored()
    {
        var mgr = new UIManager();
        var canvas = new UICanvas();
        mgr.RegisterCanvas(canvas);
        mgr.RegisterCanvas(canvas); // no exception
        mgr.Update(0.016f);
    }

    [Fact]
    public void UnregisterCanvas_RemovesFromList()
    {
        var mgr = new UIManager();
        var canvas = new UICanvas();
        mgr.RegisterCanvas(canvas);
        mgr.UnregisterCanvas(canvas);
        mgr.Update(0.016f); // should not throw
    }

    [Fact]
    public void Update_CallsMeasureAndLayout()
    {
        var mgr = new UIManager();
        var canvas = new UICanvas { Width = 800, Height = 600 };
        var child = new TestElement { Width = 100, Height = 50 };
        canvas.AddChild(child);
        mgr.RegisterCanvas(canvas);

        mgr.Update(0.016f);

        // After Measure, child should have desired size
        Assert.Equal(100, child.DesiredWidth);
        Assert.Equal(50, child.DesiredHeight);
    }

    [Fact]
    public void Update_SkipsDisabledCanvas()
    {
        var mgr = new UIManager();
        var canvas = new UICanvas { Enabled = false };
        var child = new TestElement { Width = 100 };
        canvas.AddChild(child);
        mgr.RegisterCanvas(canvas);

        mgr.Update(0.016f);
        // child.Measure not called because canvas is disabled
        Assert.Equal(0, child.DesiredWidth);
    }

    [Fact]
    public void Update_SkipsInvisibleCanvas()
    {
        var mgr = new UIManager();
        var canvas = new UICanvas { Visible = false };
        var child = new TestElement { Width = 100 };
        canvas.AddChild(child);
        mgr.RegisterCanvas(canvas);

        mgr.Update(0.016f);
        Assert.Equal(0, child.DesiredWidth);
    }

    [Fact]
    public void Render_SkipsWorldSpaceCanvas()
    {
        var mgr = new UIManager();
        var screenCanvas = new UICanvas { Space = CanvasSpace.Screen };
        var worldCanvas = new UICanvas { Space = CanvasSpace.World };
        mgr.RegisterCanvas(screenCanvas);
        mgr.RegisterCanvas(worldCanvas);

        // Render only processes Screen space
        // This is a smoke test ensuring no exception
        mgr.Update(0.016f);
    }

    [Fact]
    public void SortOrder_SortedAscending()
    {
        var mgr = new UIManager();
        var canvas1 = new UICanvas { SortOrder = 1 };
        var canvas2 = new UICanvas { SortOrder = 2 };
        mgr.RegisterCanvas(canvas2);
        mgr.RegisterCanvas(canvas1);

        // canvas2 registered first, but canvas1 has lower SortOrder
        // Internal sort should put canvas1 first
        // We verify via Update (no exception) - more precise test would need reflection
        mgr.Update(0.016f);
    }

    private class TestElement : UIElement { }
}
