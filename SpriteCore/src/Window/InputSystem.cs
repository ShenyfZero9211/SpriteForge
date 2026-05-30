using SDL2;

namespace SpriteCore.Window;

public class InputSystem : IDisposable
{
    private readonly HashSet<SDL.SDL_Keycode> _keysPressed = new();
    private readonly HashSet<SDL.SDL_Keycode> _keysDown = new();
    private readonly HashSet<SDL.SDL_Keycode> _keysUp = new();
    private readonly bool[] _mouseButtons = new bool[5];
    private readonly bool[] _mouseButtonsDown = new bool[5];
    private readonly bool[] _mouseButtonsUp = new bool[5];
    private readonly List<char> _textInputQueue = new();

    public float MouseX { get; private set; }
    public float MouseY { get; private set; }
    public float MouseDeltaX { get; private set; }
    public float MouseDeltaY { get; private set; }
    public float MouseWheel { get; private set; }
    public bool MouseIsPressed => _mouseButtons[0];

    public void HandleEvent(SDL.SDL_Event e)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_KEYDOWN:
                if (!_keysPressed.Contains(e.key.keysym.sym))
                {
                    _keysDown.Add(e.key.keysym.sym);
                }
                _keysPressed.Add(e.key.keysym.sym);
                break;

            case SDL.SDL_EventType.SDL_KEYUP:
                _keysPressed.Remove(e.key.keysym.sym);
                _keysUp.Add(e.key.keysym.sym);
                break;

            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                MouseDeltaX = e.motion.xrel;
                MouseDeltaY = e.motion.yrel;
                MouseX = e.motion.x;
                MouseY = e.motion.y;
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                int btnDown = e.button.button - 1;
                if (btnDown >= 0 && btnDown < _mouseButtons.Length)
                {
                    _mouseButtonsDown[btnDown] = true;
                    _mouseButtons[btnDown] = true;
                }
                MouseX = e.button.x;
                MouseY = e.button.y;
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                int btnUp = e.button.button - 1;
                if (btnUp >= 0 && btnUp < _mouseButtons.Length)
                {
                    _mouseButtonsUp[btnUp] = true;
                    _mouseButtons[btnUp] = false;
                }
                MouseX = e.button.x;
                MouseY = e.button.y;
                break;

            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                MouseWheel = e.wheel.y;
                break;

            case SDL.SDL_EventType.SDL_TEXTINPUT:
                unsafe
                {
                    byte* text = e.text.text;
                    for (int i = 0; i < 32 && text[i] != 0; i++)
                    {
                        _textInputQueue.Add((char)text[i]);
                    }
                }
                break;
        }
    }

    public void PostUpdate()
    {
        _keysDown.Clear();
        _keysUp.Clear();
        _textInputQueue.Clear();
        Array.Clear(_mouseButtonsDown, 0, _mouseButtonsDown.Length);
        Array.Clear(_mouseButtonsUp, 0, _mouseButtonsUp.Length);
        MouseDeltaX = 0;
        MouseDeltaY = 0;
        MouseWheel = 0;
    }

    public bool IsKeyPressed(SDL.SDL_Keycode key) => _keysPressed.Contains(key);
    public bool IsKeyDown(SDL.SDL_Keycode key) => _keysDown.Contains(key);
    public bool IsKeyUp(SDL.SDL_Keycode key) => _keysUp.Contains(key);

    /// <summary>本帧刚按下的键码列表（只读快照）</summary>
    public IReadOnlyCollection<SDL.SDL_Keycode> KeysDown => _keysDown;

    /// <summary>获取本帧通过 SDL_TEXTINPUT 接收到的字符列表</summary>
    public IReadOnlyList<char> TextInputQueue => _textInputQueue;
    public bool IsMousePressed(int button) => button < _mouseButtons.Length && _mouseButtons[button];
    public bool IsMouseDown(int button) => button < _mouseButtonsDown.Length && _mouseButtonsDown[button];
    public bool IsMouseUp(int button) => button < _mouseButtonsUp.Length && _mouseButtonsUp[button];

    public void Dispose() { }
}
