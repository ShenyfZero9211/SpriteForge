using SpriteCore.Graphics;
using SpriteCore.Scripting;
using SpriteCore.Time;
using SpriteCore.Utils;
using SpriteCore.Window;
using SDL2;

class Program
{
    static void Main(string[] args)
    {
        const int width = 800;
        const int height = 600;

        // 确定脚本路径
        string scriptPath = args.Length > 0 ? args[0] : "samples/01_HelloSprite/main.lua";
        if (!Path.IsPathRooted(scriptPath))
        {
            scriptPath = Path.Combine(AppContext.BaseDirectory, scriptPath);
        }

        Log.Initialize();

        if (!File.Exists(scriptPath))
        {
            Log.Error("Launcher", $"Script not found: {scriptPath}");
            Log.Info("Launcher", "Usage: SpriteLauncher <path-to-main.lua>");
            return;
        }

        using var window = new GameWindow();
        using var renderer = new Renderer();
        using var input = new InputSystem();
        using var scriptEngine = new ScriptEngine();
        var timer = new GameTimer();

        window.Create("SpriteForge - Phase 1", width, height);
        renderer.Initialize(width, height);

        // 连接 P5 API
        P5.Width = width;
        P5.Height = height;
        P5.Input = input;

        window.OnEvent += input.HandleEvent;

        // 加载 Lua 脚本
        scriptEngine.Initialize();
        scriptEngine.LoadScriptFromFile(scriptPath);
        P5.Canvas = renderer.Canvas;
        scriptEngine.CallSetup();

        // 热重载
        var watcher = new FileSystemWatcher(Path.GetDirectoryName(scriptPath)!, Path.GetFileName(scriptPath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        watcher.Changed += (s, e) =>
        {
            try
            {
                Thread.Sleep(100); // 防抖
                scriptEngine.LoadScriptFromFile(scriptPath);
                scriptEngine.CallSetup();
                Log.Info("HotReload", $"Reloaded: {scriptPath}");
            }
            catch (Exception ex)
            {
                Log.Error("HotReload", ex.Message);
            }
        };

        window.OnUpdate += () =>
        {
            timer.Update();
            input.PostUpdate();
            P5.DeltaTime = timer.DeltaTime;
            P5.FrameCount = timer.FrameCount;

            scriptEngine.CallUpdate(timer.DeltaTime);
            scriptEngine.AudioUpdate();

            if (input.IsKeyPressed(SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                window.Stop();
            }
        };

        window.OnRender += () =>
        {
            renderer.BeginFrame();
            P5.Canvas = renderer.Canvas;
            scriptEngine.CallDraw();
            renderer.EndFrame();
            renderer.PresentToSdlRenderer(window.SdlRenderer, window.SdlTexture);
            timer.CapFrameRate(60);
        };

        Log.Info("Launcher", $"Starting SpriteCore with script: {scriptPath}");
        window.Run();

        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
        Log.Info("Launcher", "Goodbye!");
        Log.Shutdown();
    }
}
