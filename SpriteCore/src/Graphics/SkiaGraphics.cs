using SkiaSharp;
using SDL2;
using SpriteCore.Utils;

namespace SpriteCore.Graphics;

/// <summary>
/// SkiaSharp 渲染器实现。管理 SKSurface/SKCanvas，实现所有抽象绘制方法。
/// 同时替代 Renderer 的职责（BeginFrame/EndFrame/Present）。
/// </summary>
public class SkiaGraphics : SPGraphics, IDisposable
{
    private SKSurface? _surface;
    private SKImageInfo _imageInfo;

    public SKCanvas? Canvas => _surface?.Canvas;

    // ── 生命周期 ──

    public override void BeginFrame()
    {
        _surface?.Canvas.Clear(SKColors.Black);
    }

    public override void EndFrame()
    {
        _surface?.Canvas.Flush();
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _surface = null;
    }

    public override void Resize(int width, int height)
    {
        _surface?.Dispose();
        Width = width;
        Height = height;
        _imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _surface = SKSurface.Create(_imageInfo);
    }

    public void Initialize(int width, int height)
    {
        Resize(width, height);
    }

    // ── 颜色 ──

    public override void Background(float r, float g, float b, float a)
    {
        _surface?.Canvas.Clear(new SKColor((byte)r, (byte)g, (byte)b, (byte)a));
    }

    // ── 形状绘制 ──

    public override void Rect(float x, float y, float w, float h)
    {
        var rect = new SKRect(x, y, x + w, y + h);
        DrawWithFillAndStroke(paint => _surface!.Canvas.DrawRect(rect, paint));
    }

    public override void Ellipse(float x, float y, float w, float h)
    {
        var rect = new SKRect(x - w / 2, y - h / 2, x + w / 2, y + h / 2);
        DrawWithFillAndStroke(paint => _surface!.Canvas.DrawOval(rect, paint));
    }

    public override void Circle(float x, float y, float r)
    {
        DrawWithFillAndStroke(paint => _surface!.Canvas.DrawCircle(x, y, r, paint));
    }

    public override void Line(float x1, float y1, float x2, float y2)
    {
        if (_surface == null) return;
        ApplyMatrix();
        using var paint = GetStrokePaint();
        _surface.Canvas.DrawLine(x1, y1, x2, y2, paint);
    }

    public override void Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        using var path = new SKPath();
        path.MoveTo(x1, y1);
        path.LineTo(x2, y2);
        path.LineTo(x3, y3);
        path.Close();
        DrawWithFillAndStroke(paint => _surface!.Canvas.DrawPath(path, paint));
    }

    // ── 文字 ──

    public override void TextSize(float size)
    {
        CurrentStyle.TextSize = size;
    }

    public override void Text(string str, float x, float y)
    {
        if (_surface == null) return;
        ApplyMatrix();
        using var font = new SKFont { Size = CurrentStyle.TextSize };
        if (CurrentStyle.Fill)
        {
            using var paint = GetFillPaint();
            _surface.Canvas.DrawText(str, x, y, CurrentStyle.TextAlign, font, paint);
        }
    }

    // ── 图像 ──

    public override void Image(SKBitmap bitmap, float x, float y)
    {
        if (_surface == null) return;
        ApplyMatrix();
        _surface.Canvas.DrawBitmap(bitmap, x, y);
    }

    // ── 输出到 SDL ──

    public override void Present(IntPtr sdlRenderer, IntPtr sdlTexture)
    {
        if (_surface == null) return;

        using var image = _surface.Snapshot();
        using var pixmap = image.PeekPixels();
        if (pixmap == null) return;

        IntPtr pixels = pixmap.GetPixels();
        int pitch = pixmap.RowBytes;

        SDL.SDL_Rect fullRect = new SDL.SDL_Rect { x = 0, y = 0, w = Width, h = Height };
        SDL.SDL_UpdateTexture(sdlTexture, ref fullRect, pixels, pitch);
        SDL.SDL_RenderCopy(sdlRenderer, sdlTexture, IntPtr.Zero, IntPtr.Zero);
        SDL.SDL_RenderPresent(sdlRenderer);
    }

    // ── 辅助方法 ──

    /// <summary>
    /// 统一执行 fill + stroke 双重绘制，消除所有形状方法中的重复代码。
    /// </summary>
    private void DrawWithFillAndStroke(Action<SKPaint> drawAction)
    {
        if (_surface == null) return;

        ApplyMatrix();

        if (CurrentStyle.Fill)
        {
            using var paint = GetFillPaint();
            drawAction(paint);
        }
        if (CurrentStyle.Stroke)
        {
            using var paint = GetStrokePaint();
            drawAction(paint);
        }
    }

    private void ApplyMatrix()
    {
        if (_surface == null) return;
        _surface.Canvas.SetMatrix(CurrentMatrix.ToSkMatrix());
    }

    private SKPaint GetFillPaint()
    {
        return new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = CurrentStyle.FillColor,
        };
    }

    private SKPaint GetStrokePaint()
    {
        return new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            Color = CurrentStyle.StrokeColor,
            StrokeWidth = CurrentStyle.StrokeWeight,
            StrokeCap = CurrentStyle.StrokeCap,
            StrokeJoin = CurrentStyle.StrokeJoin,
        };
    }
}
