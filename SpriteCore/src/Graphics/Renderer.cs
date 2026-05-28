using SkiaSharp;
using SDL2;

namespace SpriteCore.Graphics;

public class Renderer : IDisposable
{
    private SKSurface? _surface;
    private SKImageInfo _imageInfo;
    private int _width;
    private int _height;

    public SKCanvas? Canvas => _surface?.Canvas;
    public int Width => _width;
    public int Height => _height;

    public void Initialize(int width, int height)
    {
        _width = width;
        _height = height;
        _imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _surface = SKSurface.Create(_imageInfo);
    }

    public void Resize(int width, int height)
    {
        _surface?.Dispose();
        _width = width;
        _height = height;
        _imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _surface = SKSurface.Create(_imageInfo);
    }

    public void BeginFrame()
    {
        if (_surface == null) return;
        _surface.Canvas.Clear(SKColors.Black);
    }

    public void EndFrame()
    {
        _surface?.Canvas.Flush();
    }

    public void PresentToSdlRenderer(IntPtr sdlRenderer, IntPtr sdlTexture)
    {
        if (_surface == null) return;

        using var image = _surface.Snapshot();
        using var pixmap = image.PeekPixels();
        if (pixmap == null) return;

        IntPtr pixels = pixmap.GetPixels();
        int pitch = pixmap.RowBytes;

        SDL.SDL_Rect fullRect = new SDL.SDL_Rect { x = 0, y = 0, w = _width, h = _height };
        SDL.SDL_UpdateTexture(sdlTexture, ref fullRect, pixels, pitch);
        SDL.SDL_RenderCopy(sdlRenderer, sdlTexture, IntPtr.Zero, IntPtr.Zero);
        SDL.SDL_RenderPresent(sdlRenderer);
    }

    public void Dispose()
    {
        _surface?.Dispose();
    }
}
