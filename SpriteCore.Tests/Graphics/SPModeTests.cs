using SkiaSharp;
using SpriteCore.Graphics;
using Xunit;

namespace SpriteCore.Tests.Graphics;

public class SPModeTests
{
    // ── RectMode ──

    [Fact]
    public void RectMode_Default_IsCorner()
    {
        var s = new SPStyle();
        Assert.Equal(SPRectMode.CORNER, s.RectMode);
    }

    [Fact]
    public void RectMode_CanBeChanged()
    {
        var s = new SPStyle { RectMode = SPRectMode.CENTER };
        Assert.Equal(SPRectMode.CENTER, s.RectMode);
    }

    [Fact]
    public void RectMode_Cloning_PreservesValue()
    {
        var s = new SPStyle { RectMode = SPRectMode.RADIUS };
        Assert.Equal(SPRectMode.RADIUS, s.Clone().RectMode);
    }

    // ── EllipseMode ──

    [Fact]
    public void EllipseMode_Default_IsCenter()
    {
        var s = new SPStyle();
        Assert.Equal(SPEllipseMode.CENTER, s.EllipseMode);
    }

    [Fact]
    public void EllipseMode_CanBeChanged()
    {
        var s = new SPStyle { EllipseMode = SPEllipseMode.CORNER };
        Assert.Equal(SPEllipseMode.CORNER, s.EllipseMode);
    }

    // ── ImageMode ──

    [Fact]
    public void ImageMode_Default_IsCorner()
    {
        var s = new SPStyle();
        Assert.Equal(SPImageMode.CORNER, s.ImageMode);
    }

    // ── TextAlign ──

    [Fact]
    public void TextAlignH_Default_IsLeft()
    {
        var s = new SPStyle();
        Assert.Equal(SPTextAlignH.LEFT, s.TextAlignH);
    }

    [Fact]
    public void TextAlignV_Default_IsBaseline()
    {
        var s = new SPStyle();
        Assert.Equal(SPTextAlignV.BASELINE, s.TextAlignV);
    }

    [Fact]
    public void TextAlign_Cloning_PreservesBothValues()
    {
        var s = new SPStyle
        {
            TextAlignH = SPTextAlignH.CENTER,
            TextAlignV = SPTextAlignV.BOTTOM
        };
        var copy = s.Clone();
        Assert.Equal(SPTextAlignH.CENTER, copy.TextAlignH);
        Assert.Equal(SPTextAlignV.BOTTOM, copy.TextAlignV);
    }

    // ── StrokeCap / StrokeJoin ──

    [Fact]
    public void StrokeCap_Default_IsRound()
    {
        var s = new SPStyle();
        Assert.Equal(SPStrokeCap.ROUND, s.StrokeCap);
    }

    [Fact]
    public void StrokeJoin_Default_IsMiter()
    {
        var s = new SPStyle();
        Assert.Equal(SPStrokeJoin.MITER, s.StrokeJoin);
    }

    // ── SkiaGraphics smoke tests ──

    [Fact]
    public void SkiaGraphics_Rect_AllModes_DoNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);

        foreach (SPRectMode mode in Enum.GetValues(typeof(SPRectMode)))
        {
            g.RectMode(mode);
            g.Rect(10, 10, 20, 20);
        }
    }

    [Fact]
    public void SkiaGraphics_Ellipse_AllModes_DoNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);

        foreach (SPEllipseMode mode in Enum.GetValues(typeof(SPEllipseMode)))
        {
            g.EllipseMode(mode);
            g.Ellipse(50, 50, 20, 20);
        }
    }

    [Fact]
    public void SkiaGraphics_Circle_FollowsEllipseMode()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);

        g.EllipseMode(SPEllipseMode.CORNER);
        g.Circle(10, 10, 20); // should not throw

        g.EllipseMode(SPEllipseMode.CENTER);
        g.Circle(50, 50, 20); // should not throw
    }

    [Fact]
    public void SkiaGraphics_Text_AllAligns_DoNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);

        foreach (SPTextAlignH h in Enum.GetValues(typeof(SPTextAlignH)))
        {
            foreach (SPTextAlignV v in Enum.GetValues(typeof(SPTextAlignV)))
            {
                g.TextAlign(h, v);
                g.Text("Hello", 50, 50);
            }
        }
    }

    [Fact]
    public void SkiaGraphics_Point_DoesNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);
        g.Point(50, 50);
    }

    [Fact]
    public void SkiaGraphics_Quad_DoesNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);
        g.Quad(10, 10, 90, 10, 90, 90, 10, 90);
    }

    [Fact]
    public void SkiaGraphics_Arc_DoesNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);
        g.Arc(50, 50, 40, 40, 0, 90);
    }

    [Fact]
    public void SkiaGraphics_RoundRect_DoesNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);
        g.RoundRect(10, 10, 80, 80, 8);
    }

    [Fact]
    public void SkiaGraphics_StrokeCapAndJoin_DoNotThrow()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);

        g.StrokeCap(SPStrokeCap.SQUARE);
        g.StrokeJoin(SPStrokeJoin.BEVEL);
        g.Line(10, 10, 90, 90);
    }

    [Fact]
    public void SkiaGraphics_PushPopStyle_PreservesModes()
    {
        using var g = new SkiaGraphics();
        g.Initialize(100, 100);

        g.RectMode(SPRectMode.CORNER);
        g.EllipseMode(SPEllipseMode.CENTER);
        g.TextAlign(SPTextAlignH.LEFT, SPTextAlignV.BASELINE);
        g.StrokeCap(SPStrokeCap.ROUND);
        g.StrokeJoin(SPStrokeJoin.MITER);

        g.PushStyle();

        g.RectMode(SPRectMode.CENTER);
        g.EllipseMode(SPEllipseMode.CORNER);
        g.TextAlign(SPTextAlignH.RIGHT, SPTextAlignV.TOP);
        g.StrokeCap(SPStrokeCap.PROJECT);
        g.StrokeJoin(SPStrokeJoin.BEVEL);

        Assert.Equal(SPRectMode.CENTER, g.CurrentStyle.RectMode);
        Assert.Equal(SPEllipseMode.CORNER, g.CurrentStyle.EllipseMode);
        Assert.Equal(SPTextAlignH.RIGHT, g.CurrentStyle.TextAlignH);
        Assert.Equal(SPTextAlignV.TOP, g.CurrentStyle.TextAlignV);
        Assert.Equal(SPStrokeCap.PROJECT, g.CurrentStyle.StrokeCap);
        Assert.Equal(SPStrokeJoin.BEVEL, g.CurrentStyle.StrokeJoin);

        g.PopStyle();

        Assert.Equal(SPRectMode.CORNER, g.CurrentStyle.RectMode);
        Assert.Equal(SPEllipseMode.CENTER, g.CurrentStyle.EllipseMode);
        Assert.Equal(SPTextAlignH.LEFT, g.CurrentStyle.TextAlignH);
        Assert.Equal(SPTextAlignV.BASELINE, g.CurrentStyle.TextAlignV);
        Assert.Equal(SPStrokeCap.ROUND, g.CurrentStyle.StrokeCap);
        Assert.Equal(SPStrokeJoin.MITER, g.CurrentStyle.StrokeJoin);
    }
}
