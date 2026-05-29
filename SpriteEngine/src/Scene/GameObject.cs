using SpriteCore.Utils;

namespace SpriteEngine.Scenes;

/// <summary>
/// 游戏对象。是场景中所有实体的容器，本身不携带逻辑，通过附加 Component 实现功能。
/// 支持父子层级、Tag 标签、启用/禁用。
/// 对应 Unity 的 GameObject / p5engine 的 GameObject。
/// </summary>
public class GameObject
{
    private static int _nextId = 1;

    private readonly List<Component> _components = new();
    private readonly List<GameObject> _children = new();

    /// <summary>唯一 ID（只增整数）</summary>
    public int Id { get; }

    /// <summary>对象名称</summary>
    public string Name { get; set; }

    private string _tag = "Untagged";

    /// <summary>标签（用于快速查找，修改时自动同步场景索引）</summary>
    public string Tag
    {
        get => _tag;
        set
        {
            if (_tag == value) return;
            Scene?.RemoveFromTagIndexByRef(this);
            _tag = value;
            Scene?.AddToTagIndexByRef(this);
        }
    }

    /// <summary>是否启用（禁用后自身及子对象的 Update 不再调用）</summary>
    public bool Active { get; set; } = true;

    /// <summary>所属场景</summary>
    public Scene? Scene { get; internal set; }

    /// <summary>父对象（null 表示根级）</summary>
    public GameObject? Parent { get; private set; }

    /// <summary>只读子对象列表</summary>
    public IReadOnlyList<GameObject> Children => _children;

    /// <summary>快捷访问 Transform 组件（每个 GameObject 必有一个）</summary>
    public Transform Transform { get; private set; } = null!;

    /// <summary>只读组件列表</summary>
    public IReadOnlyList<Component> Components => _components;

    public GameObject(string name = "GameObject")
    {
        Id = _nextId++;
        Name = name;

        // 每个 GameObject 自动附加 Transform
        var t = new Transform();
        AddComponentInternal(t);
        Transform = t;
    }

    // ── 组件管理 ──

    /// <summary>添加组件（泛型）</summary>
    public T AddComponent<T>() where T : Component, new()
    {
        var comp = new T();
        AddComponentInternal(comp);
        return comp;
    }

    /// <summary>添加组件（实例）</summary>
    public T AddComponent<T>(T component) where T : Component
    {
        AddComponentInternal(component);
        return component;
    }

    private void AddComponentInternal(Component component)
    {
        if (component.GameObject != null && component.GameObject != this)
            throw new InvalidOperationException($"Component {component.GetType().Name} is already attached to another GameObject.");

        component.GameObject = this;
        _components.Add(component);

        if (Scene != null)
            component.Start();
    }

    /// <summary>获取组件（泛型，返回第一个匹配）</summary>
    public T? GetComponent<T>() where T : Component
        => _components.OfType<T>().FirstOrDefault();

    /// <summary>获取所有匹配组件</summary>
    public IEnumerable<T> GetComponents<T>() where T : Component
        => _components.OfType<T>();

    /// <summary>移除组件</summary>
    public bool RemoveComponent<T>() where T : Component
    {
        var comp = GetComponent<T>();
        if (comp == null) return false;
        RemoveComponentInternal(comp);
        return true;
    }

    /// <summary>移除指定组件</summary>
    public void RemoveComponent(Component component)
    {
        if (!_components.Contains(component))
            throw new ArgumentException("Component not found on this GameObject.");
        RemoveComponentInternal(component);
    }

    private void RemoveComponentInternal(Component component)
    {
        component.OnDestroy();
        component.GameObject = null;
        _components.Remove(component);
    }

    // ── 层级管理 ──

    /// <summary>设置父对象</summary>
    public void SetParent(GameObject? parent)
    {
        if (parent == this)
            throw new InvalidOperationException("GameObject cannot be its own parent.");

        if (Parent != null)
            Parent._children.Remove(this);

        Parent = parent;
        parent?._children.Add(this);
    }

    /// <summary>查找子对象（按名称，递归）</summary>
    public GameObject? FindChild(string name)
    {
        foreach (var child in _children)
        {
            if (child.Name == name) return child;
            var found = child.FindChild(name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>查找子对象（按名称，仅直接子对象）</summary>
    public GameObject? FindDirectChild(string name)
        => _children.FirstOrDefault(c => c.Name == name);

    // ── 生命周期 ──

    public void InvokeStart()
    {
        if (!Active) return;
        foreach (var c in _components)
        {
            if (c.Enabled) c.Start();
        }
        foreach (var child in _children)
            child.InvokeStart();
    }

    public void InvokeUpdate(float dt)
    {
        if (!Active) return;
        foreach (var c in _components)
        {
            if (c.Enabled) c.Update(dt);
        }
        foreach (var child in _children)
            child.InvokeUpdate(dt);
    }

    public void InvokeFixedUpdate(float fixedDt)
    {
        if (!Active) return;
        foreach (var c in _components)
        {
            if (c.Enabled) c.FixedUpdate(fixedDt);
        }
        foreach (var child in _children)
            child.InvokeFixedUpdate(fixedDt);
    }

    public void InvokeOnDestroy()
    {
        foreach (var child in _children.ToList())
            child.InvokeOnDestroy();
        foreach (var c in _components.ToList())
            c.OnDestroy();
    }

    public override string ToString() => $"[{Id}] {Name} (Tag={Tag}, Active={Active})";
}
