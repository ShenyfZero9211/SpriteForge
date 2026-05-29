using nkast.Aether.Physics2D.Dynamics;
using SpriteCore.Math;

namespace SpriteEngine.Physics;

/// <summary>
/// 2D 物理世界的封装。每个 Scene 持有一个实例。
/// 管理重力、时间步、以及所有 RigidBody2D 的同步。
/// </summary>
public class PhysicsWorld2D
{
    private readonly World _world;
    private readonly List<RigidBody2D> _bodies = new();

    /// <summary>重力加速度（像素/秒²），默认向下 9.8 * 32</summary>
    public Vector2 Gravity
    {
        get => PhysicsUtils.ToPixels(_world.Gravity);
        set => _world.Gravity = PhysicsUtils.ToPhysics(value);
    }

    /// <summary>时间缩放（1 = 正常，0 = 暂停，2 = 2倍速）</summary>
    public float TimeScale { get; set; } = 1f;

    public PhysicsWorld2D()
    {
        _world = new World(PhysicsUtils.ToPhysics(new Vector2(0, 9.8f * 32)));
    }

    /// <summary>
    /// 注册一个 RigidBody2D。由 RigidBody2D.Start 自动调用。
    /// </summary>
    internal void Register(RigidBody2D rigidBody)
    {
        if (!_bodies.Contains(rigidBody))
            _bodies.Add(rigidBody);
    }

    /// <summary>
    /// 注销一个 RigidBody2D。由 RigidBody2D.OnDestroy 自动调用。
    /// </summary>
    internal void Unregister(RigidBody2D rigidBody)
    {
        _bodies.Remove(rigidBody);
    }

    /// <summary>
    /// 执行一次物理模拟步进。
    /// 内部顺序：SyncToBodies → Step → SyncFromBodies。
    /// </summary>
    public void Step(float fixedDt)
    {
        if (TimeScale <= 0) return;

        // 1. 将 Transform 的变动同步到物理 Body（所有类型）
        foreach (var rb in _bodies)
            rb.SyncToBody();

        // 2. 步进物理世界
        _world.Step(fixedDt * TimeScale);

        // 3. 将物理模拟结果同步回 Transform（Dynamic / Kinematic）
        foreach (var rb in _bodies)
            rb.SyncFromBody();
    }

    /// <summary>创建 Aether Body。由 RigidBody2D 内部调用。</summary>
    internal Body CreateBody(Vector2 position, float rotation, BodyType bodyType)
    {
        var physPos = PhysicsUtils.ToPhysics(position);
        var physRot = MathUtils.Radians(rotation);
        return _world.CreateBody(physPos, physRot, bodyType);
    }

    /// <summary>移除 Aether Body。由 RigidBody2D 内部调用。</summary>
    internal void RemoveBody(Body body)
    {
        _world.Remove(body);
    }

    /// <summary>获取当前世界中的所有物理体（Aether 原生）</summary>
    public IEnumerable<Body> GetAllBodies() => _world.BodyList;
}
