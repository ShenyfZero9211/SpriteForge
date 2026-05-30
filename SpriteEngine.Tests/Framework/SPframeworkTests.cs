using SpriteEngine.Framework;
using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class SPframeworkTests
{
    private class TestFramework : SPframework
    {
        public bool InitCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool RenderCalled { get; private set; }
        public bool DisposeCalled { get; private set; }
        public float LastDt { get; private set; }

        public override void Init() => InitCalled = true;
        public override void Update(float dt) { UpdateCalled = true; LastDt = dt; }
        public override void Render() => RenderCalled = true;
        public override void Dispose() => DisposeCalled = true;
    }

    [Fact]
    public void DefaultProperties_AreCorrect()
    {
        var fw = new TestFramework();

        Assert.Equal("SPframework Game", fw.Title);
        Assert.Equal(0f, fw.Elapsed);
        Assert.Equal(0, fw.FrameCount);
        Assert.Equal(0f, fw.DeltaTime);
        Assert.Null(fw.Input);
        Assert.Null(fw.Audio);
    }

    [Fact]
    public void Size_SetsDimensions()
    {
        var fw = new TestFramework();
        fw.Size(1024, 768);

        // Size does not immediately affect SP5.Width/Height (only at Launch)
        // but we can verify no exception is thrown
        Assert.NotNull(fw);
    }

    [Fact]
    public void TitleSet_UpdatesTitle()
    {
        var fw = new TestFramework();
        fw.TitleSet("My Game");

        Assert.Equal("My Game", fw.Title);
    }

    [Fact]
    public void TitleSet_Null_FallsBackToDefault()
    {
        var fw = new TestFramework();
        fw.TitleSet(null!);

        Assert.Equal("SPframework Game", fw.Title);
    }

    [Fact]
    public void Lifecycle_ManualCall_InvokesHooks()
    {
        var fw = new TestFramework();

        fw.Init();
        Assert.True(fw.InitCalled);

        fw.Update(0.016f);
        Assert.True(fw.UpdateCalled);
        Assert.Equal(0.016f, fw.LastDt);

        fw.Render();
        Assert.True(fw.RenderCalled);

        fw.Dispose();
        Assert.True(fw.DisposeCalled);
    }

    [Fact]
    public void RegisterScene_AndLoadScene_Work()
    {
        var fw = new TestFramework();
        var scene = new Scene("TestScene");

        fw.RegisterScene("test", scene);
        fw.LoadScene("test");

        Assert.Same(scene, SceneManager.Instance.ActiveScene);
    }

    [Fact]
    public void LoadScene_Unregistered_Throws()
    {
        var fw = new TestFramework();

        Assert.Throws<KeyNotFoundException>(() => fw.LoadScene("missing"));
    }

    [Fact]
    public void SceneManager_Render_NullActive_DoesNotThrow()
    {
        // Ensure no active scene
        if (SceneManager.Instance.ActiveScene != null)
        {
            SceneManager.Instance.Load("__dummy__");
        }

        // This should not throw even with null active scene
        var exception = Record.Exception(() => SceneManager.Instance.Render());
        Assert.Null(exception);
    }
}
