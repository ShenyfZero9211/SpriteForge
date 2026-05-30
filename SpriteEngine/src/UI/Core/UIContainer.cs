using SpriteCore.Math;

namespace SpriteEngine.UI;

/// <summary>
/// 可持有子元素的 UI 容器。支持嵌套、布局引擎、命中测试和绘制传播。
/// </summary>
public class UIContainer : UIElement
{
    private readonly List<UIElement> _children = new();
    private ILayoutEngine? _layoutEngine;

    // ── Padding ──
    public Thickness Padding { get; set; } = new(0);

    public float ContentOffsetX => Padding.Left;
    public float ContentOffsetY => Padding.Top;
    public float ContentWidth => Math.Max(0, Width - Padding.Horizontal);
    public float ContentHeight => Math.Max(0, Height - Padding.Vertical);

    // ── 布局引擎 ──
    public ILayoutEngine? LayoutEngine
    {
        get => _layoutEngine;
        set
        {
            _layoutEngine = value;
            // 切换布局引擎后需要重新布局
        }
    }

    // ── 子元素访问 ──
    public IReadOnlyList<UIElement> Children => _children;

    public void AddChild(UIElement child)
    {
        if (child == null) return;
        if (child.Parent != null)
            child.Parent.RemoveChild(child);

        child.Parent = this;
        _children.Add(child);
    }

    public void RemoveChild(UIElement child)
    {
        if (child == null) return;
        if (_children.Remove(child))
            child.Parent = null;
    }

    public void RemoveAllChildren()
    {
        foreach (var c in _children)
            c.Parent = null;
        _children.Clear();
    }

    public UIElement? FindChildById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var c in _children)
        {
            if (c.GameObject?.Name == id) return c;
            if (c is UIContainer container)
            {
                var found = container.FindChildById(id);
                if (found != null) return found;
            }
        }
        return null;
    }

    // ── 生命周期重写 ──

    public override void Measure()
    {
        foreach (var child in _children)
        {
            if (child.Visible) child.Measure();
        }

        _layoutEngine?.Measure(this);

        // 如果布局引擎没有设置期望尺寸，使用显式尺寸或子元素包围盒
        if (DesiredWidth <= 0 && DesiredHeight <= 0)
        {
            float maxW = 0, maxH = 0;
            foreach (var child in _children)
            {
                if (!child.Visible) continue;
                maxW = Math.Max(maxW, child.LocalX + child.DesiredWidth);
                maxH = Math.Max(maxH, child.LocalY + child.DesiredHeight);
            }
            DesiredWidth = maxW + Padding.Horizontal;
            DesiredHeight = maxH + Padding.Vertical;
        }
    }

    public override void DoLayout()
    {
        _layoutEngine?.Layout(this);
        foreach (var child in _children)
        {
            if (child.Visible) child.DoLayout();
        }
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        // 绘制自身背景（如果有）
        BuildSelfDrawList(drawList);

        // 绘制子元素，按 ZOrder 排序
        var sorted = _children.Where(c => c.Visible).OrderBy(c => c.ZOrder).ToList();
        foreach (var child in sorted)
        {
            child.BuildDrawList(drawList);
        }
    }

    protected virtual void BuildSelfDrawList(UIDrawList drawList)
    {
        // 子类可重写以绘制容器背景
    }

    public override UIElement? HitTest(float x, float y)
    {
        if (!Visible || !Enabled) return null;
        if (!ContainsPoint(x, y)) return null;

        // 按 ZOrder 倒序查找最上层命中的子元素
        var sorted = _children.Where(c => c.Visible).OrderByDescending(c => c.ZOrder);
        foreach (var child in sorted)
        {
            var hit = child.HitTest(x, y);
            if (hit != null) return hit;
        }

        return this;
    }
}
