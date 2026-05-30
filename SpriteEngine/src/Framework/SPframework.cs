using SpriteCore.Graphics;
using SpriteCore.Window;
using SpriteCore.Audio;
using SpriteCore;
using SpriteEngine.Scenes;

namespace SpriteEngine.Framework;

/// <summary>
/// High-level game framework abstraction. Subclass this to build a game
/// without touching Sketch/SketchApp directly.  Under the hood an
/// <see cref="InnerSketch"/> bridges to the existing engine loop.
/// </summary>
public abstract class SPframework
{
    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    /// <summary>Window title (read-only after launch).</summary>
    public string Title => _title;

    /// <summary>Current window width in pixels.</summary>
    public int Width => SP5.Width;

    /// <summary>Current window height in pixels.</summary>
    public int Height => SP5.Height;

    /// <summary>Elapsed time since launch in seconds.</summary>
    public float Elapsed => _elapsed;

    /// <summary>Delta time for the current frame in seconds.</summary>
    public float DeltaTime => SP5.DeltaTime;

    /// <summary>Total number of frames rendered.</summary>
    public int FrameCount => SP5.FrameCount;

    /// <summary>Input system (null before <see cref="Launch"/>).</summary>
    public InputSystem? Input => _input;

    /// <summary>Audio system (null before <see cref="Launch"/>).</summary>
    public AudioSystem? Audio { get; internal set; }

    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private string _title = "SPframework Game";
    private int _desiredWidth = 800;
    private int _desiredHeight = 600;
    private float _elapsed;
    private InputSystem? _input;
    private SketchApp? _app;

    // ------------------------------------------------------------------
    // Lifecycle (called by user code)
    // ------------------------------------------------------------------

    /// <summary>
    /// Set window size before calling <see cref="Launch"/>.  Defaults to 800×600.
    /// </summary>
    public void Size(int width, int height)
    {
        _desiredWidth = width;
        _desiredHeight = height;
    }

    /// <summary>
    /// Set window title before calling <see cref="Launch"/>.  Defaults to "SPframework Game".
    /// </summary>
    public void TitleSet(string title)
    {
        _title = title ?? "SPframework Game";
    }

    /// <summary>
    /// Start the game loop.  Blocks until the window is closed.
    /// </summary>
    public void Launch()
    {
        _app = new SketchApp();
        var inner = new InnerSketch(this);
        _app.Run(inner, _title, _desiredWidth, _desiredHeight);
    }

    // ------------------------------------------------------------------
    // Override hooks (subclass implements)
    // ------------------------------------------------------------------

    /// <summary>
    /// Called once before the first frame.  Use to create scenes, load assets, etc.
    /// </summary>
    public virtual void Init() { }

    /// <summary>
    /// Called every frame.  Use for game logic, scene updates, etc.
    /// </summary>
    public virtual void Update(float dt)
    {
        _elapsed += dt;
    }

    /// <summary>
    /// Called every frame after <see cref="Update"/>.  Use for rendering.
    /// </summary>
    public virtual void Render() { }

    /// <summary>
    /// Called once when the engine loop stops.  Use for cleanup.
    /// </summary>
    public virtual void Dispose() { }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>Register a scene with the <see cref="SceneManager"/> singleton.</summary>
    public void RegisterScene(string name, Scene scene)
        => SceneManager.Instance.Register(name, scene);

    /// <summary>Switch to a previously registered scene.</summary>
    public void LoadScene(string name)
        => SceneManager.Instance.Load(name);

    // ------------------------------------------------------------------
    // Inner bridge
    // ------------------------------------------------------------------

    private class InnerSketch : Sketch
    {
        private readonly SPframework _fw;

        public InnerSketch(SPframework fw) => _fw = fw;

        public override void Setup()
        {
            if (_fw._desiredWidth != 800 || _fw._desiredHeight != 600)
                base.Size(_fw._desiredWidth, _fw._desiredHeight);

            _fw.Audio = AppAudio;
            _fw._input = AppInput;
            _fw.Init();
        }

        public override void Update(float dt)
        {
            _fw.Update(dt);
            SceneManager.Instance.Update(dt);
        }

        public override void Draw()
        {
            _fw.Render();
            SceneManager.Instance.Render();
        }

        public override void Dispose()
        {
            _fw.Dispose();
            base.Dispose();
        }
    }
}
