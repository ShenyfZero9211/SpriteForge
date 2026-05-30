using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIStateStorageTests
{
    [Fact]
    public void GetOrCreate_NewId_ReturnsDefault()
    {
        var storage = new UIStateStorage();
        var value = storage.GetOrCreate("counter", 0);
        Assert.Equal(0, value);
    }

    [Fact]
    public void GetOrCreate_ExistingId_ReturnsStoredValue()
    {
        var storage = new UIStateStorage();
        storage.Set("counter", 42);
        var value = storage.GetOrCreate("counter", 0);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Set_OverwritesValue()
    {
        var storage = new UIStateStorage();
        storage.Set("key", "first");
        storage.Set("key", "second");
        var value = storage.GetOrCreate("key", "");
        Assert.Equal("second", value);
    }

    [Fact]
    public void TryGet_Existing_ReturnsTrue()
    {
        var storage = new UIStateStorage();
        storage.Set("key", 123);
        bool found = storage.TryGet("key", out int value);
        Assert.True(found);
        Assert.Equal(123, value);
    }

    [Fact]
    public void TryGet_Missing_ReturnsFalse()
    {
        var storage = new UIStateStorage();
        bool found = storage.TryGet("missing", out int value);
        Assert.False(found);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGet_WrongType_ReturnsFalse()
    {
        var storage = new UIStateStorage();
        storage.Set("key", "string");
        bool found = storage.TryGet("key", out int value);
        Assert.False(found);
    }

    [Fact]
    public void Remove_Existing_ReturnsTrue()
    {
        var storage = new UIStateStorage();
        storage.Set("key", 1);
        bool removed = storage.Remove("key");
        Assert.True(removed);
        Assert.False(storage.TryGet("key", out int _));
    }

    [Fact]
    public void Remove_Missing_ReturnsFalse()
    {
        var storage = new UIStateStorage();
        bool removed = storage.Remove("missing");
        Assert.False(removed);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var storage = new UIStateStorage();
        storage.Set("a", 1);
        storage.Set("b", 2);
        storage.Clear();
        Assert.False(storage.TryGet("a", out int _));
        Assert.False(storage.TryGet("b", out int _));
    }

    [Fact]
    public void MultipleTypes_Coexist()
    {
        var storage = new UIStateStorage();
        storage.Set("int", 42);
        storage.Set("string", "hello");
        storage.Set("bool", true);

        Assert.Equal(42, storage.GetOrCreate("int", 0));
        Assert.Equal("hello", storage.GetOrCreate("string", ""));
        Assert.True(storage.GetOrCreate("bool", false));
    }
}
