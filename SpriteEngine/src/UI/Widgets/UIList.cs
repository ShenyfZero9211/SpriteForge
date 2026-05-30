using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 列表 Widget。展示一系列可选条目，支持单选。
/// 超出可视区域时不自动滚动（需放入 UIScrollView 中实现滚动列表）。
/// </summary>
public class UIList : UIElement
{
    private readonly List<string> _items = new();
    private int _selectedIndex = -1;

    /// <summary>条目列表</summary>
    public IReadOnlyList<string> Items => _items;

    /// <summary>当前选中索引，-1 表示未选中</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnSelectionChanged?.Invoke(SelectedItem);
            }
        }
    }

    /// <summary>当前选中的条目，null 表示未选中</summary>
    public string? SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

    /// <summary>每个条目的高度</summary>
    public float ItemHeight { get; set; } = 32;

    /// <summary>选中项的背景颜色</summary>
    public SKColor? SelectedColor { get; set; }

    /// <summary>悬停项的背景颜色</summary>
    public SKColor? HoverColor { get; set; }

    /// <summary>选中项变化时触发</summary>
    public event Action<string?>? OnSelectionChanged;

    public bool IsHovered { get; private set; }
    public int HoveredIndex { get; private set; } = -1;

    public void SetItems(IEnumerable<string> items)
    {
        _items.Clear();
        _items.AddRange(items);
        if (_selectedIndex >= _items.Count)
            _selectedIndex = -1;
    }

    public void AddItem(string item)
    {
        _items.Add(item);
    }

    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= _items.Count) return;
        _items.RemoveAt(index);
        if (_selectedIndex == index)
            _selectedIndex = -1;
        else if (_selectedIndex > index)
            _selectedIndex--;
    }

    public void ClearItems()
    {
        _items.Clear();
        _selectedIndex = -1;
    }

    public override void Measure()
    {
        DesiredWidth = Width > 0 ? Width : 160;
        DesiredHeight = Height > 0 ? Height : _items.Count * ItemHeight;
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        var selColor = SelectedColor ?? new SKColor(60, 100, 160);
        var hovColor = HoverColor ?? new SKColor(50, 50, 60);

        for (int i = 0; i < _items.Count; i++)
        {
            float itemY = AbsoluteY + i * ItemHeight;
            if (itemY >= AbsoluteY + Height) break; // 超出底部不绘制
            if (itemY + ItemHeight <= AbsoluteY) continue; // 超出顶部不绘制

            // 背景
            if (i == _selectedIndex)
            {
                drawList.AddRectFilled(AbsoluteX, itemY, Width, ItemHeight, selColor, 0);
            }
            else if (i == HoveredIndex)
            {
                drawList.AddRectFilled(AbsoluteX, itemY, Width, ItemHeight, hovColor, 0);
            }

            // 文本
            float textY = itemY + ItemHeight / 2 + fontSize * 0.35f;
            drawList.AddText(_items[i], AbsoluteX + style.Padding.Left, textY, style.TextColor, fontSize);
        }
    }

    public override bool OnEvent(UIEvent evt)
    {
        if (!Enabled) return false;

        switch (evt.Type)
        {
            case UIEventType.MouseMoved:
                bool nowHovered = ContainsPoint(evt.MouseX, evt.MouseY);
                if (nowHovered != IsHovered)
                    IsHovered = nowHovered;
                HoveredIndex = nowHovered ? HitTestItem(evt.MouseY) : -1;
                return nowHovered;

            case UIEventType.MousePressed:
                if (ContainsPoint(evt.MouseX, evt.MouseY))
                {
                    int idx = HitTestItem(evt.MouseY);
                    if (idx >= 0)
                    {
                        SelectedIndex = idx;
                        return true;
                    }
                }
                break;
        }

        return false;
    }

    private int HitTestItem(float mouseY)
    {
        float relY = mouseY - AbsoluteY;
        int idx = (int)(relY / ItemHeight);
        if (idx >= 0 && idx < _items.Count)
            return idx;
        return -1;
    }
}
