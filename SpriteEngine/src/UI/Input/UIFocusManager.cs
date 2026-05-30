namespace SpriteEngine.UI;

/// <summary>
/// UI 焦点管理器。支持 Tab 循环导航和方向键导航。
/// 受 p5engine FocusManager 启发。
/// </summary>
public class UIFocusManager
{
    private UIElement? _focused;

    public UIElement? Focused => _focused;

    public void SetFocused(UIElement? element)
    {
        if (_focused == element) return;

        if (_focused != null)
            _focused.OnEvent(UIEvent.FocusLost());

        _focused = element;

        if (_focused != null)
            _focused.OnEvent(UIEvent.FocusGained());
    }

    public void ClearFocus() => SetFocused(null);

    /// <summary>Tab 键：聚焦下一个可聚焦元素</summary>
    public void FocusNext(UIContainer root)
    {
        var focusables = CollectFocusables(root);
        if (focusables.Count == 0) return;

        int idx = _focused != null ? focusables.IndexOf(_focused) : -1;
        idx = (idx + 1) % focusables.Count;
        SetFocused(focusables[idx]);
    }

    /// <summary>Shift+Tab：聚焦上一个可聚焦元素</summary>
    public void FocusPrevious(UIContainer root)
    {
        var focusables = CollectFocusables(root);
        if (focusables.Count == 0) return;

        int idx = _focused != null ? focusables.IndexOf(_focused) : 0;
        idx = (idx - 1 + focusables.Count) % focusables.Count;
        SetFocused(focusables[idx]);
    }

    /// <summary>方向键导航：向指定方向查找最近的可聚焦元素</summary>
    public void FocusDirection(UIContainer root, UINavigationDirection direction)
    {
        if (_focused == null)
        {
            FocusNext(root);
            return;
        }

        var focusables = CollectFocusables(root);
        UIElement? best = null;
        float bestScore = float.MaxValue;

        float fx = _focused.AbsoluteX + _focused.Width / 2;
        float fy = _focused.AbsoluteY + _focused.Height / 2;

        foreach (var c in focusables)
        {
            if (c == _focused) continue;
            float cx = c.AbsoluteX + c.Width / 2;
            float cy = c.AbsoluteY + c.Height / 2;
            float dx = cx - fx;
            float dy = cy - fy;

            bool inDirection = direction switch
            {
                UINavigationDirection.Up => dy < 0 && Math.Abs(dy) > Math.Abs(dx),
                UINavigationDirection.Down => dy > 0 && Math.Abs(dy) > Math.Abs(dx),
                UINavigationDirection.Left => dx < 0 && Math.Abs(dx) > Math.Abs(dy),
                UINavigationDirection.Right => dx > 0 && Math.Abs(dx) > Math.Abs(dy),
                _ => false,
            };

            if (!inDirection) continue;

            float score = Math.Abs(dx) + Math.Abs(dy);
            if (score < bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        if (best != null)
            SetFocused(best);
    }

    private static List<UIElement> CollectFocusables(UIContainer root)
    {
        var result = new List<UIElement>();
        CollectRecursive(root, result);
        return result;
    }

    private static void CollectRecursive(UIElement element, List<UIElement> result)
    {
        if (element is UIContainer container)
        {
            foreach (var child in container.Children)
                CollectRecursive(child, result);
        }
        if (element.Focusable && element.Visible && element.Enabled)
            result.Add(element);
    }
}

public enum UINavigationDirection
{
    Up, Down, Left, Right
}
