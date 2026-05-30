namespace SpriteEngine.UI;

/// <summary>
/// 布局引擎接口。两步布局：Measure（测量期望尺寸）→ Layout（应用最终尺寸和位置）。
/// </summary>
public interface ILayoutEngine
{
    /// <summary>测量容器及其子元素的期望尺寸</summary>
    void Measure(UIContainer container);

    /// <summary>根据容器最终尺寸，计算并设置子元素的位置和大小</summary>
    void Layout(UIContainer container);
}
