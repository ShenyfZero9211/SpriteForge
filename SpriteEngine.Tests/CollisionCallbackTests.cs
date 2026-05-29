using SpriteCore.Math;
using SpriteEngine.Physics;
using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class CollisionCallbackTests
{
    /// <summary>记录碰撞事件的测试组件</summary>
    private class CollisionRecorder : Component
    {
        public bool EnterCalled { get; set; }
        public bool ExitCalled { get; set; }
        public bool TriggerCalled { get; set; }
        public Collision2D? LastCollision { get; set; }

        public override void OnCollisionEnter(Collision2D collision)
        {
            EnterCalled = true;
            LastCollision = collision;
        }

        public override void OnCollisionExit(Collision2D collision)
        {
            ExitCalled = true;
            LastCollision = collision;
        }

        public override void OnTriggerEnter(Collision2D collision)
        {
            TriggerCalled = true;
            LastCollision = collision;
        }
    }

    [Fact]
    public void OnCollisionEnter_Fires_WhenDynamicBodiesCollide()
    {
        var scene = new Scene();
        scene.PhysicsWorld.Gravity = Vector2.Zero; // 无重力，避免下落干扰

        // Box1 在 (400, 300)
        var box1 = scene.CreateGameObject("Box1");
        box1.Transform.LocalPosition = new Vector2(400, 300);
        box1.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box1.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });
        var recorder1 = box1.AddComponent<CollisionRecorder>();

        // Box2 在 (440, 300)，与 Box1 水平重叠 24px
        var box2 = scene.CreateGameObject("Box2");
        box2.Transform.LocalPosition = new Vector2(440, 300);
        box2.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box2.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });
        var recorder2 = box2.AddComponent<CollisionRecorder>();

        scene.FixedUpdate(1f / 60f);

        Assert.True(recorder1.EnterCalled, "Box1 should receive OnCollisionEnter");
        Assert.True(recorder2.EnterCalled, "Box2 should receive OnCollisionEnter");
        Assert.NotNull(recorder1.LastCollision);
        Assert.Equal("Box2", recorder1.LastCollision!.Other?.Name);
    }

    [Fact]
    public void OnTriggerEnter_Fires_ForSensor()
    {
        var scene = new Scene();
        scene.PhysicsWorld.Gravity = Vector2.Zero;

        // 触发器区域（Static Sensor）
        var trigger = scene.CreateGameObject("Trigger");
        trigger.Transform.LocalPosition = new Vector2(400, 300);
        trigger.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Static });
        trigger.AddComponent(new BoxCollider2D { Width = 100, Height = 100, IsSensor = true });
        var recorder = trigger.AddComponent<CollisionRecorder>();

        // Dynamic 物体进入触发区域
        var box = scene.CreateGameObject("Box");
        box.Transform.LocalPosition = new Vector2(400, 300);
        box.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box.AddComponent(new BoxCollider2D { Width = 32, Height = 32 });

        scene.FixedUpdate(1f / 60f);

        Assert.True(recorder.TriggerCalled, "Trigger should receive OnTriggerEnter");
        Assert.False(recorder.EnterCalled, "Trigger should NOT receive OnCollisionEnter");
    }

    [Fact]
    public void OnCollisionExit_Fires_WhenBodiesSeparate()
    {
        var scene = new Scene();
        scene.PhysicsWorld.Gravity = Vector2.Zero;

        // Box1
        var box1 = scene.CreateGameObject("Box1");
        box1.Transform.LocalPosition = new Vector2(400, 300);
        var rb1 = box1.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box1.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });
        var recorder1 = box1.AddComponent<CollisionRecorder>();

        // Box2
        var box2 = scene.CreateGameObject("Box2");
        box2.Transform.LocalPosition = new Vector2(440, 300);
        var rb2 = box2.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box2.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });
        var recorder2 = box2.AddComponent<CollisionRecorder>();

        // 先碰撞
        scene.FixedUpdate(1f / 60f);
        Assert.True(recorder1.EnterCalled);
        Assert.True(recorder2.EnterCalled);

        // 给一个相反的力让它们分离
        rb1.LinearVelocity = new Vector2(-500, 0);
        rb2.LinearVelocity = new Vector2(500, 0);

        // 步进足够长时间让它们分开
        for (int i = 0; i < 60; i++)
            scene.FixedUpdate(1f / 60f);

        Assert.True(recorder1.ExitCalled, "Box1 should receive OnCollisionExit after separation");
        Assert.True(recorder2.ExitCalled, "Box2 should receive OnCollisionExit after separation");
    }

    [Fact]
    public void Collision_ContainsContactPointAndNormal()
    {
        var scene = new Scene();
        scene.PhysicsWorld.Gravity = Vector2.Zero;

        var box1 = scene.CreateGameObject("Box1");
        box1.Transform.LocalPosition = new Vector2(400, 300);
        box1.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box1.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });
        var recorder = box1.AddComponent<CollisionRecorder>();

        var box2 = scene.CreateGameObject("Box2");
        box2.Transform.LocalPosition = new Vector2(440, 300);
        box2.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box2.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });

        scene.FixedUpdate(1f / 60f);

        Assert.NotNull(recorder.LastCollision);
        // 接触点应该在两个盒子之间（约 X=420）
        Assert.True(recorder.LastCollision!.ContactPoint.X > 400 && recorder.LastCollision.ContactPoint.X < 440,
            $"ContactPoint.X should be between 400 and 440, got {recorder.LastCollision.ContactPoint.X}");
        // 法线应该在水平方向
        Assert.True(Math.Abs(recorder.LastCollision.Normal.X) > 0.5f,
            $"Normal.X should be significant, got {recorder.LastCollision.Normal}");
    }

    [Fact]
    public void OnCollisionEnter_NotFired_WithoutCollider()
    {
        var scene = new Scene();
        scene.PhysicsWorld.Gravity = Vector2.Zero;

        // 没有 Collider 的物体
        var box1 = scene.CreateGameObject("Box1");
        box1.Transform.LocalPosition = new Vector2(400, 300);
        box1.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        // 不加 Collider
        var recorder1 = box1.AddComponent<CollisionRecorder>();

        var box2 = scene.CreateGameObject("Box2");
        box2.Transform.LocalPosition = new Vector2(440, 300);
        box2.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        box2.AddComponent(new BoxCollider2D { Width = 64, Height = 64 });

        scene.FixedUpdate(1f / 60f);

        // Box1 没有 Collider，所以 FixtureList 为空，不会发生碰撞
        Assert.False(recorder1.EnterCalled, "Body without collider should not receive collision events");
    }
}
