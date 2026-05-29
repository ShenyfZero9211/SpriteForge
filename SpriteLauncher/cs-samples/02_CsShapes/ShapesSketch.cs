using SpriteCore.Graphics;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// 形状演示：旋转的彩色矩形和椭圆组合。
/// 对应 Lua 示例：02_Shapes
/// </summary>
public class ShapesSketch : Sketch
{
    private float _angle = 0;

    public override void Setup()
    {
        Size(800, 600);
    }

    public override void Draw()
    {
        Background(20, 20, 30);

        // 中心旋转的矩形组
        PushMatrix();
        Translate(Width / 2, Height / 2);
        Rotate(_angle);

        for (int i = 0; i < 8; i++)
        {
            PushMatrix();
            Rotate(i * 45);
            Translate(120, 0);
            Fill(100 + i * 20, 200, 255 - i * 20);
            Rect(-20, -20, 40, 40);
            PopMatrix();
        }

        PopMatrix();

        // 中心圆
        Fill(255, 100, 100);
        Circle(Width / 2, Height / 2, 60);

        // 底部信息
        Fill(255);
        TextSize(16);
        Text($"Angle: {_angle:F1}", 20, Height - 30);

        _angle += 1.5f;
    }
}
