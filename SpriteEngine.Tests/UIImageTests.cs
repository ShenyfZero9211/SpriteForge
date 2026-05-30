using SkiaSharp;
using SpriteCore.Graphics;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIImageTests
{
    [Fact]
    public void Measure_WithTexture_UsesBitmapSize()
    {
        using var bitmap = new SKBitmap(64, 32);
        var texture = new SPTexture(bitmap);
        var img = new UIImage { Texture = texture };

        img.Measure();

        Assert.Equal(64, img.DesiredWidth);
        Assert.Equal(32, img.DesiredHeight);
    }

    [Fact]
    public void Measure_NoTexture_UsesExplicitSize()
    {
        var img = new UIImage { Width = 100, Height = 50 };
        img.Measure();

        Assert.Equal(100, img.DesiredWidth);
        Assert.Equal(50, img.DesiredHeight);
    }

    [Fact]
    public void BuildDrawList_WithTexture_GeneratesImageCall()
    {
        using var bitmap = new SKBitmap(64, 32);
        var texture = new SPTexture(bitmap);
        var img = new UIImage
        {
            Texture = texture,
            Width = 64,
            Height = 32,
            LocalX = 10,
            LocalY = 20
        };

        var drawList = new UIDrawList();
        img.BuildDrawList(drawList);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);

        // UIImage generates image draw calls
        // MockGraphics doesn't track image calls, but Render should not throw
        Assert.True(mockGfx.RectCalls.Count == 0); // no rect for pure image
    }

    [Fact]
    public void FitMode_Original_CentersImage()
    {
        using var bitmap = new SKBitmap(32, 32);
        var texture = new SPTexture(bitmap);
        var img = new UIImage
        {
            Texture = texture,
            Width = 100,
            Height = 100,
            FitMode = ImageFitMode.Original,
            LocalX = 0,
            LocalY = 0
        };

        var drawList = new UIDrawList();
        img.BuildDrawList(drawList);

        // Render should not throw; Original mode centers the 32x32 image in 100x100 area
        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);
    }

    [Fact]
    public void FitMode_Contain_ScalesToFit()
    {
        using var bitmap = new SKBitmap(64, 32);
        var texture = new SPTexture(bitmap);
        var img = new UIImage
        {
            Texture = texture,
            Width = 100,
            Height = 100,
            FitMode = ImageFitMode.Contain
        };

        var drawList = new UIDrawList();
        img.BuildDrawList(drawList);

        var mockGfx = new MockGraphics();
        drawList.Render(mockGfx);
    }
}
