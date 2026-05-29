using SDL2;
using SpriteCore.Utils;

namespace SpriteCore.Window;

public class GameWindow : IDisposable
{
    private IntPtr _window = IntPtr.Zero;
    private IntPtr _renderer = IntPtr.Zero;
    private IntPtr _texture = IntPtr.Zero;
    private bool _running = false;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public string Title { get; private set; } = "SpriteCore";
    public IntPtr SdlRenderer => _renderer;
    public IntPtr SdlTexture => _texture;
    public bool IsRunning => _running;

    public event Action<SDL.SDL_Event>? OnEvent;
    public event Action? OnUpdate;
    public event Action? OnRender;

    public void Create(string title, int width, int height)
    {
        Title = title;
        Width = width;
        Height = height;

        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_TIMER) < 0)
        {
            Log.Error("Window", $"SDL_Init failed: {SDL.SDL_GetError()}");
            throw new Exception($"SDL_Init failed: {SDL.SDL_GetError()}");
        }

        _window = SDL.SDL_CreateWindow(
            title,
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            width, height,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
            SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        if (_window == IntPtr.Zero)
        {
            Log.Error("Window", $"SDL_CreateWindow failed: {SDL.SDL_GetError()}");
            throw new Exception($"SDL_CreateWindow failed: {SDL.SDL_GetError()}");
        }

        _renderer = SDL.SDL_CreateRenderer(_window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (_renderer == IntPtr.Zero)
        {
            _renderer = SDL.SDL_CreateRenderer(_window, -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE);
        }

        if (_renderer == IntPtr.Zero)
        {
            Log.Error("Window", $"SDL_CreateRenderer failed: {SDL.SDL_GetError()}");
            throw new Exception($"SDL_CreateRenderer failed: {SDL.SDL_GetError()}");
        }

        _texture = SDL.SDL_CreateTexture(_renderer,
            SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            width, height);

        if (_texture == IntPtr.Zero)
        {
            Log.Error("Window", $"SDL_CreateTexture failed: {SDL.SDL_GetError()}");
            throw new Exception($"SDL_CreateTexture failed: {SDL.SDL_GetError()}");
        }
    }

    public void Run()
    {
        _running = true;
        SDL.SDL_Event e;

        while (_running)
        {
            while (SDL.SDL_PollEvent(out e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    _running = false;
                }
                else if (e.type == SDL.SDL_EventType.SDL_WINDOWEVENT &&
                         e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                {
                    Width = e.window.data1;
                    Height = e.window.data2;
                    RecreateTexture();
                }

                OnEvent?.Invoke(e);
            }

            OnUpdate?.Invoke();
            OnRender?.Invoke();
        }
    }

    public void PresentTexture(IntPtr pixels, int pitch)
    {
        SDL.SDL_Rect fullRect = new SDL.SDL_Rect { x = 0, y = 0, w = Width, h = Height };
        SDL.SDL_UpdateTexture(_texture, ref fullRect, pixels, pitch);
        SDL.SDL_RenderCopy(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
        SDL.SDL_RenderPresent(_renderer);
    }

    public void Stop()
    {
        _running = false;
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
        SDL.SDL_SetWindowSize(_window, width, height);
        RecreateTexture();
    }

    private void RecreateTexture()
    {
        if (_texture != IntPtr.Zero)
        {
            SDL.SDL_DestroyTexture(_texture);
        }
        _texture = SDL.SDL_CreateTexture(_renderer,
            SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            Width, Height);
    }

    public void Dispose()
    {
        if (_texture != IntPtr.Zero) SDL.SDL_DestroyTexture(_texture);
        if (_renderer != IntPtr.Zero) SDL.SDL_DestroyRenderer(_renderer);
        if (_window != IntPtr.Zero) SDL.SDL_DestroyWindow(_window);
        SDL.SDL_Quit();
    }
}
