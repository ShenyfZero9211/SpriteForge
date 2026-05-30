using SkiaSharp;
using SpriteCore;
using SpriteCore.Graphics;
using SpriteEngine.Framework;
using SpriteEngine.Scenes;
using static SpriteCore.Graphics.SP5;

namespace SpriteLauncher.GameSamples;

/// <summary>
/// A minimal game demo built on top of <see cref="SPframework"/>.
/// Shows a rotating square that follows the mouse and changes color on click.
/// </summary>
public class GameDemo : SPframework
{
    private float _angle;
    private float _squareX = 400f;
    private float _squareY = 300f;
    private float _squareSize = 60f;
    private SKColor _squareColor = new SKColor(100, 200, 255);
    private SKColor _targetColor = new SKColor(100, 200, 255);
    private readonly Random _rng = new();
    private float _colorLerp;

    public override void Init()
    {
        Size(800, 600);
        TitleSet("SPframework Demo — GameDemo");

        // Create a scene with a camera
        var scene = new Scene("Main");
        var camGO = scene.CreateGameObject("Camera");
        camGO.AddComponent<Camera>();
        RegisterScene("main", scene);
        LoadScene("main");
    }

    public override void Update(float dt)
    {
        _angle += 90f * dt;

        // Follow mouse with smooth lerp
        if (Input != null)
        {
            var mx = Input.MouseX;
            var my = Input.MouseY;
            _squareX += (mx - _squareX) * 5f * dt;
            _squareY += (my - _squareY) * 5f * dt;

            // Click to randomize color
            if (Input.IsMousePressed(0))
            {
                _targetColor = new SKColor(
                    (byte)_rng.Next(50, 256),
                    (byte)_rng.Next(50, 256),
                    (byte)_rng.Next(50, 256));
                _colorLerp = 0f;
            }
        }

        // Smooth color transition
        if (_colorLerp < 1f)
        {
            _colorLerp = Math.Min(1f, _colorLerp + dt * 3f);
            _squareColor = LerpColor(_squareColor, _targetColor, _colorLerp);
        }
    }

    public override void Render()
    {
        Background(30, 30, 40);

        // Draw grid lines
        StrokeWeight(1);
        Stroke(60, 60, 80);
        for (int x = 0; x < Width; x += 40)
        {
            Line(x, 0, x, Height);
        }
        for (int y = 0; y < Height; y += 40)
        {
            Line(0, y, Width, y);
        }

        // Draw rotating square
        PushMatrix();
        Translate(_squareX, _squareY);
        Rotate(_angle);

        NoStroke();
        Fill(_squareColor.Red, _squareColor.Green, _squareColor.Blue);
        RectMode((int)SpriteCore.Graphics.SPRectMode.CENTER);
        Rect(0, 0, _squareSize, _squareSize);

        // Inner highlight
        Fill(255, 255, 255, 80);
        Rect(0, 0, _squareSize * 0.5f, _squareSize * 0.5f);

        PopMatrix();
        RectMode((int)SpriteCore.Graphics.SPRectMode.CORNER); // reset to default

        // Draw info text
        Fill(200);
        TextSize(14);
        TextAlign((int)SpriteCore.Graphics.SPTextAlignH.LEFT, (int)SpriteCore.Graphics.SPTextAlignV.TOP);
        Text($"FPS: {MathF.Round(1f / DeltaTime)}", 10, 10);
        Text($"Frame: {FrameCount}", 10, 28);
        Text($"Elapsed: {Elapsed:F1}s", 10, 46);
        Text($"Mouse: {Input?.MouseX ?? 0}, {Input?.MouseY ?? 0}", 10, 64);
        Text("Click to change color", 10, 82);
    }

    private static SKColor LerpColor(SKColor a, SKColor b, float t)
    {
        return new SKColor(
            (byte)(a.Red + (b.Red - a.Red) * t),
            (byte)(a.Green + (b.Green - a.Green) * t),
            (byte)(a.Blue + (b.Blue - a.Blue) * t));
    }
}
