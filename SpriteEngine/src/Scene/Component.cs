using SpriteCore.Math;

namespace SpriteEngine.Scenes;

/// <summary>
/// 所有游戏组件的抽象基类。附加到 GameObject 上，随场景生命周期驱动。
/// 对应 Unity 的 MonoBehaviour / p5engine 的 Component。
/// </summary>
public abstract class Component
{
    /// <summary>宿主 GameObject（由 AddComponent 时自动设置）</summary>
    public GameObject? GameObject { get; internal set; }

    /// <summary>是否启用。禁用后 Update 不再被调用。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>快捷访问 Transform 组件</summary>
    public Transform? Transform => GameObject?.Transform;

    /// <summary>
    /// 组件被添加到 GameObject 后调用一次。
    /// 此时 GameObject 和 Transform 已可用。
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// 每帧调用一次（在 GameObject 启用状态下）。
    /// </summary>
    /// <param name="dt">上一帧到当前帧的秒数（DeltaTime）</param>
    public virtual void Update(float dt) { }

    /// <summary>
    /// 固定频率调用（用于物理相关逻辑）。
    /// </summary>
    /// <param name="fixedDt">固定时间步长（秒）</param>
    public virtual void FixedUpdate(float fixedDt) { }

    /// <summary>
    /// 组件被移除或 GameObject 被销毁时调用。
    /// 用于释放资源、取消事件订阅等。
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// 当组件所属 GameObject 进入场景时调用。
    /// </summary>
    public virtual void OnEnable() { }

    /// <summary>
    /// 当组件所属 GameObject 离开场景或被禁用时调用。
    /// </summary>
    public virtual void OnDisable() { }
}
