using SpriteCore.Math;
using SpriteEngine.Physics;
using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class PhysicsTests
{
    public PhysicsTests()
    {
        // 确保单位转换使用默认值
        PhysicsUtils.PixelsPerMeter = 32f;
    }

    // ── PhysicsUtils ──

    [Fact]
    public void PhysicsUtils_ToPhysics_ConvertsCorrectly()
    {
        Assert.Equal(1f, PhysicsUtils.ToPhysics(32f));
        Assert.Equal(2f, PhysicsUtils.ToPhysics(64f));
        Assert.Equal(0.5f, PhysicsUtils.ToPhysics(16f));
    }

    [Fact]
    public void PhysicsUtils_ToPixels_ConvertsCorrectly()
    {
        Assert.Equal(32f, PhysicsUtils.ToPixels(1f));
        Assert.Equal(64f, PhysicsUtils.ToPixels(2f));
        Assert.Equal(16f, PhysicsUtils.ToPixels(0.5f));
    }

    [Fact]
    public void PhysicsUtils_Vector2_RoundTrip()
    {
        var pixel = new Vector2(128, 256);
        var phys = PhysicsUtils.ToPhysics(pixel);
        var back = PhysicsUtils.ToPixels(phys);

        Assert.Equal(pixel.X, back.X, 3f);
        Assert.Equal(pixel.Y, back.Y, 3f);
    }

    // ── PhysicsWorld2D ──

    [Fact]
    public void PhysicsWorld2D_DefaultGravity_IsDownward()
    {
        var world = new PhysicsWorld2D();
        var g = world.Gravity;
        Assert.True(g.Y > 0, "Gravity should point downward (positive Y in screen space)");
        Assert.Equal(0f, g.X, 0.1f);
    }

    [Fact]
    public void PhysicsWorld2D_Gravity_CanBeChanged()
    {
        var world = new PhysicsWorld2D();
        world.Gravity = new Vector2(0, 100);
        var g = world.Gravity;
        Assert.Equal(100f, g.Y, 1f);
    }

    // ── RigidBody2D ──

    [Fact]
    public void RigidBody2D_Dynamic_FallsWithGravity()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("FallingBox");
        go.Transform.LocalPosition = new Vector2(400, 100);
        go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        go.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });

        float startY = go.Transform.Position.Y;

        // 步进物理 1 秒
        for (int i = 0; i < 60; i++)
            scene.FixedUpdate(1f / 60f);

        float endY = go.Transform.Position.Y;
        Assert.True(endY > startY, $"Dynamic body should fall down. Start={startY}, End={endY}");
    }

    [Fact]
    public void RigidBody2D_Static_DoesNotMove()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("StaticBox");
        go.Transform.LocalPosition = new Vector2(200, 300);
        go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Static });
        go.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });

        float startY = go.Transform.Position.Y;

        for (int i = 0; i < 60; i++)
            scene.FixedUpdate(1f / 60f);

        float endY = go.Transform.Position.Y;
        Assert.Equal(startY, endY, 0.1f);
    }

    [Fact]
    public void RigidBody2D_Kinematic_CanBeMovedByCode()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("KinematicBox");
        go.Transform.LocalPosition = new Vector2(100, 100);
        go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Kinematic });
        go.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });

        // 代码移动 Kinematic 物体
        go.Transform.LocalPosition = new Vector2(150, 200);

        for (int i = 0; i < 10; i++)
            scene.FixedUpdate(1f / 60f);

        Assert.Equal(150f, go.Transform.Position.X, 0.1f);
        Assert.Equal(200f, go.Transform.Position.Y, 0.1f);
    }

    [Fact]
    public void RigidBody2D_Collision_StopsFalling()
    {
        var scene = new Scene();
        scene.PhysicsWorld.Gravity = new Vector2(0, 500); // 强重力，加速测试

        // 地面（Static）
        var ground = scene.CreateGameObject("Ground");
        ground.Transform.LocalPosition = new Vector2(400, 500);
        ground.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Static });
        ground.AddComponent(new BoxCollider2D { Width = 800, Height = 32 });

        // 掉落物（Dynamic）
        var box = scene.CreateGameObject("Box");
        box.Transform.LocalPosition = new Vector2(400, 100);
        box.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });

        float startY = box.Transform.Position.Y;

        // 模拟 2 秒
        for (int i = 0; i < 120; i++)
            scene.FixedUpdate(1f / 60f);

        float endY = box.Transform.Position.Y;

        // 掉落物应该停在地面附近（不会穿透）
        Assert.True(endY > startY, "Box should have fallen");
        Assert.True(endY < 480, $"Box should not fall through ground. EndY={endY}");
    }

    // ── Collider ──

    [Fact]
    public void BoxCollider2D_CreatesFixture()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("Box");
        go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        var collider = go.AddComponent(new BoxCollider2D { Width = 64, Height = 32, Density = 2f });

        Assert.NotNull(collider);
        Assert.Equal(64f, collider.Width);
        Assert.Equal(32f, collider.Height);
        Assert.Equal(2f, collider.Density);
    }

    [Fact]
    public void CircleCollider2D_CreatesFixture()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("Circle");
        go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        var collider = go.AddComponent(new CircleCollider2D { Radius = 16, Density = 1.5f });

        Assert.NotNull(collider);
        Assert.Equal(16f, collider.Radius);
        Assert.Equal(1.5f, collider.Density);
    }

    [Fact]
    public void Collider_RequiresRigidBody2D()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("Orphan");
        // 没有 RigidBody2D，添加 Collider 应该抛出异常
        Assert.Throws<InvalidOperationException>(() =>
            go.AddComponent(new BoxCollider2D()));
    }

    // ── 综合 ──

    [Fact]
    public void RigidBody2D_LinearVelocity_CanBeSet()
    {
        var scene = new Scene();
        var go = scene.CreateGameObject("MovingBox");
        go.Transform.LocalPosition = new Vector2(0, 0);
        go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        go.AddComponent(new BoxCollider2D { Width = 32, Height = 32 });

        var rb = go.GetComponent<RigidBody2D>()!;
        rb.LinearVelocity = new Vector2(100, 0);

        // 步进 1 秒
        for (int i = 0; i < 60; i++)
            scene.FixedUpdate(1f / 60f);

        Assert.True(go.Transform.Position.X > 0, "Box should have moved to the right");
    }
}
