using SkiaSharp;

namespace SpriteEngine.UI;

/// <summary>
/// 滚动视图 Widget。内容超出可视区域时可滚动查看。
/// 通过 <see cref="Content"/> 容器添加子元素。
/// </summary>
public class UIScrollView : UIContainer
{
    private float _scrollX;
    private float _scrollY;

    /// <summary>内容容器。所有可滚动内容应添加到此容器。</summary>
    public UIContainer Content { get; }

    /// <summary>状态存储 Id。不为空时 scroll offset 会持久化到 UIStateStorage。</summary>
    public string? StateId { get; set; }

    /// <summary>水平滚动偏移</summary>
    public float ScrollX
    {
        get => _scrollX;
        set
        {
            _scrollX = ClampScroll(value, MaxScrollX);
            SaveScrollState();
        }
    }

    /// <summary>垂直滚动偏移</summary>
    public float ScrollY
    {
        get => _scrollY;
        set
        {
            _scrollY = ClampScroll(value, MaxScrollY);
            SaveScrollState();
        }
    }

    /// <summary>内容宽度（由 Content 的 DesiredWidth 决定）</summary>
    public float ContentWidth => Content.DesiredWidth > 0 ? Content.DesiredWidth : Content.Width;

    /// <summary>内容高度（由 Content 的 DesiredHeight 决定）</summary>
    public float ContentHeight => Content.DesiredHeight > 0 ? Content.DesiredHeight : Content.Height;

    /// <summary>可视区域宽度</summary>
    public float ViewportWidth => Math.Max(0, Width - Padding.Horizontal);

    /// <summary>可视区域高度</summary>
    public float ViewportHeight => Math.Max(0, Height - Padding.Vertical);

    /// <summary>最大水平滚动值</summary>
    public float MaxScrollX => Math.Max(0, ContentWidth - ViewportWidth);

    /// <summary>最大垂直滚动值</summary>
    public float MaxScrollY => Math.Max(0, ContentHeight - ViewportHeight);

    /// <summary>滚动条宽度</summary>
    public float ScrollbarWidth { get; set; } = 6;

    /// <summary>滚动条颜色</summary>
    public SKColor ScrollbarColor { get; set; } = new SKColor(120, 120, 130);

    /// <summary>滚动条轨道颜色</summary>
    public SKColor ScrollbarTrackColor { get; set; } = new SKColor(50, 50, 55);

    /// <summary>是否显示垂直滚动条</summary>
    public bool ShowVerticalScrollbar { get; set; } = true;

    /// <summary>是否显示水平滚动条</summary>
    public bool ShowHorizontalScrollbar { get; set; } = true;

    /// <summary>是否响应鼠标滚轮</summary>
    public bool HandleMouseWheel { get; set; } = true;

    /// <summary>鼠标滚轮灵敏度</summary>
    public float WheelSensitivity { get; set; } = 30;

    public UIScrollView()
    {
        Content = new UIContainer
        {
            Width = 0,
            Height = 0,
            Padding = new Thickness(0)
        };
        base.AddChild(Content);
    }

    public override void Measure()
    {
        // 先测量 Content（不限制尺寸，让它自然展开）
        Content.Measure();

        // 自身尺寸：使用显式尺寸或默认值
        DesiredWidth = Width > 0 ? Width : 200;
        DesiredHeight = Height > 0 ? Height : 150;
    }

    public override void DoLayout()
    {
        // Content 的位置随滚动偏移移动
        Content.LocalX = Padding.Left - ScrollX;
        Content.LocalY = Padding.Top - ScrollY;
        Content.Width = Math.Max(ContentWidth, ViewportWidth);
        Content.Height = Math.Max(ContentHeight, ViewportHeight);

        // 布局 Content 的子元素
        Content.DoLayout();
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        var style = ResolvedStyle;

        // 绘制背景
        if (style.BackgroundColor.Alpha > 0)
        {
            drawList.AddRectFilled(AbsoluteX, AbsoluteY, Width, Height, style.BackgroundColor, style.CornerRadius);
        }

        // 绘制边框
        if (style.BorderThickness > 0)
        {
            drawList.AddRect(AbsoluteX, AbsoluteY, Width, Height, style.BorderColor, style.BorderThickness, style.CornerRadius);
        }

        // 裁剪到可视区域
        float clipX = AbsoluteX + Padding.Left;
        float clipY = AbsoluteY + Padding.Top;
        float clipW = ViewportWidth;
        float clipH = ViewportHeight;
        drawList.PushClipRect(clipX, clipY, clipW, clipH);

        // 绘制 Content 及其子元素
        Content.BuildDrawList(drawList);

        drawList.PopClipRect();

        // 绘制滚动条
        DrawScrollbars(drawList);
    }

    private void DrawScrollbars(UIDrawList drawList)
    {
        float trackX = AbsoluteX + Width - ScrollbarWidth - 2;
        float trackY = AbsoluteY + Padding.Top + 2;
        float trackH = ViewportHeight - 4;

        // 垂直滚动条
        if (ShowVerticalScrollbar && MaxScrollY > 0)
        {
            // 轨道
            drawList.AddRectFilled(trackX, trackY, ScrollbarWidth, trackH, ScrollbarTrackColor, ScrollbarWidth / 2);

            // 滑块
            float thumbRatio = ViewportHeight / ContentHeight;
            float thumbH = Math.Max(ScrollbarWidth * 2, trackH * thumbRatio);
            float thumbY = trackY + (ScrollY / MaxScrollY) * (trackH - thumbH);
            drawList.AddRectFilled(trackX, thumbY, ScrollbarWidth, thumbH, ScrollbarColor, ScrollbarWidth / 2);
        }

        // 水平滚动条
        if (ShowHorizontalScrollbar && MaxScrollX > 0)
        {
            float hTrackX = AbsoluteX + Padding.Left + 2;
            float hTrackY = AbsoluteY + Height - ScrollbarWidth - 2;
            float hTrackW = ViewportWidth - 4;

            // 轨道
            drawList.AddRectFilled(hTrackX, hTrackY, hTrackW, ScrollbarWidth, ScrollbarTrackColor, ScrollbarWidth / 2);

            // 滑块
            float thumbRatio = ViewportWidth / ContentWidth;
            float thumbW = Math.Max(ScrollbarWidth * 2, hTrackW * thumbRatio);
            float thumbX = hTrackX + (ScrollX / MaxScrollX) * (hTrackW - thumbW);
            drawList.AddRectFilled(thumbX, hTrackY, thumbW, ScrollbarWidth, ScrollbarColor, ScrollbarWidth / 2);
        }
    }

    public override bool OnEvent(UIEvent evt)
    {
        if (!Enabled) return false;

        switch (evt.Type)
        {
            case UIEventType.MouseMoved:
                bool nowHovered = ContainsPoint(evt.MouseX, evt.MouseY);
                return nowHovered;

            case UIEventType.MouseWheel:
                if (HandleMouseWheel && ContainsPoint(evt.MouseX, evt.MouseY))
                {
                    ScrollY += evt.ScrollDelta * WheelSensitivity;
                    return true;
                }
                break;
        }

        // 将鼠标事件坐标转换为 Content 本地坐标后转发
        if (evt.Type is UIEventType.MousePressed or UIEventType.MouseReleased
            or UIEventType.MouseDragged)
        {
            var localEvt = TransformEventToContent(evt);
            if (Content.ContainsPoint(localEvt.MouseX, localEvt.MouseY))
            {
                return Content.OnEvent(localEvt);
            }
        }

        return false;
    }

    public override UIElement? HitTest(float x, float y)
    {
        if (!Visible || !Enabled) return null;
        if (!ContainsPoint(x, y)) return null;

        // 检查 Content 中的子元素（考虑滚动偏移）
        float localX = x - AbsoluteX + ScrollX - Padding.Left;
        float localY = y - AbsoluteY + ScrollY - Padding.Top;
        var hit = Content.HitTest(localX, localY);
        if (hit != null && hit != Content) return hit;

        return this;
    }

    private UIEvent TransformEventToContent(UIEvent evt)
    {
        return evt.Type switch
        {
            UIEventType.MousePressed => UIEvent.MousePressed(
                evt.MouseX - AbsoluteX + ScrollX - Padding.Left,
                evt.MouseY - AbsoluteY + ScrollY - Padding.Top, evt.MouseButton),
            UIEventType.MouseReleased => UIEvent.MouseReleased(
                evt.MouseX - AbsoluteX + ScrollX - Padding.Left,
                evt.MouseY - AbsoluteY + ScrollY - Padding.Top, evt.MouseButton),
            UIEventType.MouseDragged => UIEvent.MouseDragged(
                evt.MouseX - AbsoluteX + ScrollX - Padding.Left,
                evt.MouseY - AbsoluteY + ScrollY - Padding.Top, evt.MouseButton),
            UIEventType.MouseMoved => UIEvent.MouseMoved(
                evt.MouseX - AbsoluteX + ScrollX - Padding.Left,
                evt.MouseY - AbsoluteY + ScrollY - Padding.Top),
            _ => evt
        };
    }

    private static float ClampScroll(float value, float max)
    {
        if (max <= 0) return 0;
        return Math.Clamp(value, 0, max);
    }

    private void SaveScrollState()
    {
        if (string.IsNullOrEmpty(StateId)) return;
        var storage = GetStateStorage();
        if (storage != null)
        {
            storage.Set($"{StateId}_scroll", (_scrollX, _scrollY));
        }
    }

    /// <summary>从状态存储恢复滚动偏移。通常在初始化后调用。</summary>
    public void LoadScrollState()
    {
        if (string.IsNullOrEmpty(StateId)) return;
        var storage = GetStateStorage();
        if (storage != null && storage.TryGet($"{StateId}_scroll", out (float x, float y) scroll))
        {
            _scrollX = ClampScroll(scroll.x, MaxScrollX);
            _scrollY = ClampScroll(scroll.y, MaxScrollY);
        }
    }

    // 禁止外部直接添加子元素到 ScrollView，应通过 Content.AddChild
    public new void AddChild(UIElement child) => Content.AddChild(child);
    public new void RemoveChild(UIElement child) => Content.RemoveChild(child);
}
