using SpriteCore.Math;
using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class GameObjectTests
{
    [Fact]
    public void Constructor_AssignsIdAndName()
    {
        var go = new GameObject("Player");
        Assert.Equal("Player", go.Name);
        Assert.True(go.Id > 0);
    }

    [Fact]
    public void Constructor_AutoAttachesTransform()
    {
        var go = new GameObject();
        Assert.NotNull(go.Transform);
        Assert.NotNull(go.GetComponent<Transform>());
    }

    [Fact]
    public void AddComponent_AttachesAndSetsGameObject()
    {
        var go = new GameObject();
        var comp = go.AddComponent<TestComponent>();

        Assert.NotNull(comp);
        Assert.Equal(go, comp.GameObject);
    }

    [Fact]
    public void GetComponent_ReturnsCorrectType()
    {
        var go = new GameObject();
        go.AddComponent<TestComponent>();

        var found = go.GetComponent<TestComponent>();
        Assert.NotNull(found);
    }

    [Fact]
    public void GetComponent_WrongType_ReturnsNull()
    {
        var go = new GameObject();
        var found = go.GetComponent<TestComponent>();
        Assert.Null(found);
    }

    [Fact]
    public void RemoveComponent_RemovesAndNullsGameObject()
    {
        var go = new GameObject();
        go.AddComponent<TestComponent>();
        go.RemoveComponent<TestComponent>();

        Assert.Null(go.GetComponent<TestComponent>());
    }

    [Fact]
    public void SetParent_CreatesHierarchy()
    {
        var parent = new GameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);

        Assert.Equal(parent, child.Parent);
        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void SetParent_ToNull_Detaches()
    {
        var parent = new GameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);
        child.SetParent(null);

        Assert.Null(child.Parent);
        Assert.DoesNotContain(child, parent.Children);
    }

    [Fact]
    public void SetParent_Self_Throws()
    {
        var go = new GameObject();
        Assert.Throws<InvalidOperationException>(() => go.SetParent(go));
    }

    [Fact]
    public void FindChild_Recursive_FindsDeep()
    {
        var root = new GameObject("Root");
        var level1 = new GameObject("L1");
        var level2 = new GameObject("L2");
        level1.SetParent(root);
        level2.SetParent(level1);

        Assert.Equal(level2, root.FindChild("L2"));
    }

    [Fact]
    public void Active_False_SkipsUpdate()
    {
        var go = new GameObject();
        var comp = go.AddComponent<TestComponent>();
        go.Active = false;

        go.InvokeUpdate(0.016f);
        Assert.Equal(0, comp.UpdateCount);
    }

    [Fact]
    public void Update_InvokesOnEnabledComponents()
    {
        var go = new GameObject();
        var comp = go.AddComponent<TestComponent>();

        go.InvokeUpdate(0.016f);
        Assert.Equal(1, comp.UpdateCount);
    }

    [Fact]
    public void Enabled_False_SkipsComponentUpdate()
    {
        var go = new GameObject();
        var comp = go.AddComponent<TestComponent>();
        comp.Enabled = false;

        go.InvokeUpdate(0.016f);
        Assert.Equal(0, comp.UpdateCount);
    }

    public class TestComponent : Component
    {
        public int UpdateCount = 0;
        public override void Update(float dt) => UpdateCount++;
    }
}
