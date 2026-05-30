using System.Reflection;
using SpriteCore.Graphics;
using SpriteCore.Scripting;
using SpriteCore.Time;
using SpriteCore.Utils;
using SpriteCore.Window;
using SpriteEngine.Framework;
using SDL2;

class Program
{
    static void Main(string[] args)
    {
        // 判断模式（优先级：--framework > --sketch > .lua > 自动发现）
        if (args.Length > 0 && args[0] == "--framework" && args.Length > 1)
        {
            RunFrameworkByName(args[1]);
            return;
        }

        if (args.Length > 0 && args[0].EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
        {
            RunLua(args[0]);
            return;
        }

        if (args.Length > 0 && args[0] == "--sketch" && args.Length > 1)
        {
            RunSketchByName(args[1]);
            return;
        }

        // 自动发现 SPframework 子类（优先）
        var fwType = FindFrameworkType();
        if (fwType != null)
        {
            RunFramework(fwType);
            return;
        }

        // 自动发现 C# Sketch 子类
        var sketchType = FindSketchType();
        if (sketchType != null)
        {
            RunSketch(sketchType);
            return;
        }

        // 回退到默认 Lua 示例
        RunLua("samples/01_HelloSprite/main.lua");
    }

    // ── SPframework 模式 ──

    static Type? FindFrameworkType()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fwTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(SPframework)) && !t.IsAbstract)
            .ToList();

        if (fwTypes.Count == 0) return null;
        if (fwTypes.Count == 1) return fwTypes[0];
        return fwTypes.OrderBy(t => t.Name).First();
    }

    static void RunFrameworkByName(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(SPframework))
                && !t.IsAbstract
                && (t.Name == name || t.FullName == name));

        if (type == null)
        {
            Log.Initialize();
            Log.Error("Launcher", $"Framework not found: {name}");
            Log.Info("Launcher", "Usage: SpriteLauncher --framework <ClassName>");
            return;
        }

        RunFramework(type);
    }

    static void RunFramework(Type fwType)
    {
        var fw = (SPframework)Activator.CreateInstance(fwType)!;
        fw.Launch();
    }

    // ── C# Sketch 模式 ──

    static Type? FindSketchType()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var sketchTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Sketch)) && !t.IsAbstract)
            .ToList();

        if (sketchTypes.Count == 0) return null;
        if (sketchTypes.Count == 1) return sketchTypes[0];

        // 多个时取第一个（按字母序，保证稳定）
        return sketchTypes.OrderBy(t => t.Name).First();
    }

    static void RunSketchByName(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(Sketch))
                && !t.IsAbstract
                && (t.Name == name || t.FullName == name));

        if (type == null)
        {
            Log.Initialize();
            Log.Error("Launcher", $"Sketch not found: {name}");
            Log.Info("Launcher", "Usage: SpriteLauncher --sketch <ClassName>");
            return;
        }

        RunSketch(type);
    }

    static void RunSketch(Type sketchType)
    {
        var sketch = (Sketch)Activator.CreateInstance(sketchType)!;
        using var app = new SketchApp();
        app.Run(sketch, $"SpriteForge - {sketchType.Name}");
    }

    // ── Lua 模式（原有行为完整保留）──

    static void RunLua(string scriptPath)
    {
        const int width = 800;
        const int height = 600;

        if (!Path.IsPathRooted(scriptPath))
        {
            // 尝试多个基目录：当前工作目录、AppContext.BaseDirectory
            string[] bases = new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory,
            };

            foreach (var baseDir in bases)
            {
                var candidate = Path.Combine(baseDir, scriptPath);
                if (File.Exists(candidate))
                {
                    scriptPath = candidate;
                    break;
                }
            }
        }

        Log.Initialize();

        if (!File.Exists(scriptPath))
        {
            Log.Error("Launcher", $"Script not found: {scriptPath}");
            Log.Info("Launcher", "Usage: SpriteLauncher <path-to-main.lua>");
            return;
        }

        using var window = new GameWindow();
        var graphics = new SkiaGraphics();
        using var input = new InputSystem();
        using var scriptEngine = new ScriptEngine();
        var timer = new GameTimer();

        window.Create("SpriteForge", width, height);
        graphics.Initialize(width, height);

        SP5.Graphics = graphics;
        SP5.Width = width;
        SP5.Height = height;
        SP5.Input = input;

        window.OnEvent += input.HandleEvent;

        scriptEngine.Initialize();
        scriptEngine.LoadScriptFromFile(scriptPath);
        scriptEngine.CallSetup();

        var watcher = new FileSystemWatcher(Path.GetDirectoryName(scriptPath)!, Path.GetFileName(scriptPath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        watcher.Changed += (s, e) =>
        {
            try
            {
                Thread.Sleep(100);
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
            SP5.DeltaTime = timer.DeltaTime;
            SP5.FrameCount = timer.FrameCount;

            scriptEngine.CallUpdate(timer.DeltaTime);
            scriptEngine.AudioUpdate();

            if (input.IsKeyPressed(SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                window.Stop();
            }
        };

        window.OnRender += () =>
        {
            graphics.BeginFrame();
            scriptEngine.CallDraw();
            graphics.EndFrame();
            graphics.Present(window.SdlRenderer, window.SdlTexture);
            timer.CapFrameRate(60);
        };

        Log.Info("Launcher", $"Starting Lua script: {scriptPath}");
        window.Run();

        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
        Log.Info("Launcher", "Goodbye!");
        Log.Shutdown();
    }
}
