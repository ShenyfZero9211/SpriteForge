using SkiaSharp;
using SpriteCore.Audio;
using SpriteCore.Math;
using SpriteCore.Utils;
using SpriteCore.Window;

namespace SpriteCore.Graphics;

/// <summary>
/// Processing-like 抽象基类，供 C# 开发者以面向对象方式编写 Sketch。
/// Lua 层保持简单，C# 层获得类型安全与完整 .NET 生态。
/// </summary>
public abstract class Sketch
{
    // 内部引用，由 SketchApp 注入
    internal GameWindow? AppWindow { get; set; }
    internal Renderer? AppRenderer { get; set; }
    internal InputSystem? AppInput { get; set; }
    internal AudioSystem? AppAudio { get; set; }

    // ── 生命周期（子类可 override）──
    public virtual void Setup() { }
    public virtual void Draw() { }
    public virtual void Update(float dt) { }

    // ── 尺寸 ──
    public void Size(int width, int height)
    {
        P5.Width = width;
        P5.Height = height;
        AppWindow?.Resize(width, height);
        AppRenderer?.Resize(width, height);
    }

    // ── 绘图 API（委托给 P5）──
    public void Background(float gray) => P5.Background(gray);
    public void Background(float r, float g, float b) => P5.Background(r, g, b);
    public void Background(float r, float g, float b, float a) => P5.Background(r, g, b, a);

    public void Fill(float gray) => P5.Fill(gray);
    public void Fill(float r, float g, float b, float a = 255) => P5.Fill(r, g, b, a);
    public void Stroke(float gray) => P5.Stroke(gray);
    public void Stroke(float r, float g, float b, float a = 255) => P5.Stroke(r, g, b, a);
    public void NoStroke() => P5.NoStroke();
    public void NoFill() => P5.NoFill();
    public void StrokeWeight(float weight) => P5.StrokeWeight(weight);

    public void Rect(float x, float y, float w, float h) => P5.Rect(x, y, w, h);
    public void Ellipse(float x, float y, float w, float h) => P5.Ellipse(x, y, w, h);
    public void Circle(float x, float y, float r) => P5.Circle(x, y, r);
    public void Line(float x1, float y1, float x2, float y2) => P5.Line(x1, y1, x2, y2);
    public void Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
        => P5.Triangle(x1, y1, x2, y2, x3, y3);

    public void PushMatrix() => P5.PushMatrix();
    public void PopMatrix() => P5.PopMatrix();
    public void Translate(float x, float y) => P5.Translate(x, y);
    public void Rotate(float angle) => P5.Rotate(angle);
    public void Scale(float x, float y) => P5.Scale(x, y);

    public void TextSize(float size) => P5.TextSize(size);
    public void Text(string str, float x, float y) => P5.Text(str, x, y);

    // ── 输入属性 ──
    public float MouseX => P5.MouseX;
    public float MouseY => P5.MouseY;
    public bool MouseIsPressed => P5.MouseIsPressed;
    public bool IsKeyPressed(int keyCode) => P5.IsKeyPressed(keyCode);

    // ── 环境属性 ──
    public int Width => P5.Width;
    public int Height => P5.Height;
    public float DeltaTime => P5.DeltaTime;
    public int FrameCount => P5.FrameCount;
    public long Millis() => P5.Millis();

    // ── 数学工具（静态方法，直接暴露）──
    public static float Random(float min, float max) => MathUtils.RandomRange(min, max);
    public static float Noise(float x) => MathUtils.Noise(x);
    public static float Lerp(float a, float b, float t) => MathUtils.Lerp(a, b, t);
    public static float Clamp(float v, float min, float max) => MathUtils.Clamp(v, min, max);
    public static float Clamp01(float v) => MathUtils.Clamp01(v);
    public static float Map(float v, float iMin, float iMax, float oMin, float oMax)
        => MathUtils.Map(v, iMin, iMax, oMin, oMax);
    public static float Dist(float x1, float y1, float x2, float y2)
        => MathUtils.Distance(x1, y1, x2, y2);

    public static float Sin(float rad) => MathUtils.Sin(rad);
    public static float Cos(float rad) => MathUtils.Cos(rad);
    public static float Tan(float rad) => MathUtils.Tan(rad);
    public static float Atan2(float y, float x) => MathUtils.Atan2(y, x);
    public static float Degrees(float rad) => MathUtils.Degrees(rad);
    public static float Radians(float deg) => MathUtils.Radians(deg);

    public static float Abs(float v) => MathUtils.Abs(v);
    public static float Floor(float v) => MathUtils.Floor(v);
    public static float Ceil(float v) => MathUtils.Ceil(v);
    public static float Round(float v) => MathUtils.Round(v);
    public static float Sqrt(float v) => MathUtils.Sqrt(v);
    public static float Pow(float x, float y) => MathUtils.Pow(x, y);

    // ── 音频 API（委托给 AudioSystem）──
    public void LoadSound(string name, string filePath)
        => AppAudio?.LoadSound(name, filePath);

    public void PlaySound(string name)
        => AppAudio?.PlaySound(name);

    public void PlaySound(string name, float volume)
        => AppAudio?.PlaySound(name, volume);

    public void PlayMusic(string name)
        => AppAudio?.PlayMusic(name);

    public void PlayMusic(string name, float volume)
        => AppAudio?.PlayMusic(name, volume);

    public void StopAllSounds()
        => AppAudio?.StopAll();

    public void SetVolume(float volume)
        => AppAudio?.SetMasterVolume(volume);
}
