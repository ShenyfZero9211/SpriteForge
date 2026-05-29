using SkiaSharp;
using SpriteCore.Window;

namespace SpriteCore.Graphics;

/// <summary>
/// SP5 是 SpriteForge 的静态 API 门面。
/// 所有绘图方法委托给内部的 SPGraphics 渲染器实例。
/// 对应 Processing 中的 PApplet（但为静态门面而非类实例）。
/// </summary>
public static class SP5
{
    public static SPGraphics? Graphics { get; set; }
    public static InputSystem? Input { get; set; }

    public static int Width { get; set; }
    public static int Height { get; set; }
    public static int FrameCount { get; set; }
    public static float DeltaTime { get; set; }

    // ── 尺寸查询（供 Lua 绑定）──
    public static int GetWidth() => Width;
    public static int GetHeight() => Height;
    public static float GetMouseX() => MouseX;
    public static float GetMouseY() => MouseY;
    public static bool GetMouseIsPressed() => MouseIsPressed;

    // ── 颜色 ──
    public static void Background(float gray) => Graphics?.Background(gray);
    public static void Background(float r, float g, float b) => Graphics?.Background(r, g, b);
    public static void Background(float r, float g, float b, float a) => Graphics?.Background(r, g, b, a);

    // ── 样式 ──
    public static void Fill(float gray) => Graphics?.Fill(gray);
    public static void Fill(float r, float g, float b, float a = 255) => Graphics?.Fill(r, g, b, a);
    public static void Stroke(float gray) => Graphics?.Stroke(gray);
    public static void Stroke(float r, float g, float b, float a = 255) => Graphics?.Stroke(r, g, b, a);
    public static void NoStroke() => Graphics?.NoStroke();
    public static void NoFill() => Graphics?.NoFill();
    public static void StrokeWeight(float weight) => Graphics?.StrokeWeight(weight);

    // ── 形状 ──
    public static void Rect(float x, float y, float w, float h) => Graphics?.Rect(x, y, w, h);
    public static void Ellipse(float x, float y, float w, float h) => Graphics?.Ellipse(x, y, w, h);
    public static void Circle(float x, float y, float r) => Graphics?.Circle(x, y, r);
    public static void Line(float x1, float y1, float x2, float y2) => Graphics?.Line(x1, y1, x2, y2);
    public static void Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
        => Graphics?.Triangle(x1, y1, x2, y2, x3, y3);

    // ── 变换 ──
    public static void PushMatrix() => Graphics?.PushMatrix();
    public static void PopMatrix() => Graphics?.PopMatrix();
    public static void Translate(float x, float y) => Graphics?.Translate(x, y);
    public static void Rotate(float angle) => Graphics?.Rotate(angle);
    public static void Scale(float x, float y) => Graphics?.Scale(x, y);

    // ── 文字 ──
    public static void TextSize(float size) => Graphics?.TextSize(size);
    public static void Text(string str, float x, float y) => Graphics?.Text(str, x, y);

    // ── 图像 ──
    public static void Image(SKBitmap bitmap, float x, float y) => Graphics?.Image(bitmap, x, y);

    // ── 输入 ──
    public static float MouseX => Input?.MouseX ?? 0;
    public static float MouseY => Input?.MouseY ?? 0;
    public static bool MouseIsPressed => Input?.MouseIsPressed ?? false;
    public static bool IsKeyPressed(int keyCode)
        => Input?.IsKeyPressed((SDL2.SDL.SDL_Keycode)keyCode) ?? false;

    // ── Lua 适配方法（无重载，NLua 可正确匹配可变参数数量）──
    public static void LuaBackground(float r, float g = -1, float b = -1, float a = 255)
    {
        if (g < 0) Background(r);
        else if (b < 0) Background(r, g, a);
        else Background(r, g, b, a);
    }

    public static void LuaFill(float r, float g = -1, float b = -1, float a = 255)
    {
        if (g < 0) Fill(r);
        else Fill(r, g, b, a);
    }

    public static void LuaStroke(float r, float g = -1, float b = -1, float a = 255)
    {
        if (g < 0) Stroke(r);
        else Stroke(r, g, b, a);
    }

    // ── 环境 ──
    public static long Millis()
        => DateTimeOffset.Now.ToUnixTimeMilliseconds();
}
