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
    private SkiaGraphics? _graphics;
    private InputSystem? _input;
    private GameTimer? _timer;
    private AudioSystem? _audio;

    public void Run(Sketch sketch, string title = "SpriteForge Sketch", int defaultWidth = 800, int defaultHeight = 600)
    {
        Log.Initialize();

        _window = new GameWindow();
        _graphics = new SkiaGraphics();
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
        _graphics.Initialize(defaultWidth, defaultHeight);

        // 连接 P5 API
        SP5.Graphics = _graphics;
        SP5.Width = defaultWidth;
        SP5.Height = defaultHeight;
        SP5.Input = _input;

        // 注入引用到 Sketch
        sketch.AppWindow = _window;
        sketch.AppGraphics = _graphics;
        sketch.AppInput = _input;
        sketch.AppAudio = _audio;

        _window.OnEvent += _input.HandleEvent;

        // 调用 Setup（此时 Sketch 可以调用 Size() 调整窗口）
        sketch.Setup();

        // 如果 Setup 中修改了尺寸，同步到 Renderer
        if (_graphics.Width != SP5.Width || _graphics.Height != SP5.Height)
        {
            _graphics.Resize(SP5.Width, SP5.Height);
        }

        _window.OnUpdate += () =>
        {
            _timer!.Update();
            _input!.PostUpdate();
            SP5.DeltaTime = _timer.DeltaTime;
            SP5.FrameCount = _timer.FrameCount;

            sketch.Update(_timer.DeltaTime);
            _audio?.Update();

            if (_input.IsKeyPressed(SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                _window.Stop();
            }
        };

        _window.OnRender += () =>
        {
            _graphics!.BeginFrame();
            sketch.Draw();
            _graphics.EndFrame();
            _graphics.Present(_window.SdlRenderer, _window.SdlTexture);
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
        _graphics?.Dispose();
        _window?.Dispose();
    }
}
