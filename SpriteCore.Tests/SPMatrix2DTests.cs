using SkiaSharp;
using SpriteCore.Graphics;
using Xunit;

namespace SpriteCore.Tests;

public class SPMatrix2DTests
{
    [Fact]
    public void Default_IsIdentity()
    {
        var m = new SPMatrix2D();
        var sk = m.ToSkMatrix();
        Assert.True(sk.IsIdentity);
    }

    [Fact]
    public void Translate_AppliesCorrectly()
    {
        var m = new SPMatrix2D();
        m.Translate(100, 50);
        var sk = m.ToSkMatrix();

        // Processing 语义: translate → 坐标系原点移动
        // 在行向量系统下 (v' = v * M)，PreConcat 使变换顺序与代码一致
        Assert.Equal(100f, sk.TransX, 0.001f);
        Assert.Equal(50f, sk.TransY, 0.001f);
    }

    [Fact]
    public void TranslateThenRotate_MatchesProcessingSemantics()
    {
        // Processing: translate(100,0); rotate(90); → 先旋转再平移
        // 即 v' = v * R * T = (v * R) + T
        var m = new SPMatrix2D();
        m.Translate(100, 0);
        m.Rotate(90);
        var sk = m.ToSkMatrix();

        // 点 (10, 0) 经过 translate→rotate:
        // 先绕原点旋转 90° → (0, 10)
        // 再平移 (100, 0) → (100, 10)
        var p = sk.MapPoint(10, 0);
        Assert.Equal(100f, p.X, 1f);
        Assert.Equal(10f, p.Y, 1f);
    }

    [Fact]
    public void RotateThenTranslate_MatchesProcessingSemantics()
    {
        // Processing: rotate(90); translate(100, 0); → 先平移再旋转
        // 即 v' = v * T * R = (v + T) * R
        var m = new SPMatrix2D();
        m.Rotate(90);
        m.Translate(100, 0);
        var sk = m.ToSkMatrix();

        // 点 (10, 0) 经过 rotate→translate:
        // 先平移 (100, 0) → (110, 0)
        // 再绕原点旋转 90° → (0, 110)
        var p = sk.MapPoint(10, 0);
        Assert.Equal(0f, p.X, 1f);
        Assert.Equal(110f, p.Y, 1f);
    }

    [Fact]
    public void Scale_Uniform_Works()
    {
        var m = new SPMatrix2D();
        m.Scale(2, 2);
        var sk = m.ToSkMatrix();

        Assert.Equal(2f, sk.ScaleX, 0.001f);
        Assert.Equal(2f, sk.ScaleY, 0.001f);
    }

    [Fact]
    public void Clone_IsIndependent()
    {
        var m1 = new SPMatrix2D();
        m1.Translate(100, 100);
        var m2 = m1.Clone();

        m2.Translate(50, 50);

        var sk1 = m1.ToSkMatrix();
        var sk2 = m2.ToSkMatrix();

        Assert.Equal(100f, sk1.TransX, 1f);
        Assert.Equal(150f, sk2.TransX, 1f);
    }

    [Fact]
    public void Reset_RestoresIdentity()
    {
        var m = new SPMatrix2D();
        m.Translate(100, 100);
        m.Rotate(45);
        m.Scale(2, 2);
        m.Reset();

        Assert.True(m.ToSkMatrix().IsIdentity);
    }

    [Fact]
    public void PushPopMatrix_PreservesState()
    {
        var g = new TestGraphics();
        g.Resize(100, 100);

        g.Translate(100, 50);
        g.Rotate(30);

        g.PushMatrix();
        g.Translate(20, 10);
        g.Scale(2, 2);
        g.PopMatrix();

        // Pop 后应恢复到 Push 前的状态
        var m = g.GetCurrentMatrix();
        var p = m.MapPoint(10, 0);

        // 只经过 translate(100,50) + rotate(30)
        // 旋转 30° 后: (10*cos30, 10*sin30) ≈ (8.66, 5)
        // 平移后: (108.66, 55)
        Assert.Equal(108.66f, p.X, 1f);
        Assert.Equal(55f, p.Y, 1f);
    }

    /// <summary>用于测试的最小 SPGraphics 实现</summary>
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
        public override void Image(SkiaSharp.SKBitmap bitmap, float x, float y) { }
        public override void Image(SPTexture texture, float x, float y) { }
        public override void Image(SPTexture texture, float x, float y, float w, float h) { }
        public override void Present(IntPtr sdlRenderer, IntPtr sdlTexture) { }

        public SKMatrix GetCurrentMatrix() => CurrentMatrix.ToSkMatrix();
    }
}
