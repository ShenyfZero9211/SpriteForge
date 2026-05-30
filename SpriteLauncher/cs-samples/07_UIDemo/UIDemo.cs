using SkiaSharp;
using SpriteCore.Graphics;
using SpriteCore.Math;
using SpriteEngine.Scenes;
using SpriteEngine.UI;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// UI 系统效果演示：
/// - Screen 空间 HUD：标题栏 + 信息面板
/// - FlexLayout Row/Column 布局
/// - UILabel 动态文本更新（FPS、鼠标坐标、时间）
/// - 背景网格 + 动画装饰，展示 UI 覆盖在游戏内容之上
/// </summary>
public class UIDemo : Sketch
{
    private Scene _scene = null!;
    private UILabel _fpsLabel = null!;
    private UILabel _mouseLabel = null!;
    private UILabel _timeLabel = null!;
    private UILabel _clickCountLabel = null!;
    private float _elapsed;
    private int _clickCount;

    public override void Setup()
    {
        Size(800, 600);

        _scene = new Scene("UIDemo");

        // ── 主 HUD Canvas（Screen 空间）──
        var hudCanvas = new UICanvas
        {
            Width = 800,
            Height = 600,
            Space = CanvasSpace.Screen,
            Padding = new Thickness(20),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 16,
                AlignItems = FlexAlign.Start
            }
        };

        // ── 标题栏（水平布局，带背景）──
        var titleBar = new UIPanel
        {
            Width = 760,
            Height = 56,
            Padding = new Thickness(16, 12, 16, 12),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Gap = 12,
                AlignItems = FlexAlign.Center,
                JustifyContent = FlexJustify.SpaceBetween
            },
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(45, 45, 58),
                BorderColor = new SKColor(70, 70, 85),
                BorderThickness = 1,
                CornerRadius = 10,
                Padding = new Thickness(16, 12, 16, 12)
            }
        };

        var titleLabel = new UILabel
        {
            Text = "SpriteForge UI Demo",
            OverrideStyle = new UIStyle { FontSize = 22, TextColor = new SKColor(100, 200, 255) }
        };

        var versionLabel = new UILabel
        {
            Text = "Phase B",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(140, 140, 150) }
        };

        titleBar.AddChild(titleLabel);
        titleBar.AddChild(versionLabel);
        hudCanvas.AddChild(titleBar);

        // ── 内容区：左右分栏 ──
        var contentRow = new UIContainer
        {
            Width = 760,
            Height = 480,
            Padding = new Thickness(0),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Gap = 16,
                AlignItems = FlexAlign.Start
            }
        };

        // 左侧信息面板（垂直布局，带背景）
        var infoPanel = new UIPanel
        {
            Width = 280,
            Padding = new Thickness(16),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 10,
                AlignItems = FlexAlign.Start
            },
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(38, 38, 48),
                BorderColor = new SKColor(60, 60, 70),
                BorderThickness = 1,
                CornerRadius = 10,
                Padding = new Thickness(16)
            }
        };

        var headerLabel = new UILabel
        {
            Text = "System Info",
            OverrideStyle = new UIStyle { FontSize = 16, TextColor = new SKColor(200, 200, 210) }
        };

        _fpsLabel = new UILabel
        {
            Text = "FPS: --",
            OverrideStyle = new UIStyle { FontSize = 14, TextColor = new SKColor(100, 255, 150) }
        };

        _mouseLabel = new UILabel
        {
            Text = "Mouse: --, --",
            OverrideStyle = new UIStyle { FontSize = 14, TextColor = new SKColor(255, 200, 100) }
        };

        _timeLabel = new UILabel
        {
            Text = "Time: 0.0s",
            OverrideStyle = new UIStyle { FontSize = 14, TextColor = new SKColor(180, 140, 255) }
        };

        var layoutLabel = new UILabel
        {
            Text = "Layout: Flex Row + Column",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(160, 160, 170) }
        };

        var hintLabel = new UILabel
        {
            Text = "ESC to exit",
            OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(120, 120, 130) }
        };

        // 交互按钮
        var clickButton = new UIButton
        {
            Text = "Click Me!",
            Width = 120,
            Height = 36,
            OverrideStyle = new UIStyle
            {
                FontSize = 14,
                BackgroundColor = new SKColor(60, 120, 200),
                HoverColor = new SKColor(80, 150, 240),
                PressedColor = new SKColor(40, 90, 160),
                TextColor = new SKColor(255, 255, 255),
                CornerRadius = 6,
                Padding = new Thickness(12, 6, 12, 6)
            }
        };
        clickButton.OnClick += () =>
        {
            _clickCount++;
            _clickCountLabel.Text = $"Clicks: {_clickCount}";
        };

        _clickCountLabel = new UILabel
        {
            Text = "Clicks: 0",
            OverrideStyle = new UIStyle { FontSize = 14, TextColor = new SKColor(255, 150, 150) }
        };

        infoPanel.AddChild(headerLabel);
        infoPanel.AddChild(_fpsLabel);
        infoPanel.AddChild(_mouseLabel);
        infoPanel.AddChild(_timeLabel);
        infoPanel.AddChild(_clickCountLabel);
        infoPanel.AddChild(layoutLabel);
        infoPanel.AddChild(clickButton);
        infoPanel.AddChild(hintLabel);
        contentRow.AddChild(infoPanel);

        // 右侧说明面板
        var descPanel = new SpriteEngine.UI.UIPanel
        {
            Width = 464,
            Padding = new Thickness(16),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 8,
                AlignItems = FlexAlign.Start
            },
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(38, 38, 48),
                BorderColor = new SKColor(60, 60, 70),
                BorderThickness = 1,
                CornerRadius = 10,
                Padding = new Thickness(16)
            }
        };

        var descTitle = new UILabel
        {
            Text = "Architecture",
            OverrideStyle = new UIStyle { FontSize = 16, TextColor = new SKColor(200, 200, 210) }
        };

        var desc1 = new UILabel
        {
            Text = "- Retained UI Tree (Component-based)",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        };
        var desc2 = new UILabel
        {
            Text = "- Flexbox Layout Engine",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        };
        var desc3 = new UILabel
        {
            Text = "- Screen / World Dual Space",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        };
        var desc4 = new UILabel
        {
            Text = "- Event Bubbling + Focus Manager",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        };
        var desc5 = new UILabel
        {
            Text = "- UIDrawList Batch Rendering",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        };

        descPanel.AddChild(descTitle);
        descPanel.AddChild(desc1);
        descPanel.AddChild(desc2);
        descPanel.AddChild(desc3);
        descPanel.AddChild(desc4);
        descPanel.AddChild(desc5);
        contentRow.AddChild(descPanel);

        hudCanvas.AddChild(contentRow);

        // 注册 Canvas
        _scene.UIManager.RegisterCanvas(hudCanvas);
    }

    public override void Update(float dt)
    {
        _elapsed += dt;
        _scene.Update(dt);

        // 输入分发
        if (SP5.Input != null)
            _scene.UIManager.ProcessInput(SP5.Input);

        // 更新动态标签
        int fps = (int)MathUtils.Round(1f / Math.Max(dt, 0.0001f));
        _fpsLabel.Text = $"FPS: {fps}";
        _mouseLabel.Text = $"Mouse: {MathUtils.Round(MouseX)}, {MathUtils.Round(MouseY)}";
        _timeLabel.Text = $"Time: {_elapsed:F1}s";
    }

    public override void Draw()
    {
        Background(12, 12, 18);

        // 背景网格装饰
        NoFill();
        Stroke(25, 25, 35);
        StrokeWeight(1);
        for (int x = 0; x < Width; x += 40)
            Line(x, 0, x, Height);
        for (int y = 0; y < Height; y += 40)
            Line(0, y, Width, y);

        // 中心装饰圆（展示 UI 覆盖在游戏内容上）
        NoFill();
        Stroke(60, 60, 80);
        StrokeWeight(2);
        float t = _elapsed * 0.5f;
        float cx = Width / 2f;
        float cy = Height / 2f;
        Ellipse(cx + Cos(t) * 100, cy + Sin(t) * 80, 120, 120);
        Ellipse(cx + Cos(t + 2) * 120, cy + Sin(t + 2) * 100, 80, 80);

        // 渲染场景（包含 Screen 空间 UI）
        if (SP5.Graphics != null)
            _scene.Render(SP5.Graphics);
    }

}
