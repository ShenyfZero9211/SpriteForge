namespace SpriteEngine.UI;

/// <summary>
/// 锚点布局引擎。子元素通过 UIElement.Anchor 属性控制自适应位置。
/// 受 p5engine Anchor 启发，用于 HUD、全屏面板等需要随分辨率自适应的 UI。
/// </summary>
public class AnchorLayout : ILayoutEngine
{
    public void Measure(UIContainer container)
    {
        foreach (var child in container.Children)
        {
            if (child.Visible) child.Measure();
        }
        // 容器的期望尺寸由使用者显式设置，或继承父级
    }

    public void Layout(UIContainer container)
    {
        float cw = container.ContentWidth;
        float ch = container.ContentHeight;
        float cx = container.Padding.Left;
        float cy = container.Padding.Top;

        foreach (var child in container.Children)
        {
            if (!child.Visible) continue;

            var anchor = child.Anchor;
            if (anchor == 0) continue;

            float w = child.Width;
            float h = child.Height;
            float x = child.LocalX;
            float y = child.LocalY;

            if (child.HasAnchor(AnchorFlags.Left))
            {
                x = cx;
                if (child.HasAnchor(AnchorFlags.Right))
                    w = cw;
            }
            else if (child.HasAnchor(AnchorFlags.Right))
            {
                x = cx + cw - w;
            }
            else if (child.HasAnchor(AnchorFlags.HCenter))
            {
                x = cx + (cw - w) / 2;
            }

            if (child.HasAnchor(AnchorFlags.Top))
            {
                y = cy;
                if (child.HasAnchor(AnchorFlags.Bottom))
                    h = ch;
            }
            else if (child.HasAnchor(AnchorFlags.Bottom))
            {
                y = cy + ch - h;
            }
            else if (child.HasAnchor(AnchorFlags.VCenter))
            {
                y = cy + (ch - h) / 2;
            }

            child.LocalX = x;
            child.LocalY = y;
            child.Width = w;
            child.Height = h;
        }
    }
}
