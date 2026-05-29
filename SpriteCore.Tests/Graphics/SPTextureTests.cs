using SkiaSharp;
using SpriteCore.Graphics;

namespace SpriteCore.Tests.Graphics;

public class SPTextureTests
{
    private readonly string _testImagePath;

    public SPTextureTests()
    {
        _testImagePath = Path.Combine("..", "..", "..", "..", "..", "data", "textures", "ruins", "ruins_0.png");
    }

    [Fact]
    public void Load_ReturnsTextureWithCorrectDimensions()
    {
        var texture = SPTexture.Load(_testImagePath);
        Assert.True(texture.Width > 0);
        Assert.True(texture.Height > 0);
        Assert.NotNull(texture.Bitmap);
    }

    [Fact]
    public void Load_SamePathReturnsSameInstanceFromCache()
    {
        var a = SPTexture.Load(_testImagePath);
        var b = SPTexture.Load(_testImagePath);
        Assert.Same(a, b);
    }

    [Fact]
    public void Load_FileNotFound_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => SPTexture.Load("nonexistent.png"));
    }
}
