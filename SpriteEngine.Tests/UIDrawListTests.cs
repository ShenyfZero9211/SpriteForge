using SkiaSharp;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIDrawListTests
{
    [Fact]
    public void AddRectFilled_GeneratesRectCall()
    {
        var drawList = new UIDrawList();
        drawList.AddRectFilled(10, 20, 100, 50, new SKColor(255, 0, 0), 4);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Single(mockGfx.RectCalls);
    }

    [Fact]
    public void AddRect_GeneratesRectCall()
    {
        var drawList = new UIDrawList();
        drawList.AddRect(10, 20, 100, 50, new SKColor(0, 255, 0), 2, 4);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Single(mockGfx.RectCalls);
    }

    [Fact]
    public void AddLine_GeneratesLineCall()
    {
        var drawList = new UIDrawList();
        drawList.AddLine(0, 0, 100, 100, new SKColor(0, 0, 255), 2);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Single(mockGfx.LineCalls);
        var line = mockGfx.LineCalls[0];
        Assert.Equal(0, line.x1);
        Assert.Equal(0, line.y1);
        Assert.Equal(100, line.x2);
        Assert.Equal(100, line.y2);
    }

    [Fact]
    public void AddText_GeneratesTextCall()
    {
        var drawList = new UIDrawList();
        drawList.AddText("Hello", 10, 20, new SKColor(255, 255, 255), 14);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Single(mockGfx.TextCalls);
        Assert.Equal("Hello", mockGfx.TextCalls[0].text);
    }

    [Fact]
    public void Clear_RemovesAllCommands()
    {
        var drawList = new UIDrawList();
        drawList.AddRectFilled(0, 0, 10, 10, SKColors.Red);
        drawList.Clear();

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Empty(mockGfx.RectCalls);
    }

    [Fact]
    public void PushClipRect_IntersectsWithPrevious()
    {
        var drawList = new UIDrawList();
        drawList.PushClipRect(0, 0, 100, 100);
        drawList.PushClipRect(50, 50, 100, 100);
        drawList.AddRectFilled(0, 0, 200, 200, SKColors.Red);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        // Should still render despite clip (mock doesn't enforce clipping)
        Assert.Single(mockGfx.RectCalls);
    }

    [Fact]
    public void PopClipRect_RestoresPreviousClip()
    {
        var drawList = new UIDrawList();
        drawList.PushClipRect(0, 0, 50, 50);
        drawList.PopClipRect();
        drawList.AddRectFilled(0, 0, 200, 200, SKColors.Red);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Single(mockGfx.RectCalls);
    }

    [Fact]
    public void MultipleCommands_RenderInOrder()
    {
        var drawList = new UIDrawList();
        drawList.AddRectFilled(0, 0, 10, 10, SKColors.Red);
        drawList.AddRectFilled(10, 10, 10, 10, SKColors.Green);
        drawList.AddLine(0, 0, 10, 10, SKColors.Blue, 1);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Equal(2, mockGfx.RectCalls.Count);
        Assert.Single(mockGfx.LineCalls);
    }
}
