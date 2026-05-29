using SpriteCore.Math;
using SpriteEngine.Scenes;
using AetherBody = nkast.Aether.Physics2D.Dynamics.Body;
using AetherBodyType = nkast.Aether.Physics2D.Dynamics.BodyType;

namespace SpriteEngine.Physics;

/// <summary>
/// 2D 刚体组件。附加到 GameObject 后自动在物理世界中创建 Body。
/// 支持 Dynamic（受物理驱动）、Static（固定不动）、Kinematic（代码驱动）。
/// </summary>
public class RigidBody2D : Component
{
    private AetherBody? _body;

    /// <summary>Aether 原生 Body（只读，外部勿直接操作）</summary>
    public AetherBody? Body => _body;

    /// <summary>刚体类型</summary>
    public RigidBodyType BodyType { get; set; } = RigidBodyType.Dynamic;

    /// <summary>线性阻尼（空气阻力）</summary>
    public float LinearDamping { get; set; } = 0.1f;

    /// <summary>角阻尼</summary>
    public float AngularDamping { get; set; } = 0.1f;

    /// <summary>固定旋转（禁止物理旋转）</summary>
    public bool FixedRotation { get; set; }

    /// <summary>忽略重力</summary>
    public bool IgnoreGravity { get; set; }

    /// <summary>是否开启 CCD（连续碰撞检测，高速物体防穿透）</summary>
    public bool IsBullet { get; set; }

    /// <summary>线性速度（像素/秒）</summary>
    public Vector2 LinearVelocity
    {
        get => _body != null ? PhysicsUtils.ToPixels(_body.LinearVelocity) : Vector2.Zero;
        set
        {
            if (_body != null)
                _body.LinearVelocity = PhysicsUtils.ToPhysics(value);
        }
    }

    /// <summary>角速度（度/秒）</summary>
    public float AngularVelocity
    {
        get => _body != null ? MathUtils.Degrees(_body.AngularVelocity) : 0f;
        set
        {
            if (_body != null)
                _body.AngularVelocity = MathUtils.Radians(value);
        }
    }

    public override void Start()
    {
        var world = GameObject?.Scene?.PhysicsWorld;
        if (world == null) return;

        _body = world.CreateBody(Transform!.Position, Transform.Rotation, (AetherBodyType)BodyType);
        _body.LinearDamping = LinearDamping;
        _body.AngularDamping = AngularDamping;
        _body.FixedRotation = FixedRotation;
        _body.IgnoreGravity = IgnoreGravity;
        _body.IsBullet = IsBullet;

        world.Register(this);
    }

    public override void OnDestroy()
    {
        var world = GameObject?.Scene?.PhysicsWorld;
        if (world != null && _body != null)
        {
            world.Unregister(this);
            world.RemoveBody(_body);
        }
        _body = null;
    }

    /// <summary>施加力（世界空间，牛顿）</summary>
    public void ApplyForce(Vector2 force)
    {
        _body?.ApplyForce(PhysicsUtils.ToPhysics(force));
    }

    /// <summary>施加力（世界空间，作用于指定点）</summary>
    public void ApplyForce(Vector2 force, Vector2 point)
    {
        _body?.ApplyForce(PhysicsUtils.ToPhysics(force), PhysicsUtils.ToPhysics(point));
    }

    /// <summary>施加线性冲量</summary>
    public void ApplyLinearImpulse(Vector2 impulse)
    {
        _body?.ApplyLinearImpulse(PhysicsUtils.ToPhysics(impulse));
    }

    /// <summary>施加线性冲量（作用于指定点）</summary>
    public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
    {
        _body?.ApplyLinearImpulse(PhysicsUtils.ToPhysics(impulse), PhysicsUtils.ToPhysics(point));
    }

    /// <summary>施加扭矩</summary>
    public void ApplyTorque(float torque)
    {
        _body?.ApplyTorque(torque);
    }

    /// <summary>
    /// 在物理步进前调用：将 Transform 状态同步到 Aether Body。
    /// 对所有类型都执行，确保代码驱动的位移能被物理感知。
    /// </summary>
    internal void SyncToBody()
    {
        if (_body == null || Transform == null) return;
        _body.Position = PhysicsUtils.ToPhysics(Transform.Position);
        _body.Rotation = MathUtils.Radians(Transform.Rotation);
    }

    /// <summary>
    /// 在物理步进后调用：将 Aether Body 的模拟结果同步回 Transform。
    /// 对 Dynamic 和 Kinematic 执行；Static 不动，无需同步。
    /// </summary>
    internal void SyncFromBody()
    {
        if (_body == null || Transform == null) return;
        if ((int)_body.BodyType == 0) return;

        Transform.Position = PhysicsUtils.ToPixels(_body.Position);
        Transform.Rotation = MathUtils.Degrees(_body.Rotation);
    }
}

/// <summary>刚体类型（映射 Aether.BodyType）</summary>
public enum RigidBodyType
{
    Static = 0,
    Kinematic = 1,
    Dynamic = 2,
}
