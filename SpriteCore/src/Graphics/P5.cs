using SkiaSharp;

namespace SpriteCore.Graphics;

public static class P5
{
    public static SKCanvas? Canvas { get; set; }
    public static Window.InputSystem? Input { get; set; }

    public static int Width { get; set; }
    public static int Height { get; set; }
    public static int FrameCount { get; set; }
    public static float DeltaTime { get; set; }

    public static int GetWidth() => Width;
    public static int GetHeight() => Height;
    public static float GetMouseX() => MouseX;
    public static float GetMouseY() => MouseY;
    public static bool GetMouseIsPressed() => MouseIsPressed;

    private static readonly Stack<DrawingStyle> _styleStack = new();
    private static DrawingStyle CurrentStyle => _styleStack.Peek();

    static P5()
    {
        _styleStack.Push(new DrawingStyle());
    }

    public static void PushStyle() => _styleStack.Push(new DrawingStyle(CurrentStyle));
    public static void PopStyle()
    {
        if (_styleStack.Count > 1)
            _styleStack.Pop();
    }

    // Colors - accept float from Lua, convert to byte internally
    public static void Background(float r, float g, float b)
        => Canvas?.Clear(new SKColor((byte)r, (byte)g, (byte)b));

    public static void Background(float r, float g, float b, float a)
        => Canvas?.Clear(new SKColor((byte)r, (byte)g, (byte)b, (byte)a));

    public static void Background(float gray)
        => Canvas?.Clear(new SKColor((byte)gray, (byte)gray, (byte)gray));

    public static void Fill(float gray)
        => CurrentStyle.FillPaint.Color = new SKColor((byte)gray, (byte)gray, (byte)gray);

    public static void Fill(float r, float g, float b, float a = 255)
        => CurrentStyle.FillPaint.Color = new SKColor((byte)r, (byte)g, (byte)b, (byte)a);

    public static void Stroke(float gray)
        => CurrentStyle.StrokePaint.Color = new SKColor((byte)gray, (byte)gray, (byte)gray);

    public static void Stroke(float r, float g, float b, float a = 255)
        => CurrentStyle.StrokePaint.Color = new SKColor((byte)r, (byte)g, (byte)b, (byte)a);

    public static void NoStroke()
        => CurrentStyle.StrokePaint.Color = SKColors.Transparent;

    public static void NoFill()
        => CurrentStyle.FillPaint.Color = SKColors.Transparent;

    public static void StrokeWeight(float weight)
        => CurrentStyle.StrokePaint.StrokeWidth = weight;

    // Shapes
    public static void Rect(float x, float y, float w, float h)
    {
        if (Canvas == null) return;
        var rect = new SKRect(x, y, x + w, y + h);
        Canvas.DrawRect(rect, CurrentStyle.FillPaint);
        if (CurrentStyle.StrokePaint.Color.Alpha > 0)
            Canvas.DrawRect(rect, CurrentStyle.StrokePaint);
    }

    public static void Ellipse(float x, float y, float w, float h)
    {
        if (Canvas == null) return;
        var rect = new SKRect(x - w / 2, y - h / 2, x + w / 2, y + h / 2);
        Canvas.DrawOval(rect, CurrentStyle.FillPaint);
        if (CurrentStyle.StrokePaint.Color.Alpha > 0)
            Canvas.DrawOval(rect, CurrentStyle.StrokePaint);
    }

    public static void Circle(float x, float y, float r)
    {
        if (Canvas == null) return;
        Canvas.DrawCircle(x, y, r, CurrentStyle.FillPaint);
        if (CurrentStyle.StrokePaint.Color.Alpha > 0)
            Canvas.DrawCircle(x, y, r, CurrentStyle.StrokePaint);
    }

    public static void Line(float x1, float y1, float x2, float y2)
    {
        if (Canvas == null) return;
        Canvas.DrawLine(x1, y1, x2, y2, CurrentStyle.StrokePaint);
    }

    public static void Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        if (Canvas == null) return;
        using var path = new SKPath();
        path.MoveTo(x1, y1);
        path.LineTo(x2, y2);
        path.LineTo(x3, y3);
        path.Close();
        Canvas.DrawPath(path, CurrentStyle.FillPaint);
        if (CurrentStyle.StrokePaint.Color.Alpha > 0)
            Canvas.DrawPath(path, CurrentStyle.StrokePaint);
    }

    // Transforms
    public static void PushMatrix() => Canvas?.Save();
    public static void PopMatrix() => Canvas?.Restore();
    public static void Translate(float x, float y) => Canvas?.Translate(x, y);
    public static void Rotate(float angle) => Canvas?.RotateDegrees(angle);
    public static void Scale(float x, float y) => Canvas?.Scale(x, y);

    // Text
    public static void TextSize(float size)
        => CurrentStyle.TextSize = size;

    public static void Text(string str, float x, float y)
    {
        if (Canvas == null) return;
        using var font = new SKFont { Size = CurrentStyle.TextSize };
        Canvas.DrawText(str, x, y, SKTextAlign.Left, font, CurrentStyle.FillPaint);
    }

    // Images
    public static void Image(SKBitmap bitmap, float x, float y)
    {
        Canvas?.DrawBitmap(bitmap, x, y);
    }

    // Input
    public static float MouseX => Input?.MouseX ?? 0;
    public static float MouseY => Input?.MouseY ?? 0;
    public static bool MouseIsPressed => Input?.MouseIsPressed ?? false;
    public static bool IsKeyPressed(int keyCode)
        => Input?.IsKeyPressed((SDL2.SDL.SDL_Keycode)keyCode) ?? false;

    // Environment
    public static long Millis()
        => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    private class DrawingStyle
    {
        public SKPaint FillPaint = new() { Style = SKPaintStyle.Fill, IsAntialias = true };
        public SKPaint StrokePaint = new() { Style = SKPaintStyle.Stroke, IsAntialias = true, StrokeWidth = 1 };
        public float TextSize = 16;

        public DrawingStyle() { }

        public DrawingStyle(DrawingStyle other)
        {
            FillPaint = other.FillPaint.Clone();
            StrokePaint = other.StrokePaint.Clone();
            TextSize = other.TextSize;
        }
    }
}
