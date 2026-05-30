using SpriteEngine.Scenes;

namespace SpriteEngine.UI;

/// <summary>
/// 所有 UI 组件的抽象基类。继承 Component，天然融入 GameObject/Scene 体系。
/// 支持世界空间 UI（血条、名字标签）和屏幕空间 UI（HUD、菜单）。
/// </summary>
public abstract class UIElement : Component
{
    // ── 布局几何（本地坐标系，相对父容器）──
    public float LocalX { get; set; }
    public float LocalY { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    // ── 测量结果（由 Measure 计算）──
    public float DesiredWidth { get; internal set; }
    public float DesiredHeight { get; internal set; }

    // ── 状态 ──
    public bool Visible { get; set; } = true;
    public new bool Enabled { get; set; } = true;
    public bool Focusable { get; set; } = false;
    public int ZOrder { get; set; } = 0;
    public float Alpha { get; set; } = 1.0f;

    // ── 样式覆盖（如果为 null，继承父级/主题）──
    public UIStyle? OverrideStyle { get; set; }

    // ── 布局属性 ──
    public LayoutProperties Layout { get; set; } = new();

    // ── 锚点（用于 AnchorLayout）──
    public AnchorFlags Anchor { get; set; }

    // ── 父容器引用 ──
    public UIContainer? Parent { get; internal set; }

    // ── 计算属性 ──
    public float AbsoluteX
    {
        get
        {
            if (Parent == null) return LocalX;
            return Parent.AbsoluteX + Parent.ContentOffsetX + LocalX;
        }
    }

    public float AbsoluteY
    {
        get
        {
            if (Parent == null) return LocalY;
            return Parent.AbsoluteY + Parent.ContentOffsetY + LocalY;
        }
    }

    public float EffectiveAlpha
    {
        get
        {
            if (Parent == null) return Alpha;
            return Alpha * Parent.EffectiveAlpha;
        }
    }

    /// <summary>解析最终样式：Override → Parent → Theme Default</summary>
    public UIStyle ResolvedStyle
    {
        get
        {
            if (OverrideStyle.HasValue) return OverrideStyle.Value;
            if (Parent != null) return Parent.ResolvedStyle;
            return UITheme.Default;
        }
    }

    // ── 生命周期（由 UICanvas/UIManager 驱动）──

    /// <summary>测量期望尺寸，设置 DesiredWidth/DesiredHeight</summary>
    public virtual void Measure()
    {
        DesiredWidth = Width;
        DesiredHeight = Height;
    }

    /// <summary>应用最终尺寸和位置（已由 LayoutEngine 设置）</summary>
    public virtual void DoLayout() { }

    /// <summary>将渲染原语写入 DrawList</summary>
    public virtual void BuildDrawList(UIDrawList drawList) { }

    /// <summary>处理 UI 事件。返回 true 表示事件已消费，停止冒泡。</summary>
    public virtual bool OnEvent(UIEvent evt) => false;

    // ── 命中测试 ──

    public virtual bool ContainsPoint(float x, float y)
    {
        if (!Visible) return false;
        float ax = AbsoluteX;
        float ay = AbsoluteY;
        return x >= ax && y >= ay && x < ax + Width && y < ay + Height;
    }

    public virtual UIElement? HitTest(float x, float y)
    {
        if (!Visible || !Enabled) return null;
        return ContainsPoint(x, y) ? this : null;
    }

    // ── 锚点辅助 ──
    public bool HasAnchor(AnchorFlags flag) => (Anchor & flag) != 0;
}
