using SkiaSharp;
using SpriteCore.Graphics;

namespace SpriteEngine.UI;

/// <summary>
/// 图片显示 Widget。支持 SPTexture 和可选的 Tint 着色。
/// </summary>
public class UIImage : UIElement
{
    public SPTexture? Texture { get; set; }

    /// <summary>图片填充模式</summary>
    public ImageFitMode FitMode { get; set; } = ImageFitMode.Stretch;

    /// <summary>着色（Alpha=0 时不着色）</summary>
    public SKColor Tint { get; set; } = new SKColor(0, 0, 0, 0);

    public override void Measure()
    {
        if (Texture?.Bitmap != null)
        {
            DesiredWidth = Texture.Bitmap.Width;
            DesiredHeight = Texture.Bitmap.Height;
        }
        else
        {
            DesiredWidth = Width;
            DesiredHeight = Height;
        }
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        if (Texture?.Bitmap == null) return;

        float x = AbsoluteX;
        float y = AbsoluteY;
        float w = Width;
        float h = Height;

        if (FitMode == ImageFitMode.Original && Width > 0 && Height > 0)
        {
            // 保持原始尺寸，居中显示
            float texW = Texture.Bitmap.Width;
            float texH = Texture.Bitmap.Height;
            x += (Width - texW) / 2;
            y += (Height - texH) / 2;
            w = texW;
            h = texH;
        }
        else if (FitMode == ImageFitMode.Contain && Width > 0 && Height > 0)
        {
            // 等比缩放，完整显示在区域内
            float texW = Texture.Bitmap.Width;
            float texH = Texture.Bitmap.Height;
            float scale = Math.Min(Width / texW, Height / texH);
            w = texW * scale;
            h = texH * scale;
            x += (Width - w) / 2;
            y += (Height - h) / 2;
        }

        drawList.AddImage(Texture, x, y, w, h, Tint);
    }
}

public enum ImageFitMode
{
    /// <summary>拉伸填满整个区域</summary>
    Stretch,
    /// <summary>保持原始尺寸，居中</summary>
    Original,
    /// <summary>等比缩放，完整显示</summary>
    Contain,
}
