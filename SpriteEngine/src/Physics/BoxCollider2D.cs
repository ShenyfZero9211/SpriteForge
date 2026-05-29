using nkast.Aether.Physics2D.Dynamics;
using SpriteCore.Math;
using SpriteEngine.Scenes;

namespace SpriteEngine.Physics;

/// <summary>
/// 矩形碰撞体组件。附加到已有 RigidBody2D 的 GameObject 上，
/// 在物理世界中创建一个矩形 Fixture。
/// </summary>
public class BoxCollider2D : Component
{
    /// <summary>矩形宽度（像素）</summary>
    public float Width { get; set; } = 64;

    /// <summary>矩形高度（像素）</summary>
    public float Height { get; set; } = 64;

    /// <summary>相对于 GameObject 中心的偏移（像素）</summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    /// <summary>密度（kg/m²）</summary>
    public float Density { get; set; } = 1f;

    /// <summary>摩擦系数（0 = 冰面，1 = 高摩擦）</summary>
    public float Friction { get; set; } = 0.3f;

    /// <summary>弹性系数（0 = 无弹性，1 = 完全弹性）</summary>
    public float Restitution { get; set; } = 0f;

    private Fixture? _fixture;

    public override void Start()
    {
        var rb = GameObject?.GetComponent<RigidBody2D>();
        if (rb?.Body == null)
            throw new InvalidOperationException(
                "BoxCollider2D requires a RigidBody2D component to be added first.");

        _fixture = rb.Body.CreateRectangle(
            PhysicsUtils.ToPhysics(Width),
            PhysicsUtils.ToPhysics(Height),
            Density,
            PhysicsUtils.ToPhysics(Offset));

        _fixture.Friction = Friction;
        _fixture.Restitution = Restitution;
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
