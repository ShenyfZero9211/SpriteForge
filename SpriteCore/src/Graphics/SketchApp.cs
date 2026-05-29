using SpriteCore.Audio;
using SpriteCore.Time;
using SpriteCore.Utils;
using SpriteCore.Window;
using SDL2;

namespace SpriteCore.Graphics;

/// <summary>
/// 驱动 Sketch 的完整游戏循环。封装窗口、渲染、输入、计时和音频。
/// </summary>
public class SketchApp : IDisposable
{
    private GameWindow? _window;
    private Renderer? _renderer;
    private InputSystem? _input;
    private GameTimer? _timer;
    private AudioSystem? _audio;

    public void Run(Sketch sketch, string title = "SpriteForge Sketch", int defaultWidth = 800, int defaultHeight = 600)
    {
        Log.Initialize();

        _window = new GameWindow();
        _renderer = new Renderer();
        _input = new InputSystem();
        _timer = new GameTimer();

        try
        {
            _audio = new AudioSystem();
            _audio.Initialize();
            Log.Info("SketchApp", "Audio initialized");
        }
        catch (Exception ex)
        {
            Log.Warning("SketchApp", $"Audio initialization failed: {ex.Message}");
        }

        // 创建窗口（默认尺寸，Sketch.Setup 中可 Size 调整）
        _window.Create(title, defaultWidth, defaultHeight);
        _renderer.Initialize(defaultWidth, defaultHeight);

        // 连接 P5 API
        P5.Width = defaultWidth;
        P5.Height = defaultHeight;
        P5.Input = _input;

        // 注入引用到 Sketch
        sketch.AppWindow = _window;
        sketch.AppRenderer = _renderer;
        sketch.AppInput = _input;
        sketch.AppAudio = _audio;

        _window.OnEvent += _input.HandleEvent;

        // 调用 Setup（此时 Sketch 可以调用 Size() 调整窗口）
        sketch.Setup();

        // 如果 Setup 中修改了尺寸，同步到 Renderer
        if (_renderer.Width != P5.Width || _renderer.Height != P5.Height)
        {
            _renderer.Resize(P5.Width, P5.Height);
        }

        _window.OnUpdate += () =>
        {
            _timer!.Update();
            _input!.PostUpdate();
            P5.DeltaTime = _timer.DeltaTime;
            P5.FrameCount = _timer.FrameCount;

            sketch.Update(_timer.DeltaTime);
            _audio?.Update();

            if (_input.IsKeyPressed(SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                _window.Stop();
            }
        };

        _window.OnRender += () =>
        {
            _renderer!.BeginFrame();
            P5.Canvas = _renderer.Canvas;
            sketch.Draw();
            _renderer.EndFrame();
            _renderer.PresentToSdlRenderer(_window.SdlRenderer, _window.SdlTexture);
            _timer!.CapFrameRate(60);
        };

        Log.Info("SketchApp", $"Running sketch: {sketch.GetType().Name}");
        _window.Run();

        Log.Info("SketchApp", "Sketch stopped");
        Log.Shutdown();
    }

    public void Dispose()
    {
        _audio?.Dispose();
        _input?.Dispose();
        _renderer?.Dispose();
        _window?.Dispose();
    }
}
