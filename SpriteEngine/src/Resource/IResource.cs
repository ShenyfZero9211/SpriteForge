namespace SpriteEngine.Resource;

/// <summary>
/// 所有可管理资源的公共接口。
/// 资源生命周期由 ResourceManager 通过引用计数控制。
/// </summary>
public interface IResource : IDisposable
{
    /// <summary>资源的唯一标识路径（通常是文件路径）</summary>
    string Path { get; }
}
