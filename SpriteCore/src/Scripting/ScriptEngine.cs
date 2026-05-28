using NLua;
using SpriteCore.Audio;
using SpriteCore.Graphics;
using SpriteCore.Math;
using SpriteCore.Utils;
using SpriteCore.Window;

namespace SpriteCore.Scripting;

public class ScriptEngine : IDisposable
{
    private Lua? _lua;
    private LuaFunction? _setupFunc;
    private LuaFunction? _drawFunc;
    private LuaFunction? _updateFunc;
    private AudioSystem? _audio;

    public bool HasSetup => _setupFunc != null;
    public bool HasDraw => _drawFunc != null;
    public bool HasUpdate => _updateFunc != null;

    public void Initialize()
    {
        _lua = new Lua();
        _lua.LoadCLRPackage();
        RegisterP5API();
        RegisterInputAPI();
        RegisterMathAPI();
        RegisterAudioAPI();
        RegisterLogAPI();
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
        try { _setupFunc?.Call(); }
        catch (Exception ex) { Log.Error("Lua", $"setup: {ex.Message}"); }
    }

    public void CallDraw()
    {
        try { _drawFunc?.Call(); }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            Log.Error("Lua", $"draw: {msg}");
        }
    }

    public void CallUpdate(float dt)
    {
        try { _updateFunc?.Call(dt); }
        catch (Exception ex) { Log.Error("Lua", $"update: {ex.Message}"); }
    }

    public void AudioUpdate()
    {
        try { _audio?.Update(); }
        catch (Exception ex) { Log.Error("Audio", $"update: {ex.Message}"); }
    }

    private void RegisterP5API()
    {
        if (_lua == null) return;

        _lua.RegisterFunction("background", null, typeof(P5).GetMethod("Background", new[] { typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("fill", null, typeof(P5).GetMethod("Fill", new[] { typeof(float), typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("stroke", null, typeof(P5).GetMethod("Stroke", new[] { typeof(float), typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("noStroke", null, typeof(P5).GetMethod("NoStroke")!);
        _lua.RegisterFunction("noFill", null, typeof(P5).GetMethod("NoFill")!);
        _lua.RegisterFunction("strokeWeight", null, typeof(P5).GetMethod("StrokeWeight")!);

        _lua.RegisterFunction("rect", null, typeof(P5).GetMethod("Rect")!);
        _lua.RegisterFunction("ellipse", null, typeof(P5).GetMethod("Ellipse")!);
        _lua.RegisterFunction("circle", null, typeof(P5).GetMethod("Circle")!);
        _lua.RegisterFunction("line", null, typeof(P5).GetMethod("Line")!);
        _lua.RegisterFunction("triangle", null, typeof(P5).GetMethod("Triangle")!);

        _lua.RegisterFunction("pushMatrix", null, typeof(P5).GetMethod("PushMatrix")!);
        _lua.RegisterFunction("popMatrix", null, typeof(P5).GetMethod("PopMatrix")!);
        _lua.RegisterFunction("translate", null, typeof(P5).GetMethod("Translate")!);
        _lua.RegisterFunction("rotate", null, typeof(P5).GetMethod("Rotate")!);
        _lua.RegisterFunction("scale", null, typeof(P5).GetMethod("Scale")!);

        _lua.RegisterFunction("text", null, typeof(P5).GetMethod("Text")!);
        _lua.RegisterFunction("textSize", null, typeof(P5).GetMethod("TextSize")!);

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

    private void RegisterMathAPI()
    {
        if (_lua == null) return;

        _lua.RegisterFunction("lerp", null, typeof(MathUtils).GetMethod("Lerp", new[] { typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("clamp", null, typeof(MathUtils).GetMethod("Clamp", new[] { typeof(float), typeof(float), typeof(float) })!);
        _lua.RegisterFunction("clamp01", null, typeof(MathUtils).GetMethod("Clamp01")!);
        _lua.RegisterFunction("map", null, typeof(MathUtils).GetMethod("Map")!);
        _lua.RegisterFunction("dist", null, typeof(MathUtils).GetMethod("Distance")!);
        _lua.RegisterFunction("abs", null, typeof(MathUtils).GetMethod("Abs")!);
        _lua.RegisterFunction("floor", null, typeof(MathUtils).GetMethod("Floor")!);
        _lua.RegisterFunction("ceil", null, typeof(MathUtils).GetMethod("Ceil")!);
        _lua.RegisterFunction("round", null, typeof(MathUtils).GetMethod("Round")!);
        _lua.RegisterFunction("sqrt", null, typeof(MathUtils).GetMethod("Sqrt")!);
        _lua.RegisterFunction("sin", null, typeof(MathUtils).GetMethod("Sin")!);
        _lua.RegisterFunction("cos", null, typeof(MathUtils).GetMethod("Cos")!);
        _lua.RegisterFunction("tan", null, typeof(MathUtils).GetMethod("Tan")!);
        _lua.RegisterFunction("atan2", null, typeof(MathUtils).GetMethod("Atan2")!);
        _lua.RegisterFunction("pow", null, typeof(MathUtils).GetMethod("Pow")!);
        _lua.RegisterFunction("degrees", null, typeof(MathUtils).GetMethod("Degrees")!);
        _lua.RegisterFunction("radians", null, typeof(MathUtils).GetMethod("Radians")!);

        // Random
        _lua.RegisterFunction("random", null, typeof(MathUtils).GetMethod("RandomRange", new[] { typeof(float), typeof(float) })!);

        // Noise
        _lua.RegisterFunction("noise", null, typeof(MathUtils).GetMethod("Noise", new[] { typeof(float) })!);

        // Vector2 factory
        _lua.RegisterFunction("Vector2", null, typeof(Vector2).GetMethod("LuaCreate")!);
    }

    private void RegisterAudioAPI()
    {
        if (_lua == null) return;

        try
        {
            _audio = new AudioSystem();
            _audio.Initialize();
            Log.Info("Audio", "Initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Error("Audio", $"Initialization failed: {ex.Message}");
            return;
        }

        _lua.RegisterFunction("loadSound", _audio, typeof(AudioSystem).GetMethod("LoadSound")!);
        _lua.RegisterFunction("playSound", _audio, typeof(AudioSystem).GetMethod("PlaySound", new[] { typeof(string) })!);
        _lua.RegisterFunction("playSoundVol", _audio, typeof(AudioSystem).GetMethod("PlaySound", new[] { typeof(string), typeof(float) })!);
        _lua.RegisterFunction("playMusic", _audio, typeof(AudioSystem).GetMethod("PlayMusic", new[] { typeof(string) })!);
        _lua.RegisterFunction("playMusicVol", _audio, typeof(AudioSystem).GetMethod("PlayMusic", new[] { typeof(string), typeof(float) })!);
        _lua.RegisterFunction("stopAllSounds", _audio, typeof(AudioSystem).GetMethod("StopAll")!);
        _lua.RegisterFunction("setVolume", _audio, typeof(AudioSystem).GetMethod("SetMasterVolume")!);
    }

    private void RegisterLogAPI()
    {
        if (_lua == null) return;
        _lua.RegisterFunction("log", null, typeof(Log).GetMethod("LuaLog")!);
    }

    public void Dispose()
    {
        _setupFunc?.Dispose();
        _drawFunc?.Dispose();
        _updateFunc?.Dispose();
        _audio?.Dispose();
        _lua?.Dispose();
    }
}
