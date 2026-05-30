using SpriteCore.Graphics;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// 3×3 纹理网格示例：
/// 默认情况下所有图片以 50% 透明度显示，鼠标悬停时恢复 100% 透明度。
/// 展示 Tint() / NoTint() 与 Image() 的协同使用。
/// </summary>
public class TextureGrid : Sketch
{
    private readonly List<SPTexture> _textures = new();
    private const int Cols = 3;
    private const int Rows = 3;
    private const float CellW = 200;
    private const float CellH = 150;
    private const float Gap = 20;
    private float _startX;
    private float _startY;

    public override void Setup()
    {
        Size(800, 600);

        // 扫描可用纹理
        var texDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "textures", "ruins");
        if (Directory.Exists(texDir))
        {
            foreach (var f in Directory.GetFiles(texDir, "*.png").Take(Cols * Rows))
                _textures.Add(LoadImage(f));
        }

        // 若不足 9 张则循环复用
        int originalCount = _textures.Count;
        for (int i = _textures.Count; i < Cols * Rows && originalCount > 0; i++)
            _textures.Add(_textures[i % originalCount]);

        // 网格居中
        float totalW = Cols * CellW + (Cols - 1) * Gap;
        float totalH = Rows * CellH + (Rows - 1) * Gap;
        _startX = (Width - totalW) / 2;
        _startY = (Height - totalH) / 2;
    }

    public override void Draw()
    {
        Background(5, 10, 40);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                int idx = row * Cols + col;
                if (idx >= _textures.Count) break;

                float x = _startX + col * (CellW + Gap);
                float y = _startY + row * (CellH + Gap);

                bool hovered = MouseX >= x && MouseX <= x + CellW
                            && MouseY >= y && MouseY <= y + CellH;

                if (hovered)
                    NoTint();
                else
                    Tint(255, 128); // 50% 透明

                Image(_textures[idx], x, y, CellW, CellH);

                // 悬停高亮边框
                if (hovered)
                {
                    NoTint();
                    NoFill();
                    Stroke(255);
                    StrokeWeight(3);
                    Rect(x, y, CellW, CellH);
                }
            }
        }

        // HUD
        NoTint();
        Fill(200);
        TextSize(14);
        Text($"Textures: {_textures.Count} | Hover to reveal", 20, 30);
    }
}
