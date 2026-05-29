using SpriteCore.Graphics;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// 基础示例：灰色背景上，红色圆跟随鼠标移动。
/// 对应 Lua 示例：01_HelloSprite
/// </summary>
public class HelloSketch : Sketch
{
    public override void Setup()
    {
        Size(800, 600);
        Background(220);
    }

    public override void Draw()
    {
        Background(220);
        Fill(255, 0, 0);
        NoStroke();
        Ellipse(MouseX, MouseY, 50, 50);
    }
}
