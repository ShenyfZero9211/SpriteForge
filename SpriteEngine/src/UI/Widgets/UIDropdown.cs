using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 下拉选择框 Widget。点击展开选项列表，选择后收起。
/// </summary>
public class UIDropdown : UIElement
{
    private readonly List<string> _items = new();
    private int _selectedIndex = -1;
    private bool _isExpanded;
    private int _hoveredItemIndex = -1;
    private int _pressedItemIndex = -1;

    public UIDropdown()
    {
        Focusable = true;
        ZOrder = 100; // 高 ZOrder 确保展开时选项列表覆盖其他子元素
    }

    /// <summary>选项列表</summary>
    public IReadOnlyList<string> Items => _items;

    /// <summary>状态存储 Id。不为空时选中索引会持久化到 UIStateStorage。</summary>
    public string? StateId { get; set; }

    /// <summary>当前选中索引，-1 表示未选中</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                SaveSelectedIndex();
                OnSelectionChanged?.Invoke(_selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null);
            }
        }
    }

    /// <summary>当前选中的文本，null 表示未选中</summary>
    public string? SelectedText => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

    /// <summary>是否展开选项列表</summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnExpandedChanged?.Invoke(_isExpanded);
            }
        }
    }

    /// <summary>选中项变化时触发（参数为选中的文本，null 表示未选中）</summary>
    public event Action<string?>? OnSelectionChanged;

    /// <summary>展开/收起状态变化时触发</summary>
    public event Action<bool>? OnExpandedChanged;

    /// <summary>每个选项的高度</summary>
    public float ItemHeight { get; set; } = 28;

    /// <summary>选项列表的最大高度（超出则显示滚动条，当前版本不实现内部滚动）</summary>
    public float MaxDropdownHeight { get; set; } = 200;

    /// <summary>箭头符号颜色</summary>
    public SKColor? ArrowColor { get; set; }

    public bool IsHovered { get; private set; }
    public int HoveredItemIndex => _hoveredItemIndex;

    public void SetItems(IEnumerable<string> items)
    {
        _items.Clear();
        _items.AddRange(items);
        if (_selectedIndex >= _items.Count)
            _selectedIndex = -1;
    }

    public override void Measure()
    {
        var style = ResolvedStyle;
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        float padV = style.Padding.Vertical;

        // 头部高度（下拉框收起/展开时布局尺寸不变，选项列表以浮动层绘制）
        float headerH = Math.Max(ItemHeight, fontSize * 1.4f + padV);
        DesiredHeight = Height > 0 ? Height : headerH;
        DesiredWidth = Width > 0 ? Width : 120;
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;
        float radius = style.CornerRadius;
        var bgColor = IsHovered ? style.HoverColor : style.BackgroundColor;
        float headerH = GetHeaderHeight(style);

        // 头部背景
        drawList.AddRectFilled(AbsoluteX, AbsoluteY, Width, headerH, bgColor, radius);

        // 头部边框
        if (style.BorderThickness > 0)
        {
            drawList.AddRect(AbsoluteX, AbsoluteY, Width, headerH, style.BorderColor, style.BorderThickness, radius);
        }

        // 选中文本
        string displayText = SelectedText ?? "";
        if (!string.IsNullOrEmpty(displayText))
        {
            float fontSize = style.FontSize > 0 ? style.FontSize : 14;
            float tx = AbsoluteX + style.Padding.Left;
            float ty = AbsoluteY + headerH / 2 + fontSize * 0.35f;
            drawList.AddText(displayText, tx, ty, style.TextColor, fontSize);
        }

        // 下拉箭头
        float arrowSize = 6;
        float arrowX = AbsoluteX + Width - style.Padding.Right - arrowSize;
        float arrowY = AbsoluteY + headerH / 2;
        var arrowColor = ArrowColor ?? style.TextColor;

        if (_isExpanded)
        {
            drawList.AddLine(arrowX - arrowSize, arrowY + arrowSize / 2, arrowX, arrowY - arrowSize / 2, arrowColor, 1.5f);
            drawList.AddLine(arrowX, arrowY - arrowSize / 2, arrowX + arrowSize, arrowY + arrowSize / 2, arrowColor, 1.5f);
        }
        else
        {
            drawList.AddLine(arrowX - arrowSize, arrowY - arrowSize / 2, arrowX, arrowY + arrowSize / 2, arrowColor, 1.5f);
            drawList.AddLine(arrowX, arrowY + arrowSize / 2, arrowX + arrowSize, arrowY - arrowSize / 2, arrowColor, 1.5f);
        }

        // 选项列表（浮动层，绘制在自身 BuildDrawList 中，通过 ZOrder 控制覆盖顺序）
        if (_isExpanded && _items.Count > 0)
        {
            float listY = AbsoluteY + headerH;
            float listH = Math.Min(_items.Count * ItemHeight, MaxDropdownHeight);
            float fontSize = style.FontSize > 0 ? style.FontSize : 14;

            // 列表背景
            var listBg = new SKColor(
                (byte)Math.Min(style.BackgroundColor.Red + 15, 255),
                (byte)Math.Min(style.BackgroundColor.Green + 15, 255),
                (byte)Math.Min(style.BackgroundColor.Blue + 15, 255),
                style.BackgroundColor.Alpha);
            drawList.AddRectFilled(AbsoluteX, listY, Width, listH, listBg, 0);

            // 列表边框
            if (style.BorderThickness > 0)
            {
                drawList.AddRect(AbsoluteX, listY, Width, listH, style.BorderColor, style.BorderThickness, 0);
            }

            // 各个选项
            for (int i = 0; i < _items.Count; i++)
            {
                float itemY = listY + i * ItemHeight;
                bool isHovered = i == _hoveredItemIndex;
                bool isPressed = i == _pressedItemIndex;
                bool isSelected = i == _selectedIndex;

                // 背景：选中 > 按压 > 悬停
                if (isSelected)
                {
                    var selColor = new SKColor(60, 100, 160);
                    drawList.AddRectFilled(AbsoluteX, itemY, Width, ItemHeight, selColor, 0);
                }
                else if (isPressed)
                {
                    drawList.AddRectFilled(AbsoluteX, itemY, Width, ItemHeight, style.PressedColor, 0);
                }
                else if (isHovered)
                {
                    drawList.AddRectFilled(AbsoluteX, itemY, Width, ItemHeight, style.HoverColor, 0);
                }

                // 文本颜色：选中项用白色更醒目
                var textColor = isSelected ? new SKColor(255, 255, 255) : style.TextColor;
                float itemTextY = itemY + ItemHeight / 2 + fontSize * 0.35f;
                drawList.AddText(_items[i], AbsoluteX + style.Padding.Left, itemTextY, textColor, fontSize);
            }
        }
    }

    public override bool ContainsPoint(float x, float y)
    {
        if (!Visible) return false;
        float ax = AbsoluteX;
        float ay = AbsoluteY;
        float headerH = GetHeaderHeight(ResolvedStyle);

        // 头部区域
        if (x >= ax && y >= ay && x < ax + Width && y < ay + headerH)
            return true;

        // 展开时的选项列表区域
        if (_isExpanded && _items.Count > 0)
        {
            float listY = ay + headerH;
            float listH = Math.Min(_items.Count * ItemHeight, MaxDropdownHeight);
            if (x >= ax && y >= listY && x < ax + Width && y < listY + listH)
                return true;
        }

        return false;
    }

    public override bool OnEvent(UIEvent evt)
    {
        if (!Enabled) return false;

        float headerH = GetHeaderHeight(ResolvedStyle);

        switch (evt.Type)
        {
            case UIEventType.MouseMoved:
                bool nowHovered = ContainsPoint(evt.MouseX, evt.MouseY);
                if (nowHovered != IsHovered)
                    IsHovered = nowHovered;
                // 展开时检测选项悬停
                if (_isExpanded)
                {
                    int? newHover = HitTestItem(evt.MouseX, evt.MouseY, headerH);
                    _hoveredItemIndex = newHover ?? -1;
                }
                else
                {
                    _hoveredItemIndex = -1;
                }
                return nowHovered;

            case UIEventType.MousePressed:
                if (ContainsPoint(evt.MouseX, evt.MouseY))
                {
                    if (_isExpanded)
                    {
                        // 检查是否点击了某个选项
                        int? clickedIndex = HitTestItem(evt.MouseX, evt.MouseY, headerH);
                        if (clickedIndex.HasValue)
                        {
                            _pressedItemIndex = clickedIndex.Value;
                            SelectedIndex = clickedIndex.Value;
                            _pressedItemIndex = -1;
                            IsExpanded = false;
                            return true;
                        }
                        // 点击头部收起
                        if (evt.MouseY < AbsoluteY + headerH)
                        {
                            IsExpanded = false;
                            return true;
                        }
                    }
                    else
                    {
                        // 点击头部展开
                        IsExpanded = true;
                        return true;
                    }
                }
                else if (_isExpanded)
                {
                    // 点击外部收起
                    IsExpanded = false;
                }
                break;
        }

        return false;
    }

    private float GetHeaderHeight(UIStyle style)
    {
        float fontSize = style.FontSize > 0 ? style.FontSize : 14;
        return Math.Max(ItemHeight, fontSize * 1.4f + style.Padding.Vertical);
    }

    private void SaveSelectedIndex()
    {
        if (string.IsNullOrEmpty(StateId)) return;
        var storage = GetStateStorage();
        if (storage != null)
            storage.Set($"{StateId}_selected", _selectedIndex);
    }

    /// <summary>从状态存储恢复选中索引。通常在初始化后调用。</summary>
    public void LoadSelectedIndex()
    {
        if (string.IsNullOrEmpty(StateId)) return;
        var storage = GetStateStorage();
        if (storage != null && storage.TryGet($"{StateId}_selected", out int saved))
        {
            if (saved >= -1 && saved < _items.Count)
                _selectedIndex = saved;
        }
    }

    private int? HitTestItem(float mx, float my, float headerH)
    {
        if (!_isExpanded || _items.Count == 0) return null;
        float listY = AbsoluteY + headerH;
        if (my < listY || my >= listY + Math.Min(_items.Count * ItemHeight, MaxDropdownHeight))
            return null;
        int index = (int)((my - listY) / ItemHeight);
        if (index >= 0 && index < _items.Count)
            return index;
        return null;
    }

    // IsItemHovered 已替换为 _hoveredItemIndex 字段驱动
}
