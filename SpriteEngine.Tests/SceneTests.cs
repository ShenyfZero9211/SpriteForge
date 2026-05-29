using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class SceneTests
{
    [Fact]
    public void CreateGameObject_AddsToRoot()
    {
        var scene = new Scene("Test");
        var go = scene.CreateGameObject("Player");

        Assert.Single(scene.RootObjects);
        Assert.Equal(go, scene.RootObjects[0]);
        Assert.Equal(scene, go.Scene);
    }

    [Fact]
    public void Find_ByName_ReturnsObject()
    {
        var scene = new Scene("Test");
        scene.CreateGameObject("Player");
        scene.CreateGameObject("Enemy");

        var found = scene.Find("Enemy");
        Assert.NotNull(found);
        Assert.Equal("Enemy", found.Name);
    }

    [Fact]
    public void Find_ByName_DeepSearch()
    {
        var scene = new Scene("Test");
        var parent = scene.CreateGameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);

        var found = scene.Find("Child");
        Assert.NotNull(found);
        Assert.Equal("Child", found.Name);
    }

    [Fact]
    public void Find_NotFound_ReturnsNull()
    {
        var scene = new Scene("Test");
        Assert.Null(scene.Find("NonExistent"));
    }

    [Fact]
    public void FindWithTag_ReturnsFirst()
    {
        var scene = new Scene("Test");
        var go1 = scene.CreateGameObject("A");
        go1.Tag = "Enemy";
        var go2 = scene.CreateGameObject("B");
        go2.Tag = "Enemy";

        var found = scene.FindWithTag("Enemy");
        Assert.NotNull(found);
    }

    [Fact]
    public void FindAllWithTag_ReturnsAll()
    {
        var scene = new Scene("Test");
        scene.CreateGameObject("A").Tag = "Enemy";
        scene.CreateGameObject("B").Tag = "Enemy";
        scene.CreateGameObject("C").Tag = "Player";

        var enemies = scene.FindAllWithTag("Enemy").ToList();
        Assert.Equal(2, enemies.Count);
    }

    [Fact]
    public void FindWithComponent_ReturnsMatching()
    {
        var scene = new Scene("Test");
        var go1 = scene.CreateGameObject("A");
        go1.AddComponent<GameObjectTests.TestComponent>();
        scene.CreateGameObject("B");

        var found = scene.FindWithComponent<GameObjectTests.TestComponent>();
        Assert.NotNull(found);
        Assert.Equal("A", found.Name);
    }

    [Fact]
    public void Update_InvokesComponentUpdate()
    {
        var scene = new Scene("Test");
        var go = scene.CreateGameObject("A");
        var comp = go.AddComponent<GameObjectTests.TestComponent>();

        scene.Update(0.016f);
        Assert.Equal(1, comp.UpdateCount);
    }

    [Fact]
    public void Update_SkipsInactiveObjects()
    {
        var scene = new Scene("Test");
        var go = scene.CreateGameObject("A");
        var comp = go.AddComponent<GameObjectTests.TestComponent>();
        go.Active = false;

        scene.Update(0.016f);
        Assert.Equal(0, comp.UpdateCount);
    }

    [Fact]
    public void RemoveGameObject_RemovesFromScene()
    {
        var scene = new Scene("Test");
        var go = scene.CreateGameObject("A");
        scene.RemoveGameObject(go);

        Assert.Empty(scene.RootObjects);
        Assert.Null(go.Scene);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var scene = new Scene("Test");
        scene.CreateGameObject("A");
        scene.CreateGameObject("B");
        scene.Clear();

        Assert.Empty(scene.RootObjects);
    }

    [Fact]
    public void AllObjects_FlattensHierarchy()
    {
        var scene = new Scene("Test");
        var root = scene.CreateGameObject("Root");
        var child = new GameObject("Child");
        child.SetParent(root);

        var all = scene.AllObjects.ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(root, all);
        Assert.Contains(child, all);
    }
}
