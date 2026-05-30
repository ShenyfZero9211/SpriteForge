using SkiaSharp;
using SpriteCore.Graphics;
using SpriteCore.Math;
using SpriteEngine.Scenes;
using SpriteEngine.UI;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// World 空间 UI 演示：角色在世界中移动，头顶跟随血条和名字标签。
/// 展示 Screen / World 双空间 UI 的协同工作。
/// </summary>
public class WorldSpaceUIDemo : Sketch
{
    private Scene _scene = null!;
    private GameObject _player = null!;
    private UIPanel _healthBarFg = null!;
    private float _time;
    private float _health = 100;

    public override void Setup()
    {
        Size(800, 600);

        _scene = new Scene("WorldSpaceUIDemo");

        // ── 创建角色 ──
        _player = _scene.CreateGameObject("Player");
        var renderer = _player.AddComponent<SpriteRenderer>();
        renderer.Color = new SKColor(100, 200, 100);
        renderer.Width = 40;
        renderer.Height = 40;

        // ── 角色头顶的 World 空间 UI ──
        var nameTagCanvas = _player.AddComponent<UICanvas>();
        nameTagCanvas.Space = CanvasSpace.World;
        nameTagCanvas.Width = 100;
        nameTagCanvas.Height = 40;
        nameTagCanvas.LocalX = -50; // 居中于角色
        nameTagCanvas.LocalY = -55; // 角色上方

        // 名字标签
        var nameLabel = new UILabel
        {
            Text = "Player",
            Width = 100,
            Height = 14,
            OverrideStyle = new UIStyle
            {
                FontSize = 12,
                TextColor = SKColors.White
            }
        };
        nameTagCanvas.AddChild(nameLabel);

        // 血条背景
        var healthBarBg = new UIPanel
        {
            Width = 60,
            Height = 6,
            LocalX = 20,
            LocalY = 18,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(60, 60, 60),
                CornerRadius = 3
            }
        };
        nameTagCanvas.AddChild(healthBarBg);

        // 血条前景（动态宽度）
        _healthBarFg = new UIPanel
        {
            Width = 60,
            Height = 6,
            LocalX = 20,
            LocalY = 18,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(220, 60, 60),
                CornerRadius = 3
            }
        };
        nameTagCanvas.AddChild(_healthBarFg);

        // 注册 Canvas 到场景 UIManager
        _scene.UIManager.RegisterCanvas(nameTagCanvas);

        // ── Screen 空间 HUD ──
        var hudCanvas = new UICanvas
        {
            Width = 800,
            Height = 600,
            Space = CanvasSpace.Screen,
            Padding = new Thickness(20),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 8,
                AlignItems = FlexAlign.Start
            }
        };

        var hudPanel = new UIPanel
        {
            Width = 240,
            Padding = new Thickness(12),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 6,
                AlignItems = FlexAlign.Start
            },
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(38, 38, 48),
                BorderColor = new SKColor(60, 60, 70),
                BorderThickness = 1,
                CornerRadius = 8,
                Padding = new Thickness(12)
            }
        };

        hudPanel.AddChild(new UILabel
        {
            Text = "World Space UI Demo",
            OverrideStyle = new UIStyle { FontSize = 16, TextColor = new SKColor(100, 200, 255) }
        });
        hudPanel.AddChild(new UILabel
        {
            Text = "Green square = Player (World space)",
            OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(180, 180, 190) }
        });
        hudPanel.AddChild(new UILabel
        {
            Text = "Red bar = Health (follows player)",
            OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(180, 180, 190) }
        });
        hudPanel.AddChild(new UILabel
        {
            Text = "ESC to exit",
            OverrideStyle = new UIStyle { FontSize = 11, TextColor = new SKColor(120, 120, 130) }
        });

        hudCanvas.AddChild(hudPanel);
        _scene.UIManager.RegisterCanvas(hudCanvas);
    }

    public override void Update(float dt)
    {
        _time += dt;
        _scene.Update(dt);

        if (SP5.Input != null)
            _scene.UIManager.ProcessInput(SP5.Input);

        // 角色做椭圆轨道运动
        _player.Transform.Position = new Vector2(
            Width / 2f + Cos(_time) * 250,
            Height / 2f + Sin(_time * 1.3f) * 180);
        _player.Transform.Rotation = Degrees(_time * 30);

        // 血条波动
        _health = 50 + Sin(_time * 2) * 45;
        _healthBarFg.Width = 60 * (_health / 100f);
        // 低血量变橙色
        _healthBarFg.OverrideStyle = new UIStyle
        {
            BackgroundColor = _health < 30
                ? new SKColor(255, 140, 0)
                : new SKColor(220, 60, 60),
            CornerRadius = 3
        };
    }

    public override void Draw()
    {
        Background(12, 12, 18);

        // 背景网格
        NoFill();
        Stroke(25, 25, 35);
        StrokeWeight(1);
        for (int x = 0; x < Width; x += 40)
            Line(x, 0, x, Height);
        for (int y = 0; y < Height; y += 40)
            Line(0, y, Width, y);

        // 场景渲染（包含 World 空间 UI）
        if (SP5.Graphics != null)
            _scene.Render(SP5.Graphics);
    }
}
