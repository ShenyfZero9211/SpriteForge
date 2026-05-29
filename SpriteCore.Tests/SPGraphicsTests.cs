using SkiaSharp;
using SpriteCore.Graphics;
using Xunit;

namespace SpriteCore.Tests;

public class SPGraphicsTests
{
    private class TestGraphics : SPGraphics
    {
        public override void BeginFrame() { }
        public override void EndFrame() { }
        public override void Resize(int width, int height) { Width = width; Height = height; }
        public override void Background(float r, float g, float b, float a) { }
        public override void Rect(float x, float y, float w, float h) { }
        public override void Ellipse(float x, float y, float w, float h) { }
        public override void Circle(float x, float y, float r) { }
        public override void Line(float x1, float y1, float x2, float y2) { }
        public override void Triangle(float x1, float y1, float x2, float y2, float x3, float y3) { }
        public override void TextSize(float size) { }
        public override void Text(string str, float x, float y) { }
        public override void Image(SKBitmap bitmap, float x, float y) { }
        public override void Image(SPTexture texture, float x, float y) { }
        public override void Image(SPTexture texture, float x, float y, float w, float h) { }
        public override void Present(IntPtr sdlRenderer, IntPtr sdlTexture) { }
        public SKMatrix GetCurrentMatrix() => CurrentMatrix.ToSkMatrix();

        public SPStyle ExposeStyle() => CurrentStyle;
        public void ExposeFill(float gray) => Fill(gray);
        public void ExposeFill(float r, float g, float b, float a = 255) => Fill(r, g, b, a);
        public void ExposeStroke(float gray) => Stroke(gray);
        public void ExposeStroke(float r, float g, float b, float a = 255) => Stroke(r, g, b, a);
        public void ExposeStrokeWeight(float w) => StrokeWeight(w);
        public void ExposeNoFill() => NoFill();
        public void ExposeNoStroke() => NoStroke();
    }

    [Fact]
    public void DefaultStyle_HasCorrectValues()
    {
        var g = new TestGraphics();
        var s = g.ExposeStyle();

        Assert.True(s.Fill);
        Assert.Equal(SKColors.White, s.FillColor);
        Assert.True(s.Stroke);
        Assert.Equal(SKColors.Black, s.StrokeColor);
        Assert.Equal(1, s.StrokeWeight);
    }

    [Fact]
    public void Fill_UpdatesFillColor()
    {
        var g = new TestGraphics();
        g.ExposeFill(255, 128, 64);

        var s = g.ExposeStyle();
        Assert.True(s.Fill);
        Assert.Equal(new SKColor(255, 128, 64), s.FillColor);
    }

    [Fact]
    public void FillGray_UpdatesFillColor()
    {
        var g = new TestGraphics();
        g.ExposeFill(128);

        var s = g.ExposeStyle();
        Assert.Equal(new SKColor(128, 128, 128), s.FillColor);
    }

    [Fact]
    public void NoFill_DisablesFill()
    {
        var g = new TestGraphics();
        g.ExposeNoFill();

        var s = g.ExposeStyle();
        Assert.False(s.Fill);
    }

    [Fact]
    public void NoStroke_DisablesStroke()
    {
        var g = new TestGraphics();
        g.ExposeNoStroke();

        var s = g.ExposeStyle();
        Assert.False(s.Stroke);
    }

    [Fact]
    public void StrokeWeight_Updates()
    {
        var g = new TestGraphics();
        g.ExposeStrokeWeight(5.5f);

        Assert.Equal(5.5f, g.ExposeStyle().StrokeWeight);
    }

    [Fact]
    public void PushPopStyle_IsolatesChanges()
    {
        var g = new TestGraphics();

        g.ExposeFill(255, 0, 0);
        g.PushStyle();
        g.ExposeFill(0, 255, 0);
        g.ExposeStrokeWeight(10);

        Assert.Equal(new SKColor(0, 255, 0), g.ExposeStyle().FillColor);
        Assert.Equal(10, g.ExposeStyle().StrokeWeight);

        g.PopStyle();

        Assert.Equal(new SKColor(255, 0, 0), g.ExposeStyle().FillColor);
        Assert.Equal(1, g.ExposeStyle().StrokeWeight);
    }

    [Fact]
    public void PushStyle_MultipleLevels()
    {
        var g = new TestGraphics();

        g.ExposeFill(1, 1, 1);
        g.PushStyle(); // level 1
        g.ExposeFill(2, 2, 2);
        g.PushStyle(); // level 2
        g.ExposeFill(3, 3, 3);

        Assert.Equal(new SKColor(3, 3, 3), g.ExposeStyle().FillColor);

        g.PopStyle(); // back to level 1
        Assert.Equal(new SKColor(2, 2, 2), g.ExposeStyle().FillColor);

        g.PopStyle(); // back to root
        Assert.Equal(new SKColor(1, 1, 1), g.ExposeStyle().FillColor);
    }

    [Fact]
    public void PopStyle_WithEmptyStack_DoesNotThrow()
    {
        var g = new TestGraphics();
        g.PopStyle(); // should not throw
        Assert.True(g.ExposeStyle().Fill); // still default
    }

    [Fact]
    public void PopMatrix_WithEmptyStack_DoesNotThrow()
    {
        var g = new TestGraphics();
        g.PopMatrix(); // should not throw
        Assert.True(g.GetCurrentMatrix().IsIdentity);
    }

    [Fact]
    public void BackgroundGray_ForwardsToRGBA()
    {
        var g = new TestGraphics();
        // Background(128) → Background(128,128,128,255)
        // 这里只验证虚方法转发逻辑不抛异常
        g.Background(128);
    }

    [Fact]
    public void Fill_WithAlpha()
    {
        var g = new TestGraphics();
        g.ExposeFill(100, 150, 200, 128);

        var s = g.ExposeStyle();
        Assert.Equal(new SKColor(100, 150, 200, 128), s.FillColor);
    }
}
