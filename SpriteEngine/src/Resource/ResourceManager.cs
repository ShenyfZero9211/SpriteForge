using SpriteCore.Graphics;

namespace SpriteEngine.Resource;

/// <summary>
/// 引用计数式资源管理器。
/// 设计参考 libgdx AssetManager：统一加载、缓存、释放。
/// 线程安全：所有公共方法内部加锁。
/// </summary>
public class ResourceManager
{
    private readonly Dictionary<string, IResource> _resources = new();
    private readonly Dictionary<string, int> _refCounts = new();
    private readonly object _lock = new();

    /// <summary>已加载资源数量</summary>
    public int LoadedCount
    {
        get { lock (_lock) return _resources.Count; }
    }

    /// <summary>
    /// 同步加载资源。若已缓存则引用计数 +1 并返回现有实例。
    /// </summary>
    public T Load<T>(string path) where T : class, IResource
    {
        var fullPath = System.IO.Path.GetFullPath(path);

        lock (_lock)
        {
            if (_resources.TryGetValue(fullPath, out var existing))
            {
                _refCounts[fullPath]++;
                return (T)existing;
            }
        }

        var resource = CreateResource<T>(fullPath);

        lock (_lock)
        {
            // 双重检查：其他线程可能已加载
            if (_resources.TryGetValue(fullPath, out var existing))
            {
                resource.Dispose();
                _refCounts[fullPath]++;
                return (T)existing;
            }

            _resources[fullPath] = resource;
            _refCounts[fullPath] = 1;
            return resource;
        }
    }

    /// <summary>
    /// 异步加载资源。后台线程执行文件 IO，完成后加入缓存。
    /// </summary>
    public Task<T> LoadAsync<T>(string path) where T : class, IResource
    {
        return Task.Run(() => Load<T>(path));
    }

    /// <summary>
    /// 获取已加载资源（不增加引用计数）。若未加载则返回 null。
    /// </summary>
    public T? Get<T>(string path) where T : class, IResource
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        lock (_lock)
        {
            return _resources.TryGetValue(fullPath, out var r) ? (T)r : null;
        }
    }

    /// <summary>
    /// 手动注册一个已创建的资源（不调用 CreateResource）。引用计数设为 1。
    /// </summary>
    public void Register<T>(string path, T resource) where T : class, IResource
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        lock (_lock)
        {
            _resources[fullPath] = resource;
            _refCounts[fullPath] = 1;
        }
    }

    /// <summary>
    /// 卸载资源。引用计数 -1，归零时真正 Dispose 并移除缓存。
    /// </summary>
    public void Unload(string path)
    {
        var fullPath = System.IO.Path.GetFullPath(path);

        lock (_lock)
        {
            if (!_refCounts.TryGetValue(fullPath, out var count)) return;

            count--;
            if (count <= 0)
            {
                if (_resources.TryGetValue(fullPath, out var resource))
                {
                    resource.Dispose();
                    _resources.Remove(fullPath);
                }
                _refCounts.Remove(fullPath);
            }
            else
            {
                _refCounts[fullPath] = count;
            }
        }
    }

    /// <summary>
    /// 卸载资源（通过实例查找路径）。
    /// </summary>
    public void Unload(IResource resource)
    {
        if (resource == null) return;
        Unload(resource.Path);
    }

    /// <summary>
    /// 清空所有资源（无论引用计数）。用于场景切换等强制清理场景。
    /// </summary>
    public void DisposeAll()
    {
        lock (_lock)
        {
            foreach (var r in _resources.Values)
                r.Dispose();
            _resources.Clear();
            _refCounts.Clear();
        }
    }

    private static T CreateResource<T>(string fullPath) where T : class, IResource
    {
        if (typeof(T) == typeof(Texture2D))
        {
            var texture = SPTexture.Load(fullPath);
            return (T)(IResource)new Texture2D(fullPath, texture);
        }

        throw new NotSupportedException($"Resource type {typeof(T).Name} is not supported.");
    }
}
