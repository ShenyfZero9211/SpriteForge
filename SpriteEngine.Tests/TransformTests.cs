using SpriteCore.Math;
using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class TransformTests
{
    [Fact]
    public void DefaultValues_AreZeroAndOne()
    {
        var t = new Transform();
        Assert.Equal(Vector2.Zero, t.LocalPosition);
        Assert.Equal(0f, t.LocalRotation);
        Assert.Equal(Vector2.One, t.LocalScale);
    }

    [Fact]
    public void LocalPosition_ChangesIndependently()
    {
        var t = new Transform();
        t.LocalPosition = new Vector2(100, 200);
        Assert.Equal(new Vector2(100, 200), t.LocalPosition);
    }

    [Fact]
    public void Position_NoParent_EqualsLocalPosition()
    {
        var go = new GameObject();
        go.Transform.LocalPosition = new Vector2(50, 100);
        Assert.Equal(new Vector2(50, 100), go.Transform.Position);
    }

    [Fact]
    public void Position_WithParent_Accumulates()
    {
        var parent = new GameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);

        parent.Transform.LocalPosition = new Vector2(100, 0);
        child.Transform.LocalPosition = new Vector2(50, 0);

        Assert.Equal(new Vector2(150, 0), child.Transform.Position);
    }

    [Fact]
    public void Position_Setter_UpdatesLocalRelativeToParent()
    {
        var parent = new GameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);

        parent.Transform.LocalPosition = new Vector2(100, 0);
        child.Transform.Position = new Vector2(150, 0);

        Assert.Equal(new Vector2(50, 0), child.Transform.LocalPosition);
    }

    [Fact]
    public void Rotation_NoParent_EqualsLocalRotation()
    {
        var go = new GameObject();
        go.Transform.LocalRotation = 45;
        Assert.Equal(45f, go.Transform.Rotation);
    }

    [Fact]
    public void Rotation_WithParent_Accumulates()
    {
        var parent = new GameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);

        parent.Transform.LocalRotation = 30;
        child.Transform.LocalRotation = 20;

        Assert.Equal(50f, child.Transform.Rotation);
    }

    [Fact]
    public void Scale_WithParent_Multiplies()
    {
        var parent = new GameObject("Parent");
        var child = new GameObject("Child");
        child.SetParent(parent);

        parent.Transform.LocalScale = new Vector2(2, 2);
        child.Transform.LocalScale = new Vector2(3, 3);

        Assert.Equal(new Vector2(6, 6), child.Transform.Scale);
    }

    [Fact]
    public void Forward_ZeroRotation_IsRight()
    {
        var go = new GameObject();
        go.Transform.LocalRotation = 0;
        var forward = go.Transform.Forward;
        Assert.Equal(1f, forward.X, 0.001f);
        Assert.Equal(0f, forward.Y, 0.001f);
    }

    [Fact]
    public void Forward_90Rotation_IsUp()
    {
        var go = new GameObject();
        go.Transform.LocalRotation = 90;
        var forward = go.Transform.Forward;
        Assert.Equal(0f, forward.X, 0.001f);
        Assert.Equal(1f, forward.Y, 0.001f);
    }

    [Fact]
    public void Right_ZeroRotation_IsUp()
    {
        var go = new GameObject();
        go.Transform.LocalRotation = 0;
        var right = go.Transform.Right;
        Assert.Equal(0f, right.X, 0.001f);
        Assert.Equal(1f, right.Y, 0.001f);
    }

    [Fact]
    public void LocalToWorld_WithPositionOffset()
    {
        var go = new GameObject();
        go.Transform.LocalPosition = new Vector2(100, 100);
        var world = go.Transform.LocalToWorld(new Vector2(10, 0));
        Assert.Equal(110f, world.X, 0.1f);
        Assert.Equal(100f, world.Y, 0.1f);
    }

    [Fact]
    public void LocalToWorld_WithRotation()
    {
        var go = new GameObject();
        go.Transform.LocalRotation = 90;
        var world = go.Transform.LocalToWorld(new Vector2(10, 0));
        Assert.Equal(0f, world.X, 0.1f);
        Assert.Equal(10f, world.Y, 0.1f);
    }

    [Fact]
    public void WorldToLocal_RoundTrip()
    {
        var go = new GameObject();
        go.Transform.LocalPosition = new Vector2(50, 50);
        go.Transform.LocalRotation = 30;
        go.Transform.LocalScale = new Vector2(2, 2);

        var localPoint = new Vector2(10, 5);
        var world = go.Transform.LocalToWorld(localPoint);
        var back = go.Transform.WorldToLocal(world);

        Assert.Equal(localPoint.X, back.X, 0.1f);
        Assert.Equal(localPoint.Y, back.Y, 0.1f);
    }

    [Fact]
    public void DeepHierarchy_PositionAccumulates()
    {
        var root = new GameObject("Root");
        var l1 = new GameObject("L1");
        var l2 = new GameObject("L2");
        l1.SetParent(root);
        l2.SetParent(l1);

        root.Transform.LocalPosition = new Vector2(10, 0);
        l1.Transform.LocalPosition = new Vector2(20, 0);
        l2.Transform.LocalPosition = new Vector2(30, 0);

        Assert.Equal(new Vector2(60, 0), l2.Transform.Position);
    }
}
