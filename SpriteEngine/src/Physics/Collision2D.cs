using SpriteCore.Math;
using SpriteEngine.Scenes;
using AetherBody = nkast.Aether.Physics2D.Dynamics.Body;
using AetherContact = nkast.Aether.Physics2D.Dynamics.Contacts.Contact;

namespace SpriteEngine.Physics;

/// <summary>
/// 2D 碰撞信息。在 OnCollisionEnter / OnCollisionExit / OnTriggerEnter 中传递。
/// </summary>
public class Collision2D
{
    /// <summary>对方 GameObject</summary>
    public GameObject? Other { get; }

    /// <summary>对方 RigidBody2D</summary>
    public RigidBody2D? OtherRigidBody { get; }

    /// <summary>碰撞法线（像素空间，从 self 指向 other）</summary>
    public Vector2 Normal { get; }

    /// <summary>接触点（像素空间）</summary>
    public Vector2 ContactPoint { get; }

    /// <summary>相对速度（像素/秒，other - self）</summary>
    public Vector2 RelativeVelocity { get; }

    /// <summary>是否为触发器碰撞（任一 Fixture.IsSensor）</summary>
    public bool IsTrigger { get; }

    internal Collision2D(AetherContact contact, RigidBody2D self, RigidBody2D other)
    {
        Other = other.GameObject;
        OtherRigidBody = other;
        IsTrigger = contact.FixtureA.IsSensor || contact.FixtureB.IsSensor;

        contact.GetWorldManifold(out var normal, out var points);
        Normal = PhysicsUtils.ToPixels(normal);

        // FixedArray2 通过索引器访问，取第一个接触点
        ContactPoint = PhysicsUtils.ToPixels(points[0]);

        RelativeVelocity = other.LinearVelocity - self.LinearVelocity;
    }
}
