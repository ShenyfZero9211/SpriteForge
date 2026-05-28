using NLua;
using SpriteCore.Graphics;
using SpriteCore.Window;

namespace SpriteCore.Scripting;

public class ScriptEngine : IDisposable
{
    private Lua? _lua;
    private LuaFunction? _setupFunc;
    private LuaFunction? _drawFunc;
    private LuaFunction? _updateFunc;

    public bool HasSetup => _setupFunc != null;
    public bool HasDraw => _drawFunc != null;
    public bool HasUpdate => _updateFunc != null;

    public void Initialize()
    {
        _lua = new Lua();
        _lua.LoadCLRPackage();
        RegisterP5API();
        RegisterInputAPI();
    }

    public void LoadScript(string code)
    {
        if (_lua == null) throw new InvalidOperationException("ScriptEngine not initialized");
        _lua.DoString(code);
        _setupFunc = _lua["setup"] as LuaFunction;
        _drawFunc = _lua["draw"] as LuaFunction;
        _updateFunc = _lua["update"] as LuaFunction;
    }

    public void LoadScriptFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Lua script not found: {path}");
        var code = File.ReadAllText(path);
        LoadScript(code);
    }

    public void CallSetup()
    {
        try
        {
            _setupFunc?.Call();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Lua Error] setup: {ex.Message}");
        }
    }

    public void CallDraw()
    {
        try
        {
            _drawFunc?.Call();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            Console.WriteLine($"[Lua Error] draw: {msg}");
        }
    }

    public void CallUpdate(float dt)
    {
        try
        {
            _updateFunc?.Call(dt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Lua Error] update: {ex.Message}");
        }
    }

    private void RegisterP5API()
    {
        if (_lua == null) return;

        // Colors
        _lua.RegisterFunction("background", null, typeof(P5).GetMethod("Background", new[] { typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("fill", null, typeof(P5).GetMethod("Fill", new[] { typeof(float), typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("stroke", null, typeof(P5).GetMethod("Stroke", new[] { typeof(float), typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("noStroke", null, typeof(P5).GetMethod("NoStroke")!);
        _lua.RegisterFunction("noFill", null, typeof(P5).GetMethod("NoFill")!);
        _lua.RegisterFunction("strokeWeight", null, typeof(P5).GetMethod("StrokeWeight")!);

        // Shapes
        _lua.RegisterFunction("rect", null, typeof(P5).GetMethod("Rect")!);
        _lua.RegisterFunction("ellipse", null, typeof(P5).GetMethod("Ellipse")!);
        _lua.RegisterFunction("circle", null, typeof(P5).GetMethod("Circle")!);
        _lua.RegisterFunction("line", null, typeof(P5).GetMethod("Line")!);
        _lua.RegisterFunction("triangle", null, typeof(P5).GetMethod("Triangle")!);

        // Transforms
        _lua.RegisterFunction("pushMatrix", null, typeof(P5).GetMethod("PushMatrix")!);
        _lua.RegisterFunction("popMatrix", null, typeof(P5).GetMethod("PopMatrix")!);
        _lua.RegisterFunction("translate", null, typeof(P5).GetMethod("Translate")!);
        _lua.RegisterFunction("rotate", null, typeof(P5).GetMethod("Rotate")!);
        _lua.RegisterFunction("scale", null, typeof(P5).GetMethod("Scale")!);

        // Text
        _lua.RegisterFunction("text", null, typeof(P5).GetMethod("Text")!);
        _lua.RegisterFunction("textSize", null, typeof(P5).GetMethod("TextSize")!);

        // Environment
        _lua.RegisterFunction("width", null, typeof(P5).GetMethod("GetWidth")!);
        _lua.RegisterFunction("height", null, typeof(P5).GetMethod("GetHeight")!);
        _lua.RegisterFunction("millis", null, typeof(P5).GetMethod("Millis")!);
    }

    private void RegisterInputAPI()
    {
        if (_lua == null) return;

        _lua.RegisterFunction("mouseX", null, typeof(P5).GetMethod("GetMouseX")!);
        _lua.RegisterFunction("mouseY", null, typeof(P5).GetMethod("GetMouseY")!);
        _lua.RegisterFunction("mouseIsPressed", null, typeof(P5).GetMethod("GetMouseIsPressed")!);
    }

    public void Dispose()
    {
        _setupFunc?.Dispose();
        _drawFunc?.Dispose();
        _updateFunc?.Dispose();
        _lua?.Dispose();
    }
}
