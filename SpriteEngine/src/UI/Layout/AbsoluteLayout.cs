namespace SpriteEngine.UI;

/// <summary>
/// 绝对定位布局引擎。子元素使用 LocalX/LocalY/Width/Height 直接定位。
/// 适用于浮动窗口、弹窗、拖拽面板等需要精确控制位置的 UI。
/// </summary>
public class AbsoluteLayout : ILayoutEngine
{
    public void Measure(UIContainer container)
    {
        foreach (var child in container.Children)
        {
            if (child.Visible) child.Measure();
        }
    }

    public void Layout(UIContainer container)
    {
        // 绝对定位不修改任何子元素的位置和大小
        // 完全依赖使用者显式设置 LocalX/LocalY/Width/Height
    }
}
