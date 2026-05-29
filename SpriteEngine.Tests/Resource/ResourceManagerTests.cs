using SpriteEngine.Resource;

namespace SpriteEngine.Tests.Resource;

public class ResourceManagerTests
{
    private readonly string _testImagePath;

    public ResourceManagerTests()
    {
        _testImagePath = Path.Combine("..", "..", "..", "..", "..", "data", "textures", "ruins", "ruins_0.png");
    }

    [Fact]
    public void Load_Texture2D_ReturnsValidTexture()
    {
        var rm = new ResourceManager();
        var tex = rm.Load<Texture2D>(_testImagePath);

        Assert.NotNull(tex);
        Assert.True(tex.Width > 0);
        Assert.True(tex.Height > 0);
        Assert.Equal(Path.GetFullPath(_testImagePath), tex.Path);
    }

    [Fact]
    public void Load_SamePathTwice_ReturnsSameInstance()
    {
        var rm = new ResourceManager();
        var a = rm.Load<Texture2D>(_testImagePath);
        var b = rm.Load<Texture2D>(_testImagePath);

        Assert.Same(a, b);
    }

    [Fact]
    public void Unload_RefCountReachesZero_RemovesFromCache()
    {
        var rm = new ResourceManager();
        var tex = rm.Load<Texture2D>(_testImagePath);
        var path = tex.Path;

        Assert.Equal(1, rm.LoadedCount);

        rm.Unload(path);
        Assert.Equal(0, rm.LoadedCount);
        Assert.Null(rm.Get<Texture2D>(path));
    }

    [Fact]
    public void Unload_RefCountStillPositive_KeepsInCache()
    {
        var rm = new ResourceManager();
        var tex = rm.Load<Texture2D>(_testImagePath);
        var path = tex.Path;

        var same = rm.Load<Texture2D>(_testImagePath); // refCount = 2
        rm.Unload(path); // refCount = 1

        Assert.Equal(1, rm.LoadedCount);
        Assert.NotNull(rm.Get<Texture2D>(path));
    }

    [Fact]
    public void Get_NotLoaded_ReturnsNull()
    {
        var rm = new ResourceManager();
        Assert.Null(rm.Get<Texture2D>("nonexistent.png"));
    }

    [Fact]
    public void DisposeAll_ClearsEverything()
    {
        var rm = new ResourceManager();
        rm.Load<Texture2D>(_testImagePath);
        rm.DisposeAll();
        Assert.Equal(0, rm.LoadedCount);
    }

    [Fact]
    public async Task LoadAsync_Texture2D_ReturnsValidTexture()
    {
        var rm = new ResourceManager();
        var tex = await rm.LoadAsync<Texture2D>(_testImagePath);

        Assert.NotNull(tex);
        Assert.True(tex.Width > 0);
    }
}
