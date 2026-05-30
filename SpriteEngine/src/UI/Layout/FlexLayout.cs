using SpriteCore.Math;

namespace SpriteEngine.UI;

/// <summary>
/// Flexbox 布局引擎。默认布局方式。
/// 支持 Row/Column 方向、Wrap、JustifyContent、AlignItems。
/// </summary>
public class FlexLayout : ILayoutEngine
{
    public FlexDirection Direction { get; set; } = FlexDirection.Row;
    public FlexWrap Wrap { get; set; } = FlexWrap.NoWrap;
    public FlexJustify JustifyContent { get; set; } = FlexJustify.Start;
    public FlexAlign AlignItems { get; set; } = FlexAlign.Start;
    public float Gap { get; set; } = 0;

    public void Measure(UIContainer container)
    {
        float mainSize = 0;
        float crossSize = 0;
        bool isRow = Direction == FlexDirection.Row;

        var children = container.Children.Where(c => c.Visible).ToList();
        int count = children.Count;
        float totalGap = count > 0 ? (count - 1) * Gap : 0;

        foreach (var child in children)
        {
            child.Measure();
            float basis = child.Layout.FlexBasis ?? (isRow ? child.DesiredWidth : child.DesiredHeight);

            if (isRow)
            {
                mainSize += basis;
                crossSize = Math.Max(crossSize, child.DesiredHeight);
            }
            else
            {
                mainSize += basis;
                crossSize = Math.Max(crossSize, child.DesiredWidth);
            }
        }

        mainSize += totalGap;

        float paddingW = container.Padding.Left + container.Padding.Right;
        float paddingH = container.Padding.Top + container.Padding.Bottom;

        container.DesiredWidth = isRow ? mainSize + paddingW : crossSize + paddingW;
        container.DesiredHeight = isRow ? crossSize + paddingH : mainSize + paddingH;
    }

    public void Layout(UIContainer container)
    {
        bool isRow = Direction == FlexDirection.Row;
        var children = container.Children.Where(c => c.Visible).ToList();

        float contentMain = isRow ? container.ContentWidth : container.ContentHeight;
        float contentCross = isRow ? container.ContentHeight : container.ContentWidth;

        float totalBasis = 0;
        float totalGrow = 0;
        float totalShrink = 0;
        int count = children.Count;
        float totalGap = count > 0 ? (count - 1) * Gap : 0;

        foreach (var child in children)
        {
            float basis = child.Layout.FlexBasis ?? (isRow ? child.DesiredWidth : child.DesiredHeight);
            totalBasis += basis;
            totalGrow += child.Layout.FlexGrow;
            totalShrink += child.Layout.FlexShrink;
        }

        float extra = contentMain - totalBasis - totalGap;
        float scale = 1f;

        if (extra > 0 && totalGrow > 0)
        {
            // 分配剩余空间
        }
        else if (extra < 0 && totalShrink > 0)
        {
            // 收缩空间
            float shrinkTotal = 0;
            foreach (var child in children)
            {
                float basis = child.Layout.FlexBasis ?? (isRow ? child.DesiredWidth : child.DesiredHeight);
                shrinkTotal += basis * child.Layout.FlexShrink;
            }
            if (shrinkTotal > 0)
                scale = Math.Max(0, (contentMain - totalGap) / shrinkTotal);
        }

        // 计算每个子元素的主轴尺寸
        var sizes = new float[children.Count];
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            float basis = child.Layout.FlexBasis ?? (isRow ? child.DesiredWidth : child.DesiredHeight);

            if (extra > 0 && totalGrow > 0 && child.Layout.FlexGrow > 0)
            {
                sizes[i] = basis + extra * (child.Layout.FlexGrow / totalGrow);
            }
            else if (extra < 0 && totalShrink > 0)
            {
                sizes[i] = basis * child.Layout.FlexShrink * scale;
            }
            else
            {
                sizes[i] = basis;
            }
        }

        // 计算主轴偏移（JustifyContent）
        float usedMain = sizes.Sum() + totalGap;
        float offsetMain = isRow ? container.Padding.Left : container.Padding.Top;

        offsetMain += JustifyContent switch
        {
            FlexJustify.Center => (contentMain - usedMain) / 2,
            FlexJustify.End => contentMain - usedMain,
            FlexJustify.SpaceBetween when count > 1 => 0, // 间距由后面处理
            FlexJustify.SpaceAround when count > 0 => (contentMain - usedMain) / (count * 2),
            _ => 0,
        };

        float spaceBetween = 0;
        if (JustifyContent == FlexJustify.SpaceBetween && count > 1)
            spaceBetween = (contentMain - sizes.Sum()) / (count - 1);
        else if (JustifyContent == FlexJustify.SpaceAround && count > 0)
            spaceBetween = (contentMain - sizes.Sum()) / count;

        // 布局每个子元素
        float posMain = offsetMain;
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            float sizeMain = sizes[i];
            float sizeCross;

            // 交叉轴尺寸
            var align = child.Layout.AlignSelf ?? AlignItems;
            if (align == FlexAlign.Stretch)
            {
                sizeCross = contentCross;
            }
            else
            {
                sizeCross = isRow ? child.DesiredHeight : child.DesiredWidth;
            }

            // 交叉轴偏移
            float offsetCross = isRow ? container.Padding.Top : container.Padding.Left;
            float availableCross = contentCross;
            float crossPos = align switch
            {
                FlexAlign.Center => offsetCross + (availableCross - sizeCross) / 2,
                FlexAlign.End => offsetCross + availableCross - sizeCross,
                _ => offsetCross,
            };

            if (isRow)
            {
                child.LocalX = posMain;
                child.LocalY = crossPos;
                child.Width = sizeMain;
                child.Height = sizeCross;
            }
            else
            {
                child.LocalX = crossPos;
                child.LocalY = posMain;
                child.Width = sizeCross;
                child.Height = sizeMain;
            }

            posMain += sizeMain + (JustifyContent == FlexJustify.SpaceBetween || JustifyContent == FlexJustify.SpaceAround
                ? spaceBetween : Gap);
        }
    }
}
