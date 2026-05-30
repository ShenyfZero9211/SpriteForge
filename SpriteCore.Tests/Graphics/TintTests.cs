using SkiaSharp;
using SpriteCore.Graphics;

namespace SpriteCore.Tests.Graphics;

public class TintTests
{
    // ── SPStyle ──

    [Fact]
    public void SPStyle_Clone_PreservesTintState()
    {
        var original = new SPStyle
        {
            IsTinted = true,
            TintColor = new SKColor(255, 128, 64, 200)
        };

        var cloned = original.Clone();

        Assert.Equal(original.IsTinted, cloned.IsTinted);
        Assert.Equal(original.TintColor, cloned.TintColor);
    }

    // ── SPGraphics 状态机 ──

    [Fact]
    public void Tint_SetsIsTintedAndTintColor()
    {
        var g = new TestGraphics();
        g.Tint(255, 128);

        Assert.True(g.CurrentStyle.IsTinted);
        Assert.Equal(new SKColor(255, 255, 255, 128), g.CurrentStyle.TintColor);
    }

    [Fact]
    public void Tint_RGB_SetsCorrectTintColor()
    {
        var g = new TestGraphics();
        g.Tint(255, 128, 64, 200);

        Assert.True(g.CurrentStyle.IsTinted);
        Assert.Equal(new SKColor(255, 128, 64, 200), g.CurrentStyle.TintColor);
    }

    [Fact]
    public void NoTint_ClearsIsTinted()
    {
        var g = new TestGraphics();
        g.Tint(255, 128);
        g.NoTint();

        Assert.False(g.CurrentStyle.IsTinted);
    }

    // ── SkiaGraphics 绘制验证 ──

    [Fact]
    public void Image_WithTint_DimsRGB()
    {
        using var graphics = new SkiaGraphics();
        graphics.Initialize(4, 4);

        // 创建纯红色不透明纹理
        using var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(SKColors.Red);
        using var texture = new SPTexture(bitmap);

        // 背景黑色
        graphics.Background(0, 0, 0, 255);

        // Tint 50% 亮度（Skia premultiplied alpha：alpha 调制体现在 RGB 上）
        graphics.Tint(255, 128);
        graphics.Image(texture, 0, 0);

        // 读取像素
        var pixel = ReadPixel(graphics, 0, 0);

        // RGB 减半，alpha 保持不变（premultiplied 存储下 R_premul = 128）
        Assert.True(pixel.Red >= 120 && pixel.Red <= 135,
            $"Expected red ~128, got {pixel.Red}");
        Assert.True(pixel.Green < 10);
        Assert.True(pixel.Blue < 10);
        Assert.Equal(255, pixel.Alpha);
    }

    [Fact]
    public void Image_WithoutTint_KeepsOriginalColor()
    {
        using var graphics = new SkiaGraphics();
        graphics.Initialize(4, 4);

        using var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(SKColors.Red);
        using var texture = new SPTexture(bitmap);

        graphics.Background(0, 0, 0, 255);
        graphics.NoTint();
        graphics.Image(texture, 0, 0);

        var pixel = ReadPixel(graphics, 0, 0);

        Assert.Equal(255, pixel.Red);
        Assert.True(pixel.Green < 10);
        Assert.True(pixel.Blue < 10);
        Assert.Equal(255, pixel.Alpha);
    }

    [Fact]
    public void Image_WithTintColor_MultiplesRGB()
    {
        using var graphics = new SkiaGraphics();
        graphics.Initialize(4, 4);

        // 创建白色纹理
        using var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(SKColors.White);
        using var texture = new SPTexture(bitmap);

        graphics.Background(0, 0, 0, 255);

        // Tint 纯红色（R=255, G=0, B=0），保持 alpha
        graphics.Tint(255, 0, 0, 255);
        graphics.Image(texture, 0, 0);

        var pixel = ReadPixel(graphics, 0, 0);

        // White * Red = Red
        Assert.Equal(255, pixel.Red);
        Assert.True(pixel.Green < 10, $"Expected G~0, got {pixel.Green}");
        Assert.True(pixel.Blue < 10, $"Expected B~0, got {pixel.Blue}");
    }

    // ── 辅助 ──

    private static SKColor ReadPixel(SkiaGraphics graphics, int x, int y)
    {
        var canvas = graphics.Canvas;
        Assert.NotNull(canvas);

        using var image = canvas.Surface.Snapshot();
        using var pixmap = image.PeekPixels();
        Assert.NotNull(pixmap);

        return pixmap.GetPixelColor(x, y);
    }

    private class TestGraphics : SPGraphics
    {
        public override void BeginFrame() { }
        public override void EndFrame() { }
        public override void Resize(int width, int height) { Width = width; Height = height; }
        public override void Background(float r, float g, float b, float a) { }
        public override void Rect(float x, float y, float w, float h) { }
        public override void RoundRect(float x, float y, float w, float h, float r) { }
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
    }
}
