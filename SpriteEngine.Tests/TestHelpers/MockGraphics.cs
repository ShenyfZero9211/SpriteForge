using SkiaSharp;
using SpriteCore.Graphics;

namespace SpriteEngine.Tests;

/// <summary>
/// 测试用 SPGraphics mock，记录所有绘制调用。
/// </summary>
internal class MockGraphics : SPGraphics
{
    public readonly List<(string text, float x, float y)> TextCalls = new();
    public readonly List<(float x, float y, float w, float h)> RectCalls = new();
    public readonly List<(float x1, float y1, float x2, float y2)> LineCalls = new();

    public override void BeginFrame() { }
    public override void EndFrame() { }
    public override void Resize(int width, int height) { }
    public override void Background(float r, float g, float b, float a) { }
    public override void Rect(float x, float y, float w, float h) => RectCalls.Add((x, y, w, h));
    public override void RoundRect(float x, float y, float w, float h, float r) => RectCalls.Add((x, y, w, h));
    public override void Ellipse(float x, float y, float w, float h) { }
    public override void Circle(float x, float y, float r) { }
    public override void Line(float x1, float y1, float x2, float y2) => LineCalls.Add((x1, y1, x2, y2));
    public override void Triangle(float x1, float y1, float x2, float y2, float x3, float y3) { }
    public override void Point(float x, float y) { }
    public override void Quad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) { }
    public override void Arc(float x, float y, float w, float h, float start, float stop) { }
    public override void TextSize(float size) { }
    public override void Text(string str, float x, float y) => TextCalls.Add((str, x, y));
    public override void Image(SKBitmap bitmap, float x, float y) { }
    public override void Image(SPTexture texture, float x, float y) { }
    public override void Image(SPTexture texture, float x, float y, float w, float h) { }
    public override void Present(IntPtr sdlRenderer, IntPtr sdlTexture) { }
}
