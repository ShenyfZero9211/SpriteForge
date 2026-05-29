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
        Assert.Equal(SKStrokeCap.Round, s.StrokeCap);
        Assert.Equal(SKStrokeJoin.Miter, s.StrokeJoin);
        Assert.Equal(16, s.TextSize);
        Assert.Equal(SKTextAlign.Left, s.TextAlign);
        Assert.Equal(0, s.ColorMode);
        Assert.Equal(0, s.RectMode);
        Assert.Equal(0, s.EllipseMode);
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
            StrokeCap = SKStrokeCap.Square,
            StrokeJoin = SKStrokeJoin.Round,
            TextSize = 42,
            TextAlign = SKTextAlign.Center,
            ColorMode = 1,
            ColorModeX = 100,
            ColorModeY = 200,
            ColorModeZ = 300,
            ColorModeA = 400,
            RectMode = 2,
            EllipseMode = 3
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
        Assert.Equal(original.TextAlign, copy.TextAlign);
        Assert.Equal(original.ColorMode, copy.ColorMode);
        Assert.Equal(original.ColorModeX, copy.ColorModeX);
        Assert.Equal(original.ColorModeY, copy.ColorModeY);
        Assert.Equal(original.ColorModeZ, copy.ColorModeZ);
        Assert.Equal(original.ColorModeA, copy.ColorModeA);
        Assert.Equal(original.RectMode, copy.RectMode);
        Assert.Equal(original.EllipseMode, copy.EllipseMode);
    }
}
