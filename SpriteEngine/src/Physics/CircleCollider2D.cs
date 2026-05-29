using nkast.Aether.Physics2D.Dynamics;
using SpriteCore.Math;
using SpriteEngine.Scenes;

namespace SpriteEngine.Physics;

/// <summary>
/// 圆形碰撞体组件。附加到已有 RigidBody2D 的 GameObject 上，
/// 在物理世界中创建一个圆形 Fixture。
/// </summary>
public class CircleCollider2D : Component
{
    /// <summary>圆半径（像素）</summary>
    public float Radius { get; set; } = 32;

    /// <summary>相对于 GameObject 中心的偏移（像素）</summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    /// <summary>密度（kg/m²）</summary>
    public float Density { get; set; } = 1f;

    /// <summary>摩擦系数</summary>
    public float Friction { get; set; } = 0.3f;

    /// <summary>弹性系数</summary>
    public float Restitution { get; set; } = 0f;

    /// <summary>是否为触发器（触发器不阻挡物理运动，只触发事件）</summary>
    public bool IsSensor { get; set; }

    private Fixture? _fixture;

    public override void Start()
    {
        var rb = GameObject?.GetComponent<RigidBody2D>();
        if (rb?.Body == null)
            throw new InvalidOperationException(
                "CircleCollider2D requires a RigidBody2D component to be added first.");

        _fixture = rb.Body.CreateCircle(
            PhysicsUtils.ToPhysics(Radius),
            Density,
            PhysicsUtils.ToPhysics(Offset));

        _fixture.Friction = Friction;
        _fixture.Restitution = Restitution;
        _fixture.IsSensor = IsSensor;
    }

    public override void OnDestroy()
    {
        if (_fixture != null && _fixture.Body != null && _fixture.Body.World != null)
        {
            _fixture.Body.Remove(_fixture);
        }
        _fixture = null;
    }
}
