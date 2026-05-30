using SkiaSharp;
using SpriteCore.Graphics;
using Xunit;

namespace SpriteCore.Tests;

public class SPStyleTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var s = new SPStyle();

        Assert.True(s.Fill);
        Assert.Equal(SKColors.White, s.FillColor);
        Assert.True(s.Stroke);
        Assert.Equal(SKColors.Black, s.StrokeColor);
        Assert.Equal(1, s.StrokeWeight);
        Assert.Equal(SPStrokeCap.ROUND, s.StrokeCap);
        Assert.Equal(SPStrokeJoin.MITER, s.StrokeJoin);
        Assert.Equal(16, s.TextSize);
        Assert.Equal(SPTextAlignH.LEFT, s.TextAlignH);
        Assert.Equal(SPTextAlignV.BASELINE, s.TextAlignV);
        Assert.Equal(SPColorMode.RGB, s.ColorMode);
        Assert.Equal(SPRectMode.CORNER, s.RectMode);
        Assert.Equal(SPEllipseMode.CENTER, s.EllipseMode);
        Assert.Equal(SPImageMode.CORNER, s.ImageMode);
        Assert.False(s.IsTinted);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new SPStyle
        {
            FillColor = new SKColor(100, 150, 200),
            StrokeWeight = 5,
            TextSize = 24
        };

        var copy = original.Clone();

        Assert.Equal(original.FillColor, copy.FillColor);
        Assert.Equal(original.StrokeWeight, copy.StrokeWeight);
        Assert.Equal(original.TextSize, copy.TextSize);

        // 修改副本不应影响原始
        copy.FillColor = SKColors.Red;
        copy.StrokeWeight = 10;

        Assert.Equal(new SKColor(100, 150, 200), original.FillColor);
        Assert.Equal(5, original.StrokeWeight);
    }

    [Fact]
    public void Clone_CopiesAllFields()
    {
        var original = new SPStyle
        {
            Fill = false,
            FillColor = new SKColor(1, 2, 3, 4),
            Stroke = false,
            StrokeColor = new SKColor(5, 6, 7, 8),
            StrokeWeight = 3.5f,
            StrokeCap = SPStrokeCap.SQUARE,
            StrokeJoin = SPStrokeJoin.ROUND,
            TextSize = 42,
            TextAlignH = SPTextAlignH.CENTER,
            TextAlignV = SPTextAlignV.TOP,
            ColorMode = SPColorMode.HSB,
            ColorModeX = 100,
            ColorModeY = 200,
            ColorModeZ = 300,
            ColorModeA = 400,
            RectMode = SPRectMode.RADIUS,
            EllipseMode = SPEllipseMode.CORNERS,
            ImageMode = SPImageMode.CENTER,
            IsTinted = true,
            TintColor = new SKColor(255, 128, 64)
        };

        var copy = original.Clone();

        Assert.Equal(original.Fill, copy.Fill);
        Assert.Equal(original.FillColor, copy.FillColor);
        Assert.Equal(original.Stroke, copy.Stroke);
        Assert.Equal(original.StrokeColor, copy.StrokeColor);
        Assert.Equal(original.StrokeWeight, copy.StrokeWeight);
        Assert.Equal(original.StrokeCap, copy.StrokeCap);
        Assert.Equal(original.StrokeJoin, copy.StrokeJoin);
        Assert.Equal(original.TextSize, copy.TextSize);
        Assert.Equal(original.TextAlignH, copy.TextAlignH);
        Assert.Equal(original.TextAlignV, copy.TextAlignV);
        Assert.Equal(original.ColorMode, copy.ColorMode);
        Assert.Equal(original.ColorModeX, copy.ColorModeX);
        Assert.Equal(original.ColorModeY, copy.ColorModeY);
        Assert.Equal(original.ColorModeZ, copy.ColorModeZ);
        Assert.Equal(original.ColorModeA, copy.ColorModeA);
        Assert.Equal(original.RectMode, copy.RectMode);
        Assert.Equal(original.EllipseMode, copy.EllipseMode);
        Assert.Equal(original.ImageMode, copy.ImageMode);
        Assert.Equal(original.IsTinted, copy.IsTinted);
        Assert.Equal(original.TintColor, copy.TintColor);
    }
}
