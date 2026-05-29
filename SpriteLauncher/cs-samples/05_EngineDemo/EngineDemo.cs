using SkiaSharp;
using SpriteCore.Graphics;
using SpriteCore.Math;
using SpriteEngine.Scenes;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// SpriteEngine 渲染验证 Demo：
/// 三个彩色矩形围绕中心旋转，展示 GameObject + Transform + SpriteRenderer 的完整工作流。
/// </summary>
public class EngineDemo : Sketch
{
    private Scene _scene = null!;
    private float _time;

    public override void Setup()
    {
        Size(800, 600);

        _scene = new Scene("EngineDemo");

        // 中心红色方块
        var center = _scene.CreateGameObject("Center");
        center.Transform.Position = new Vector2(Width / 2, Height / 2);
        var r1 = center.AddComponent<SpriteRenderer>();
        r1.Color = new SKColor(255, 80, 80);
        r1.Width = 60;
        r1.Height = 60;
        r1.DrawStroke = true;
        r1.StrokeColor = SKColors.White;
        r1.StrokeWeight = 2;

        // 轨道绿色方块
        var green = _scene.CreateGameObject("Green");
        var r2 = green.AddComponent<SpriteRenderer>();
        r2.Color = new SKColor(80, 255, 120);
        r2.Width = 40;
        r2.Height = 40;

        // 轨道蓝色方块（父级为绿色，形成层级）
        var blue = _scene.CreateGameObject("Blue");
        blue.SetParent(green);
        var r3 = blue.AddComponent<SpriteRenderer>();
        r3.Color = new SKColor(80, 160, 255);
        r3.Width = 30;
        r3.Height = 30;
    }

    public override void Update(float dt)
    {
        _time += dt;

        // 中心方块缓慢自转
        var center = _scene.Find("Center")!;
        center.Transform.Rotation = _time * 30;

        // 绿色方块围绕中心做圆周运动
        var green = _scene.Find("Green")!;
        float angle1 = _time * 1.5f;
        green.Transform.Position = new Vector2(
            Width / 2 + MathUtils.Cos(angle1) * 180,
            Height / 2 + MathUtils.Sin(angle1) * 180);
        green.Transform.Rotation = angle1 * 60;

        // 蓝色方块围绕绿色方块运动
        var blue = _scene.Find("Blue")!;
        float angle2 = _time * 2.5f;
        // 相对于绿色方块的本地位置
        blue.Transform.LocalPosition = new Vector2(
            MathUtils.Cos(angle2) * 60,
            MathUtils.Sin(angle2) * 60);
        blue.Transform.Rotation = -angle2 * 90;

        _scene.Update(dt);
    }

    public override void Draw()
    {
        Background(15, 15, 25);

        // 轨道轨迹线（装饰）
        NoFill();
        Stroke(50, 50, 70);
        StrokeWeight(1);
        Ellipse(Width / 2, Height / 2, 360, 360);

        // 使用 SpriteEngine 渲染所有 SpriteRenderer
        if (SP5.Graphics != null)
            _scene.Render(SP5.Graphics);

        // HUD
        Fill(200);
        TextSize(14);
        Text($"Objects: {_scene.AllObjects.Count()}", 20, 30);
        Text($"FPS: {MathUtils.Round(1 / DeltaTime)}", 20, 50);
    }
}
