using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 标签页 Widget。顶部标签栏切换不同的内容面板。
/// </summary>
public class UITabPane : UIContainer
{
    private readonly List<TabInfo> _tabs = new();
    private int _activeIndex = -1;

    private class TabInfo
    {
        public string Title = "";
        public UIContainer Content = new();
    }

    /// <summary>标签页标题列表</summary>
    public IReadOnlyList<string> TabTitles => _tabs.Select(t => t.Title).ToList();

    /// <summary>状态存储 Id。不为空时 active index 会持久化到 UIStateStorage。</summary>
    public string? StateId { get; set; }

    /// <summary>当前激活的标签索引</summary>
    public int ActiveIndex
    {
        get => _activeIndex;
        set
        {
            if (_activeIndex != value)
            {
                _activeIndex = value;
                UpdateContentVisibility();
                SaveActiveIndex();
                OnTabChanged?.Invoke(_activeIndex >= 0 && _activeIndex < _tabs.Count ? _tabs[_activeIndex].Title : null);
            }
        }
    }

    /// <summary>当前激活的标签标题</summary>
    public string? ActiveTitle => _activeIndex >= 0 && _activeIndex < _tabs.Count ? _tabs[_activeIndex].Title : null;

    /// <summary>标签栏高度</summary>
    public float TabHeight { get; set; } = 36;

    /// <summary>标签切换时触发（参数为新的标签标题）</summary>
    public event Action<string?>? OnTabChanged;

    /// <summary>激活标签的背景颜色</summary>
    public SKColor? ActiveTabColor { get; set; }

    /// <summary>非激活标签的悬停颜色</summary>
    public SKColor? InactiveHoverColor { get; set; }

    /// <summary>标签之间的间隔</summary>
    public float TabGap { get; set; } = 0;

    private int _hoveredTabIndex = -1;

    /// <summary>添加一个标签页</summary>
    public UIContainer AddTab(string title)
    {
        var tab = new TabInfo { Title = title, Content = new UIContainer() };
        _tabs.Add(tab);
        base.AddChild(tab.Content);
        tab.Content.Visible = _tabs.Count == 1; // 第一个默认可见
        if (_tabs.Count == 1)
            _activeIndex = 0;
        return tab.Content;
    }

    /// <summary>移除指定标签页</summary>
    public void RemoveTabAt(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;
        base.RemoveChild(_tabs[index].Content);
        _tabs.RemoveAt(index);
        if (_activeIndex >= _tabs.Count)
            _activeIndex = _tabs.Count - 1;
        if (_activeIndex < 0 && _tabs.Count > 0)
            _activeIndex = 0;
        UpdateContentVisibility();
    }

    /// <summary>清空所有标签页</summary>
    public void ClearTabs()
    {
        foreach (var tab in _tabs)
            base.RemoveChild(tab.Content);
        _tabs.Clear();
        _activeIndex = -1;
    }

    public override void Measure()
    {
        DesiredWidth = Width > 0 ? Width : 300;
        DesiredHeight = Height > 0 ? Height : TabHeight + 150;

        foreach (var tab in _tabs)
        {
            tab.Content.Measure();
        }
    }

    public override void DoLayout()
    {
        // 标签栏占顶部
        float contentY = TabHeight;
        float contentH = Math.Max(0, Height - contentY);

        foreach (var tab in _tabs)
        {
            tab.Content.LocalX = 0;
            tab.Content.LocalY = contentY;
            tab.Content.Width = Width;
            tab.Content.Height = contentH;
            tab.Content.DoLayout();
        }
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        var activeColor = ActiveTabColor ?? style.ForegroundColor;
        var inactiveColor = style.BackgroundColor;
        var hoverColor = InactiveHoverColor ?? style.HoverColor;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;

        // 计算每个标签的宽度
        float tabWidth = _tabs.Count > 0 ? Width / _tabs.Count : 0;

        // 绘制标签栏背景
        drawList.AddRectFilled(AbsoluteX, AbsoluteY, Width, TabHeight, inactiveColor, 0);

        // 绘制标签分隔线（底部）
        drawList.AddLine(AbsoluteX, AbsoluteY + TabHeight - 1, AbsoluteX + Width, AbsoluteY + TabHeight - 1, style.BorderColor, 1);

        // 绘制每个标签
        for (int i = 0; i < _tabs.Count; i++)
        {
            float tx = AbsoluteX + i * tabWidth;
            bool isActive = i == _activeIndex;
            bool isHovered = i == _hoveredTabIndex;

            // 标签背景
            if (isActive)
            {
                drawList.AddRectFilled(tx, AbsoluteY, tabWidth - TabGap, TabHeight, activeColor, 0);
            }
            else if (isHovered)
            {
                drawList.AddRectFilled(tx, AbsoluteY, tabWidth - TabGap, TabHeight, hoverColor, 0);
            }

            // 标签文本（居中）
            float textW = _tabs[i].Title.Length * fontSize * 0.6f;
            float textX = tx + (tabWidth - TabGap - textW) / 2;
            float textY = AbsoluteY + TabHeight / 2 + fontSize * 0.35f;
            drawList.AddText(_tabs[i].Title, textX, textY, style.TextColor, fontSize);
        }

        // 绘制当前激活的内容面板
        if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
        {
            _tabs[_activeIndex].Content.BuildDrawList(drawList);
        }
    }

    public override bool OnEvent(UIEvent evt)
    {
        if (!Enabled) return false;

        switch (evt.Type)
        {
            case UIEventType.MouseMoved:
                bool inHeader = evt.MouseY >= AbsoluteY && evt.MouseY < AbsoluteY + TabHeight
                                && evt.MouseX >= AbsoluteX && evt.MouseX < AbsoluteX + Width;
                _hoveredTabIndex = inHeader ? HitTestTab(evt.MouseX) : -1;
                return ContainsPoint(evt.MouseX, evt.MouseY);

            case UIEventType.MousePressed:
                if (evt.MouseY >= AbsoluteY && evt.MouseY < AbsoluteY + TabHeight
                    && evt.MouseX >= AbsoluteX && evt.MouseX < AbsoluteX + Width)
                {
                    int idx = HitTestTab(evt.MouseX);
                    if (idx >= 0)
                    {
                        ActiveIndex = idx;
                        return true;
                    }
                }
                break;
        }

        // 转发事件给当前内容面板
        if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
        {
            return _tabs[_activeIndex].Content.OnEvent(evt);
        }

        return false;
    }

    public override UIElement? HitTest(float x, float y)
    {
        if (!Visible || !Enabled) return null;
        if (!ContainsPoint(x, y)) return null;

        // 优先命中当前内容面板
        if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
        {
            var hit = _tabs[_activeIndex].Content.HitTest(x, y);
            if (hit != null) return hit;
        }

        return this;
    }

    private int HitTestTab(float mouseX)
    {
        if (_tabs.Count == 0) return -1;
        float relX = mouseX - AbsoluteX;
        float tabWidth = Width / _tabs.Count;
        int idx = (int)(relX / tabWidth);
        if (idx >= 0 && idx < _tabs.Count)
            return idx;
        return -1;
    }

    private void UpdateContentVisibility()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].Content.Visible = i == _activeIndex;
        }
    }

    private void SaveActiveIndex()
    {
        if (string.IsNullOrEmpty(StateId)) return;
        var storage = GetStateStorage();
        if (storage != null)
            storage.Set($"{StateId}_active", _activeIndex);
    }

    /// <summary>从状态存储恢复激活标签。通常在初始化后调用。</summary>
    public void LoadActiveIndex()
    {
        if (string.IsNullOrEmpty(StateId)) return;
        var storage = GetStateStorage();
        if (storage != null && storage.TryGet($"{StateId}_active", out int saved))
        {
            if (saved >= 0 && saved < _tabs.Count)
            {
                _activeIndex = saved;
                UpdateContentVisibility();
            }
        }
    }
}
