namespace SpriteEngine.UI;

/// <summary>
/// Id-based 状态存储。受 egui 隐式状态存储启发。
/// 用于文本输入框、滚动视图、下拉框等需要跨帧持久状态的 widgets。
/// </summary>
public class UIStateStorage
{
    private readonly Dictionary<string, object> _state = new();

    /// <summary>获取或创建指定 Id 的状态。如果不存在，用 defaultValue 初始化。</summary>
    public T GetOrCreate<T>(string id, T defaultValue) where T : notnull
    {
        if (_state.TryGetValue(id, out var existing) && existing is T t)
            return t;
        _state[id] = defaultValue;
        return defaultValue;
    }

    /// <summary>设置指定 Id 的状态。</summary>
    public void Set<T>(string id, T value) where T : notnull
    {
        _state[id] = value;
    }

    /// <summary>尝试获取指定 Id 的状态。</summary>
    public bool TryGet<T>(string id, out T value) where T : notnull
    {
        if (_state.TryGetValue(id, out var existing) && existing is T t)
        {
            value = t;
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>移除指定 Id 的状态。</summary>
    public bool Remove(string id) => _state.Remove(id);

    /// <summary>清空所有状态。</summary>
    public void Clear() => _state.Clear();
}
