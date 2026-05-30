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
    internal SkiaGraphics? AppGraphics { get; set; }
    public InputSystem? AppInput { get; internal set; }
    public AudioSystem? AppAudio { get; internal set; }

    // ── 生命周期（子类可 override）──
    public virtual void Setup() { }
    public virtual void Draw() { }
    public virtual void Update(float dt) { }
    public virtual void Dispose() { }

    // ── 尺寸 ──
    public void Size(int width, int height)
    {
        SP5.Width = width;
        SP5.Height = height;
        AppWindow?.Resize(width, height);
        SP5.Graphics?.Resize(width, height);
    }

    // ── 绘图 API（委托给 P5）──
    public void Background(float gray) => SP5.Background(gray);
    public void Background(float r, float g, float b) => SP5.Background(r, g, b);
    public void Background(float r, float g, float b, float a) => SP5.Background(r, g, b, a);

    public void Fill(float gray) => SP5.Fill(gray);
    public void Fill(float r, float g, float b, float a = 255) => SP5.Fill(r, g, b, a);
    public void Stroke(float gray) => SP5.Stroke(gray);
    public void Stroke(float r, float g, float b, float a = 255) => SP5.Stroke(r, g, b, a);
    public void NoStroke() => SP5.NoStroke();
    public void NoFill() => SP5.NoFill();
    public void StrokeWeight(float weight) => SP5.StrokeWeight(weight);
    public void StrokeCap(int cap) => SP5.StrokeCap(cap);
    public void StrokeJoin(int join) => SP5.StrokeJoin(join);

    public void RectMode(int mode) => SP5.RectMode(mode);
    public void EllipseMode(int mode) => SP5.EllipseMode(mode);
    public void ImageMode(int mode) => SP5.ImageMode(mode);
    public void TextAlign(int alignH) => SP5.TextAlign(alignH);
    public void TextAlign(int alignH, int alignV) => SP5.TextAlign(alignH, alignV);
    public void PushStyle() => SP5.PushStyle();
    public void PopStyle() => SP5.PopStyle();

    public void Rect(float x, float y, float w, float h) => SP5.Rect(x, y, w, h);
    public void RoundRect(float x, float y, float w, float h, float r) => SP5.RoundRect(x, y, w, h, r);
    public void Ellipse(float x, float y, float w, float h) => SP5.Ellipse(x, y, w, h);
    public void Circle(float x, float y, float r) => SP5.Circle(x, y, r);
    public void Line(float x1, float y1, float x2, float y2) => SP5.Line(x1, y1, x2, y2);
    public void Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
        => SP5.Triangle(x1, y1, x2, y2, x3, y3);
    public void Point(float x, float y) => SP5.Point(x, y);
    public void Quad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        => SP5.Quad(x1, y1, x2, y2, x3, y3, x4, y4);
    public void Arc(float x, float y, float w, float h, float start, float stop)
        => SP5.Arc(x, y, w, h, start, stop);

    public void PushMatrix() => SP5.PushMatrix();
    public void PopMatrix() => SP5.PopMatrix();
    public void Translate(float x, float y) => SP5.Translate(x, y);
    public void Rotate(float angle) => SP5.Rotate(angle);
    public void Scale(float x, float y) => SP5.Scale(x, y);

    public void TextSize(float size) => SP5.TextSize(size);
    public void Text(string str, float x, float y) => SP5.Text(str, x, y);

    // ── 图像 ──
    public SPTexture LoadImage(string path) => SP5.LoadImage(path);
    public void Image(SKBitmap bitmap, float x, float y) => SP5.Image(bitmap, x, y);
    public void Image(SPTexture texture, float x, float y) => SP5.Image(texture, x, y);
    public void Image(SPTexture texture, float x, float y, float w, float h) => SP5.Image(texture, x, y, w, h);

    // ── Tint ──
    public void Tint(float gray, float alpha = 255) => SP5.Tint(gray, alpha);
    public void Tint(float r, float g, float b, float a = 255) => SP5.Tint(r, g, b, a);
    public void NoTint() => SP5.NoTint();

    // ── 输入属性 ──
    public float MouseX => SP5.MouseX;
    public float MouseY => SP5.MouseY;
    public bool MouseIsPressed => SP5.MouseIsPressed;
    public bool IsKeyPressed(int keyCode) => SP5.IsKeyPressed(keyCode);

    // ── 环境属性 ──
    public int Width => SP5.Width;
    public int Height => SP5.Height;
    public float DeltaTime => SP5.DeltaTime;
    public int FrameCount => SP5.FrameCount;
    public long Millis() => SP5.Millis();

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
