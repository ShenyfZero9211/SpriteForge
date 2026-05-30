namespace SpriteEngine.UI;

/// <summary>
/// UI 统一事件结构体。不可变，由工厂方法创建。
/// </summary>
public readonly struct UIEvent
{
    public UIEventType Type { get; }
    public float MouseX { get; }
    public float MouseY { get; }
    public int MouseButton { get; }
    public char KeyChar { get; }
    public int KeyCode { get; }
    public float ScrollDelta { get; }

    public UIEvent(UIEventType type, float mouseX, float mouseY, int mouseButton,
                   char keyChar, int keyCode, float scrollDelta)
    {
        Type = type;
        MouseX = mouseX;
        MouseY = mouseY;
        MouseButton = mouseButton;
        KeyChar = keyChar;
        KeyCode = keyCode;
        ScrollDelta = scrollDelta;
    }

    // ── 工厂方法 ──

    public static UIEvent MousePressed(float x, float y, int button)
        => new(UIEventType.MousePressed, x, y, button, '\0', 0, 0);

    public static UIEvent MouseReleased(float x, float y, int button)
        => new(UIEventType.MouseReleased, x, y, button, '\0', 0, 0);

    public static UIEvent MouseMoved(float x, float y)
        => new(UIEventType.MouseMoved, x, y, 0, '\0', 0, 0);

    public static UIEvent MouseDragged(float x, float y, int button)
        => new(UIEventType.MouseDragged, x, y, button, '\0', 0, 0);

    public static UIEvent MouseWheel(float x, float y, float delta)
        => new(UIEventType.MouseWheel, x, y, 0, '\0', 0, delta);

    public static UIEvent MouseEntered()
        => new(UIEventType.MouseEntered, 0, 0, 0, '\0', 0, 0);

    public static UIEvent MouseExited()
        => new(UIEventType.MouseExited, 0, 0, 0, '\0', 0, 0);

    public static UIEvent KeyPressed(char keyChar, int keyCode)
        => new(UIEventType.KeyPressed, 0, 0, 0, keyChar, keyCode, 0);

    public static UIEvent KeyReleased(char keyChar, int keyCode)
        => new(UIEventType.KeyReleased, 0, 0, 0, keyChar, keyCode, 0);

    public static UIEvent KeyTyped(char keyChar)
        => new(UIEventType.KeyTyped, 0, 0, 0, keyChar, 0, 0);

    public static UIEvent FocusGained()
        => new(UIEventType.FocusGained, 0, 0, 0, '\0', 0, 0);

    public static UIEvent FocusLost()
        => new(UIEventType.FocusLost, 0, 0, 0, '\0', 0, 0);
}
