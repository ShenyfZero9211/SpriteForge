using SpriteCore.Graphics;
using SpriteCore.Window;

namespace SpriteEngine.UI;

/// <summary>
/// 场景级 UI 协调器。管理所有 UICanvas，处理输入分发和渲染排序。
/// </summary>
public class UIManager
{
    private readonly List<UICanvas> _canvases = new();
    private readonly UIFocusManager _focusManager = new();
    private UIElement? _pressedTarget;
    private UIElement? _hoverTarget;

    // 上一帧输入状态（用于生成 UIEvent）
    private bool _lastMousePressed;
    private float _lastMouseX;
    private float _lastMouseY;

    /// <summary>注册画布</summary>
    public void RegisterCanvas(UICanvas canvas)
    {
        if (!_canvases.Contains(canvas))
        {
            _canvases.Add(canvas);
            SortCanvases();
        }
    }

    /// <summary>注销画布</summary>
    public void UnregisterCanvas(UICanvas canvas)
    {
        _canvases.Remove(canvas);
    }

    private void SortCanvases()
    {
        _canvases.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
    }

    // ── Update ──

    public void Update(float dt)
    {
        foreach (var canvas in _canvases)
        {
            if (!canvas.Enabled || !canvas.Visible) continue;
            canvas.Measure();
            canvas.DoLayout();
        }
    }

    // ── Render ──

    public void Render(SPGraphics g)
    {
        foreach (var canvas in _canvases)
        {
            if (!canvas.Enabled || !canvas.Visible) continue;
            if (canvas.Space != CanvasSpace.Screen) continue;

            canvas.Render(g);
        }
    }

    /// <summary>渲染 World 空间的 UI（在 Scene 渲染游戏对象后调用）</summary>
    public void RenderWorldSpace(SPGraphics g)
    {
        foreach (var canvas in _canvases)
        {
            if (!canvas.Enabled || !canvas.Visible) continue;
            if (canvas.Space != CanvasSpace.World) continue;

            // World 空间：应用 GameObject 的 Transform 矩阵
            if (canvas.Transform != null)
            {
                g.PushMatrix();
                g.Translate(canvas.Transform.Position.X, canvas.Transform.Position.Y);
                g.Rotate(canvas.Transform.Rotation);
                g.Scale(canvas.Transform.Scale.X, canvas.Transform.Scale.Y);
                canvas.Render(g);
                g.PopMatrix();
            }
            else
            {
                canvas.Render(g);
            }
        }
    }

    // ── 输入处理 ──

    public void ProcessInput(InputSystem input)
    {
        float mx = input.MouseX;
        float my = input.MouseY;
        bool mousePressed = input.MouseIsPressed;

        // 生成鼠标移动事件
        if (mx != _lastMouseX || my != _lastMouseY)
        {
            DispatchToCanvases(UIEvent.MouseMoved(mx, my), mx, my);
        }

        // 生成鼠标按下事件
        if (mousePressed && !_lastMousePressed)
        {
            var consumed = DispatchToCanvases(UIEvent.MousePressed(mx, my, 0), mx, my);
            if (consumed) _pressedTarget = FindHitTarget(mx, my);
        }

        // 生成鼠标释放事件
        if (!mousePressed && _lastMousePressed)
        {
            var hit = FindHitTarget(mx, my);
            // 如果鼠标在按下目标之外释放，确保按下目标也收到释放事件
            if (_pressedTarget != null && hit != _pressedTarget)
            {
                _pressedTarget.OnEvent(UIEvent.MouseReleased(mx, my, 0));
            }
            DispatchToCanvases(UIEvent.MouseReleased(mx, my, 0), mx, my);
            _pressedTarget = null;
        }

        // 生成鼠标拖拽事件
        if (mousePressed && _lastMousePressed && (mx != _lastMouseX || my != _lastMouseY))
        {
            if (_pressedTarget != null)
            {
                _pressedTarget.OnEvent(UIEvent.MouseDragged(mx, my, 0));
            }
        }

        _lastMouseX = mx;
        _lastMouseY = my;
        _lastMousePressed = mousePressed;
    }

    /// <summary>
    /// 处理键盘事件。由外部调用（Scene 或 Sketch）。
    /// </summary>
    public void ProcessKeyEvent(UIEvent evt)
    {
        // Tab 导航
        if (evt.Type == UIEventType.KeyPressed && evt.KeyCode == (int)SDL2.SDL.SDL_Keycode.SDLK_TAB)
        {
            // 找到当前激活的 Screen Canvas 的根容器
            var activeCanvas = _canvases.LastOrDefault(c => c.Space == CanvasSpace.Screen && c.Enabled && c.Visible);
            if (activeCanvas != null)
            {
                if (SDL2.SDL.SDL_GetModState() == SDL2.SDL.SDL_Keymod.KMOD_SHIFT)
                    _focusManager.FocusPrevious(activeCanvas);
                else
                    _focusManager.FocusNext(activeCanvas);
            }
            return;
        }

        // 方向键导航
        if (evt.Type == UIEventType.KeyPressed)
        {
            var dir = evt.KeyCode switch
            {
                (int)SDL2.SDL.SDL_Keycode.SDLK_UP => UINavigationDirection.Up,
                (int)SDL2.SDL.SDL_Keycode.SDLK_DOWN => UINavigationDirection.Down,
                (int)SDL2.SDL.SDL_Keycode.SDLK_LEFT => UINavigationDirection.Left,
                (int)SDL2.SDL.SDL_Keycode.SDLK_RIGHT => UINavigationDirection.Right,
                _ => (UINavigationDirection?)null,
            };

            if (dir.HasValue)
            {
                var activeCanvas = _canvases.LastOrDefault(c => c.Space == CanvasSpace.Screen && c.Enabled && c.Visible);
                if (activeCanvas != null)
                    _focusManager.FocusDirection(activeCanvas, dir.Value);
                return;
            }
        }

        // 转发给焦点组件
        var focused = _focusManager.Focused;
        if (focused != null)
        {
            focused.OnEvent(evt);
        }
    }

    // ── 私有辅助 ──

    /// <summary>按 SortOrder 从高到低分发事件。返回 true 表示被消费。</summary>
    private bool DispatchToCanvases(UIEvent evt, float mx, float my)
    {
        // 倒序遍历：高 SortOrder 优先（弹窗 > HUD）
        for (int i = _canvases.Count - 1; i >= 0; i--)
        {
            var canvas = _canvases[i];
            if (!canvas.Enabled || !canvas.Visible) continue;
            if (canvas.Space != CanvasSpace.Screen) continue;

            var hit = canvas.HitTest(mx, my);
            if (hit != null)
            {
                // 冒泡分发
                var consumed = DispatchBubble(hit, evt);
                if (consumed)
                {
                    // 更新 hover
                    if (evt.Type == UIEventType.MouseMoved)
                    {
                        if (_hoverTarget != hit)
                        {
                            _hoverTarget?.OnEvent(UIEvent.MouseExited());
                            hit.OnEvent(UIEvent.MouseEntered());
                            _hoverTarget = hit;
                        }
                    }

                    if (canvas.ConsumeInput)
                        return true;
                }
            }
        }

        // 鼠标移出所有 UI
        if (evt.Type == UIEventType.MouseMoved && _hoverTarget != null)
        {
            _hoverTarget.OnEvent(UIEvent.MouseExited());
            _hoverTarget = null;
        }

        return false;
    }

    private static bool DispatchBubble(UIElement target, UIEvent evt)
    {
        UIElement? current = target;
        while (current != null)
        {
            if (current.OnEvent(evt))
                return true;
            current = current.Parent;
        }
        return false;
    }

    private UIElement? FindHitTarget(float x, float y)
    {
        for (int i = _canvases.Count - 1; i >= 0; i--)
        {
            var canvas = _canvases[i];
            if (!canvas.Enabled || !canvas.Visible) continue;
            if (canvas.Space != CanvasSpace.Screen) continue;
            var hit = canvas.HitTest(x, y);
            if (hit != null) return hit;
        }
        return null;
    }
}
