using SkiaSharp;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIPanelTests
{
    [Fact]
    public void Default_DrawBackground_IsTrue()
    {
        var panel = new UIPanel();
        Assert.True(panel.DrawBackground);
        Assert.True(panel.DrawBorder);
    }

    [Fact]
    public void BuildDrawList_WithBackground_GeneratesRectCall()
    {
        var panel = new UIPanel
        {
            Width = 100,
            Height = 50,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(40, 40, 50),
                CornerRadius = 4
            }
        };

        var drawList = new UIDrawList();
        panel.BuildDrawList(drawList);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.True(mockGfx.RectCalls.Count > 0);
    }

    [Fact]
    public void BuildDrawList_NoBackground_NoRectCall()
    {
        var panel = new UIPanel
        {
            Width = 100,
            Height = 50,
            DrawBackground = false,
            OverrideStyle = new UIStyle { BackgroundColor = new SKColor(40, 40, 50) }
        };

        var drawList = new UIDrawList();
        panel.BuildDrawList(drawList);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Empty(mockGfx.RectCalls);
    }

    [Fact]
    public void BuildDrawList_ChildrenRendered()
    {
        var panel = new UIPanel
        {
            Width = 100,
            Height = 50,
            OverrideStyle = new UIStyle { BackgroundColor = new SKColor(40, 40, 50) }
        };
        var label = new UILabel { Text = "Inside", OverrideStyle = new UIStyle { FontSize = 12 } };
        panel.AddChild(label);

        var drawList = new UIDrawList();
        panel.BuildDrawList(drawList);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        Assert.Contains(mockGfx.TextCalls, c => c.text == "Inside");
    }
}
