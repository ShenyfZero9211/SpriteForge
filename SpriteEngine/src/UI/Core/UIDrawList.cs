using SkiaSharp;
using SpriteCore.Graphics;

namespace SpriteEngine.UI;

/// <summary>
/// UI 渲染原语缓冲。Widgets 将绘制命令写入此列表，由 UICanvas 统一输出到 SPGraphics。
/// 受 Dear ImGui DrawList 启发。
/// </summary>
public class UIDrawList
{
    private readonly List<DrawCommand> _commands = new();
    private readonly Stack<SKRect> _clipStack = new();

    private SKRect CurrentClip => _clipStack.Count > 0 ? _clipStack.Peek() : SKRect.Create(-99999, -99999, 199998, 199998);

    // ── 原语添加 ──

    public void AddRectFilled(float x, float y, float w, float h, SKColor color, float cornerRadius = 0)
    {
        _commands.Add(new DrawRectFilledCmd(x, y, w, h, color, cornerRadius, CurrentClip));
    }

    public void AddRect(float x, float y, float w, float h, SKColor color, float thickness, float cornerRadius = 0)
    {
        _commands.Add(new DrawRectCmd(x, y, w, h, color, thickness, cornerRadius, CurrentClip));
    }

    public void AddText(string text, float x, float y, SKColor color, float fontSize)
    {
        _commands.Add(new DrawTextCmd(text, x, y, color, fontSize, CurrentClip));
    }

    public void AddImage(SPTexture texture, float x, float y, float w, float h, SKColor tint)
    {
        _commands.Add(new DrawImageCmd(texture, x, y, w, h, tint, CurrentClip));
    }

    public void AddLine(float x1, float y1, float x2, float y2, SKColor color, float thickness)
    {
        _commands.Add(new DrawLineCmd(x1, y1, x2, y2, color, thickness, CurrentClip));
    }

    // ── 裁剪 ──

    public void PushClipRect(float x, float y, float w, float h)
    {
        var newClip = SKRect.Create(x, y, w, h);
        if (_clipStack.Count > 0)
        {
            var current = _clipStack.Peek();
            newClip = SKRect.Intersect(current, newClip);
            if (newClip.IsEmpty) newClip = SKRect.Empty;
        }
        _clipStack.Push(newClip);
    }

    public void PopClipRect()
    {
        if (_clipStack.Count > 0) _clipStack.Pop();
    }

    // ── 输出 ──

    public void Clear()
    {
        _commands.Clear();
        _clipStack.Clear();
    }

    public void Render(SPGraphics g)
    {
        foreach (var cmd in _commands)
        {
            cmd.Execute(g);
        }
    }

    // ── 内部命令类型 ──

    private abstract class DrawCommand
    {
        public SKRect Clip { get; }
        protected DrawCommand(SKRect clip) => Clip = clip;
        public abstract void Execute(SPGraphics g);
    }

    private class DrawRectFilledCmd : DrawCommand
    {
        public float X, Y, W, H;
        public SKColor Color;
        public float CornerRadius;
        public DrawRectFilledCmd(float x, float y, float w, float h, SKColor color, float r, SKRect clip) : base(clip)
            { X = x; Y = y; W = w; H = h; Color = color; CornerRadius = r; }
        public override void Execute(SPGraphics g)
        {
            g.Fill(Color.Red, Color.Green, Color.Blue, Color.Alpha);
            if (CornerRadius > 0)
                g.RoundRect(X, Y, W, H, CornerRadius);
            else
                g.Rect(X, Y, W, H);
        }
    }

    private class DrawRectCmd : DrawCommand
    {
        public float X, Y, W, H;
        public SKColor Color;
        public float Thickness, CornerRadius;
        public DrawRectCmd(float x, float y, float w, float h, SKColor color, float t, float r, SKRect clip) : base(clip)
            { X = x; Y = y; W = w; H = h; Color = color; Thickness = t; CornerRadius = r; }
        public override void Execute(SPGraphics g)
        {
            g.Stroke(Color.Red, Color.Green, Color.Blue, Color.Alpha);
            g.StrokeWeight(Thickness);
            g.Rect(X, Y, W, H);
        }
    }

    private class DrawTextCmd : DrawCommand
    {
        public string Text;
        public float X, Y;
        public SKColor Color;
        public float FontSize;
        public DrawTextCmd(string text, float x, float y, SKColor color, float fontSize, SKRect clip) : base(clip)
            { Text = text; X = x; Y = y; Color = color; FontSize = fontSize; }
        public override void Execute(SPGraphics g)
        {
            g.Fill(Color.Red, Color.Green, Color.Blue, Color.Alpha);
            g.TextSize(FontSize);
            g.Text(Text, X, Y);
        }
    }

    private class DrawImageCmd : DrawCommand
    {
        public SPTexture Texture;
        public float X, Y, W, H;
        public SKColor Tint;
        public DrawImageCmd(SPTexture texture, float x, float y, float w, float h, SKColor tint, SKRect clip) : base(clip)
            { Texture = texture; X = x; Y = y; W = w; H = h; Tint = tint; }
        public override void Execute(SPGraphics g)
        {
            if (Tint.Alpha > 0)
                g.Tint(Tint.Red, Tint.Green, Tint.Blue, Tint.Alpha);
            g.Image(Texture, X, Y, W, H);
            if (Tint.Alpha > 0)
                g.NoTint();
        }
    }

    private class DrawLineCmd : DrawCommand
    {
        public float X1, Y1, X2, Y2;
        public SKColor Color;
        public float Thickness;
        public DrawLineCmd(float x1, float y1, float x2, float y2, SKColor color, float t, SKRect clip) : base(clip)
            { X1 = x1; Y1 = y1; X2 = x2; Y2 = y2; Color = color; Thickness = t; }
        public override void Execute(SPGraphics g)
        {
            g.Stroke(Color.Red, Color.Green, Color.Blue, Color.Alpha);
            g.StrokeWeight(Thickness);
            g.Line(X1, Y1, X2, Y2);
        }
    }
}
