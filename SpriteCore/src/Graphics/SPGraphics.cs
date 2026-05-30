using SkiaSharp;

namespace SpriteCore.Graphics;

/// <summary>
/// 抽象图形渲染器。管理样式状态、变换矩阵，定义所有绘图 API 的接口。
/// 具体绘制由子类（如 SkiaGraphics）实现。
/// 对应 Processing 中的 PGraphics。
/// </summary>
public abstract class SPGraphics
{
    // ── 尺寸 ──
    public int Width { get; set; }
    public int Height { get; set; }

    // ── 样式状态 ──
    public SPStyle CurrentStyle { get; protected set; } = new();
    private readonly Stack<SPStyle> _styleStack = new();

    // ── 矩阵状态 ──
    protected SPMatrix2D CurrentMatrix { get; set; } = new();
    private readonly Stack<SPMatrix2D> _matrixStack = new();

    // ── 生命周期 ──
    public abstract void BeginFrame();
    public abstract void EndFrame();
    public abstract void Resize(int width, int height);

    // ── 颜色 ──
    public virtual void Background(float gray) => Background(gray, gray, gray, 255);
    public virtual void Background(float r, float g, float b) => Background(r, g, b, 255);
    public abstract void Background(float r, float g, float b, float a);

    // ── 样式设置 ──
    public virtual void Fill(float gray) => Fill(gray, gray, gray, 255);
    public virtual void Fill(float r, float g, float b, float a = 255)
    {
        CurrentStyle.Fill = true;
        CurrentStyle.FillColor = new SKColor((byte)r, (byte)g, (byte)b, (byte)a);
    }
    public virtual void NoFill() => CurrentStyle.Fill = false;

    public virtual void Stroke(float gray) => Stroke(gray, gray, gray, 255);
    public virtual void Stroke(float r, float g, float b, float a = 255)
    {
        CurrentStyle.Stroke = true;
        CurrentStyle.StrokeColor = new SKColor((byte)r, (byte)g, (byte)b, (byte)a);
    }
    public virtual void NoStroke() => CurrentStyle.Stroke = false;

    public virtual void StrokeWeight(float weight) => CurrentStyle.StrokeWeight = weight;
    public virtual void StrokeCap(SKStrokeCap cap) => CurrentStyle.StrokeCap = cap;
    public virtual void StrokeJoin(SKStrokeJoin join) => CurrentStyle.StrokeJoin = join;

    // ── 样式栈 ──
    public virtual void PushStyle() => _styleStack.Push(CurrentStyle.Clone());
    public virtual void PopStyle()
    {
        if (_styleStack.Count > 0)
            CurrentStyle = _styleStack.Pop();
    }

    // ── 矩阵 ──
    public virtual void PushMatrix() => _matrixStack.Push(CurrentMatrix.Clone());
    public virtual void PopMatrix()
    {
        if (_matrixStack.Count > 0)
            CurrentMatrix = _matrixStack.Pop();
    }
    public virtual void Translate(float x, float y) => CurrentMatrix.Translate(x, y);
    public virtual void Rotate(float angle) => CurrentMatrix.Rotate(angle);
    public virtual void Scale(float x, float y) => CurrentMatrix.Scale(x, y);

    // ── 抽象绘制方法 ──
    public abstract void Rect(float x, float y, float w, float h);
    public abstract void Ellipse(float x, float y, float w, float h);
    public abstract void Circle(float x, float y, float r);
    public abstract void Line(float x1, float y1, float x2, float y2);
    public abstract void Triangle(float x1, float y1, float x2, float y2, float x3, float y3);

    public abstract void TextSize(float size);
    public abstract void Text(string str, float x, float y);
    public abstract void Image(SKBitmap bitmap, float x, float y);
    public abstract void Image(SPTexture texture, float x, float y);
    public abstract void Image(SPTexture texture, float x, float y, float w, float h);

    // ── Tint（对应 Processing tint() / noTint()）──
    public virtual void Tint(float gray, float alpha = 255)
    {
        CurrentStyle.IsTinted = true;
        CurrentStyle.TintColor = new SKColor((byte)gray, (byte)gray, (byte)gray, (byte)alpha);
    }
    public virtual void Tint(float r, float g, float b, float a = 255)
    {
        CurrentStyle.IsTinted = true;
        CurrentStyle.TintColor = new SKColor((byte)r, (byte)g, (byte)b, (byte)a);
    }
    public virtual void NoTint() => CurrentStyle.IsTinted = false;

    // ── 抽象输出 ──
    public abstract void Present(IntPtr sdlRenderer, IntPtr sdlTexture);
}
