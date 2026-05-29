using SpriteCore.Utils;

namespace SpriteEngine.Scenes;

/// <summary>
/// 场景：管理一组 GameObject 的容器。
/// 支持添加/移除对象、按名称/标签/类型查找、层级更新。
/// 对应 Unity 的 Scene / p5engine 的 Scene。
/// </summary>
public class Scene
{
    private readonly List<GameObject> _rootObjects = new();
    private readonly Dictionary<string, List<GameObject>> _tagIndex = new();

    /// <summary>场景名称</summary>
    public string Name { get; set; }

    /// <summary>只读根对象列表</summary>
    public IReadOnlyList<GameObject> RootObjects => _rootObjects;

    /// <summary>场景中所有对象（包括子对象）的扁平枚举</summary>
    public IEnumerable<GameObject> AllObjects => EnumerateAll();

    public Scene(string name = "Scene")
    {
        Name = name;
    }

    // ── 对象管理 ──

    /// <summary>在场景中创建新的 GameObject</summary>
    public GameObject CreateGameObject(string name = "GameObject")
    {
        var go = new GameObject(name);
        AddGameObject(go);
        return go;
    }

    /// <summary>将已有 GameObject 添加到场景</summary>
    public void AddGameObject(GameObject go)
    {
        if (go.Scene != null && go.Scene != this)
            throw new InvalidOperationException($"GameObject '{go.Name}' is already in another scene.");

        if (go.Scene == this) return;

        go.Scene = this;
        _rootObjects.Add(go);
        AddToTagIndex(go);

        go.InvokeStart();
    }

    /// <summary>从场景中移除 GameObject（递归移除子对象）</summary>
    public void RemoveGameObject(GameObject go)
    {
        if (go.Scene != this) return;

        go.InvokeOnDestroy();
        go.Scene = null;
        _rootObjects.Remove(go);
        RemoveFromTagIndex(go);
    }

    /// <summary>清空场景中所有对象</summary>
    public void Clear()
    {
        foreach (var go in _rootObjects.ToList())
            RemoveGameObject(go);
        _tagIndex.Clear();
    }

    // ── 查找 ──

    /// <summary>按名称查找（递归搜索所有层级）</summary>
    public GameObject? Find(string name)
    {
        foreach (var go in _rootObjects)
        {
            if (go.Name == name) return go;
            var found = go.FindChild(name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>按标签查找（返回第一个）</summary>
    public GameObject? FindWithTag(string tag)
    {
        if (!_tagIndex.TryGetValue(tag, out var list) || list.Count == 0)
            return null;
        return list[0];
    }

    /// <summary>按标签查找（返回所有）</summary>
    public IEnumerable<GameObject> FindAllWithTag(string tag)
    {
        if (!_tagIndex.TryGetValue(tag, out var list))
            return Enumerable.Empty<GameObject>();
        return list.ToList(); // 防御性拷贝
    }

    /// <summary>按组件类型查找 GameObject（返回第一个匹配）</summary>
    public GameObject? FindWithComponent<T>() where T : Component
        => AllObjects.FirstOrDefault(go => go.GetComponent<T>() != null);

    /// <summary>按组件类型查找所有 GameObject</summary>
    public IEnumerable<GameObject> FindAllWithComponent<T>() where T : Component
        => AllObjects.Where(go => go.GetComponent<T>() != null);

    // ── 更新 ──

    /// <summary>调用场景中所有对象的 Update</summary>
    public void Update(float dt)
    {
        // 快照遍历，防止 Update 中修改集合
        foreach (var go in _rootObjects.ToList())
            go.InvokeUpdate(dt);
    }

    /// <summary>调用场景中所有对象的 FixedUpdate</summary>
    public void FixedUpdate(float fixedDt)
    {
        foreach (var go in _rootObjects.ToList())
            go.InvokeFixedUpdate(fixedDt);
    }

    /// <summary>渲染场景中所有 SpriteRenderer</summary>
    public void Render(SpriteCore.Graphics.SPGraphics graphics)
    {
        foreach (var go in AllObjects)
        {
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer?.Render(graphics);
        }
    }

    // ── 内部 ──

    private void AddToTagIndex(GameObject go)
    {
        if (!_tagIndex.TryGetValue(go.Tag, out var list))
        {
            list = new List<GameObject>();
            _tagIndex[go.Tag] = list;
        }
        list.Add(go);

        foreach (var child in go.Children)
            AddToTagIndex(child);
    }

    private void RemoveFromTagIndex(GameObject go)
    {
        if (_tagIndex.TryGetValue(go.Tag, out var list))
        {
            list.Remove(go);
            if (list.Count == 0)
                _tagIndex.Remove(go.Tag);
        }

        foreach (var child in go.Children.ToList())
            RemoveFromTagIndex(child);
    }

    internal void RemoveFromTagIndexByRef(GameObject go)
    {
        if (_tagIndex.TryGetValue(go.Tag, out var list))
        {
            list.Remove(go);
            if (list.Count == 0)
                _tagIndex.Remove(go.Tag);
        }
    }

    internal void AddToTagIndexByRef(GameObject go)
    {
        if (!_tagIndex.TryGetValue(go.Tag, out var list))
        {
            list = new List<GameObject>();
            _tagIndex[go.Tag] = list;
        }
        if (!list.Contains(go))
            list.Add(go);
    }

    private IEnumerable<GameObject> EnumerateAll()
    {
        foreach (var go in _rootObjects)
        {
            yield return go;
            foreach (var child in EnumerateChildren(go))
                yield return child;
        }
    }

    private IEnumerable<GameObject> EnumerateChildren(GameObject parent)
    {
        foreach (var child in parent.Children)
        {
            yield return child;
            foreach (var grandChild in EnumerateChildren(child))
                yield return grandChild;
        }
    }
}
