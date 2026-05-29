namespace SpriteEngine.Scene;

/// <summary>
/// 场景管理器：管理场景的加载、切换、卸载。
/// 单例模式（通过静态属性访问）。
/// </summary>
public class SceneManager
{
    private static SceneManager? _instance;
    public static SceneManager Instance => _instance ??= new SceneManager();

    private readonly Dictionary<string, Scene> _scenes = new();
    private Scene? _activeScene;

    /// <summary>当前激活的场景</summary>
    public Scene? ActiveScene => _activeScene;

    /// <summary>场景切换前触发</summary>
    public event Action<Scene?>? OnSceneWillChange;

    /// <summary>场景切换后触发</summary>
    public event Action<Scene>? OnSceneChanged;

    private SceneManager() { }

    /// <summary>注册一个场景（不激活）</summary>
    public void Register(string name, Scene scene)
    {
        _scenes[name] = scene;
    }

    /// <summary>加载并激活指定场景（销毁当前场景）</summary>
    public void Load(string name)
    {
        if (!_scenes.TryGetValue(name, out var scene))
            throw new KeyNotFoundException($"Scene '{name}' not found. Did you forget to Register it?");

        OnSceneWillChange?.Invoke(_activeScene);

        // 卸载当前场景
        _activeScene?.Clear();

        _activeScene = scene;
        OnSceneChanged?.Invoke(scene);
    }

    /// <summary>异步加载场景（预留接口，当前同步实现）</summary>
    public Task LoadAsync(string name)
    {
        Load(name);
        return Task.CompletedTask;
    }

    /// <summary>在当前场景中创建 GameObject 的快捷方法</summary>
    public GameObject? CreateGameObject(string name = "GameObject")
        => _activeScene?.CreateGameObject(name);

    /// <summary>按名称在当前场景中查找</summary>
    public GameObject? Find(string name)
        => _activeScene?.Find(name);

    /// <summary>按标签在当前场景中查找</summary>
    public GameObject? FindWithTag(string tag)
        => _activeScene?.FindWithTag(tag);

    /// <summary>更新当前场景（由外部游戏循环调用）</summary>
    public void Update(float dt) => _activeScene?.Update(dt);

    /// <summary>固定更新当前场景</summary>
    public void FixedUpdate(float fixedDt) => _activeScene?.FixedUpdate(fixedDt);
}
