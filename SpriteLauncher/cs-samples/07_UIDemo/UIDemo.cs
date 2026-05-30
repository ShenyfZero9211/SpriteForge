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
    private UILabel _sliderValueLabel = null!;
    private UILabel _checkStateLabel = null!;
    private UILabel _inputValueLabel = null!;
    private float _elapsed;
    private int _clickCount;

    public override void Setup()
    {
        Size(1200, 800);

        _scene = new Scene("UIDemo");

        // ── 主 HUD Canvas（Screen 空间）──
        var hudCanvas = new UICanvas
        {
            Width = 1200,
            Height = 800,
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
            Width = 1160,
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
            Text = "Phase C",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(140, 140, 150) }
        };

        titleBar.AddChild(titleLabel);
        titleBar.AddChild(versionLabel);
        hudCanvas.AddChild(titleBar);

        // ── 内容区：左右分栏 ──
        var contentRow = new UIContainer
        {
            Width = 1160,
            Height = 700,
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
            Text = "ESC to exit  |  Tab to navigate",
            OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(120, 120, 130) }
        };

        // ── UISlider 演示 ──
        var sliderLabel = new UILabel
        {
            Text = "Volume:",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(200, 200, 210) }
        };

        _sliderValueLabel = new UILabel
        {
            Text = "50",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(100, 255, 150) }
        };

        var slider = new UISlider
        {
            Width = 260,
            Height = 24,
            Value = 50,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(60, 60, 70),
                ForegroundColor = new SKColor(100, 200, 255),
                HoverColor = new SKColor(130, 220, 255),
                CornerRadius = 4
            }
        };
        slider.OnValueChanged += v =>
        {
            _sliderValueLabel.Text = $"{v:F0}";
        };

        // ── UICheckbox 演示 ──
        var checkbox = new UICheckbox
        {
            Text = "Show Decorations",
            BoxSize = 16,
            IsChecked = true,
            OverrideStyle = new UIStyle
            {
                FontSize = 13,
                TextColor = new SKColor(200, 200, 210),
                ForegroundColor = new SKColor(100, 255, 150)
            }
        };
        _checkStateLabel = new UILabel
        {
            Text = "On",
            OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(100, 255, 150) }
        };
        checkbox.OnValueChanged += checkedState =>
        {
            _checkStateLabel.Text = checkedState ? "On" : "Off";
        };

        // ── UITextInput 演示 ──
        var inputLabel = new UILabel
        {
            Text = "Name:",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(200, 200, 210) }
        };

        var textInput = new UITextInput
        {
            Width = 180,
            Height = 30,
            Placeholder = "Type here...",
            OverrideStyle = new UIStyle
            {
                FontSize = 14,
                BackgroundColor = new SKColor(30, 30, 38),
                BorderColor = new SKColor(70, 70, 80),
                BorderThickness = 1,
                TextColor = new SKColor(220, 220, 230),
                CornerRadius = 4,
                Padding = new Thickness(8, 4, 8, 4)
            }
        };
        _inputValueLabel = new UILabel
        {
            Text = "",
            OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(160, 160, 170) }
        };
        textInput.OnSubmit += text =>
        {
            _inputValueLabel.Text = $"Submitted: {text}";
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

        // 右侧控件面板
        var ctrlPanel = new UIPanel
        {
            Width = 340,
            Padding = new Thickness(16),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 12,
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

        var ctrlTitle = new UILabel
        {
            Text = "Interactive Widgets",
            OverrideStyle = new UIStyle { FontSize = 16, TextColor = new SKColor(200, 200, 210) }
        };

        ctrlPanel.AddChild(ctrlTitle);
        ctrlPanel.AddChild(sliderLabel);

        var sliderRow = new UIContainer
        {
            Width = 248,
            Height = 24,
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Gap = 8,
                AlignItems = FlexAlign.Center
            }
        };
        sliderRow.AddChild(slider);
        sliderRow.AddChild(_sliderValueLabel);
        ctrlPanel.AddChild(sliderRow);

        var checkRow = new UIContainer
        {
            Width = 248,
            Height = 20,
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Gap = 8,
                AlignItems = FlexAlign.Center
            }
        };
        checkRow.AddChild(checkbox);
        checkRow.AddChild(_checkStateLabel);
        ctrlPanel.AddChild(checkRow);

        ctrlPanel.AddChild(inputLabel);
        ctrlPanel.AddChild(textInput);
        ctrlPanel.AddChild(_inputValueLabel);

        contentRow.AddChild(infoPanel);
        contentRow.AddChild(ctrlPanel);

        // ── 右侧 TabPane 展示新组件 ──
        var tabPane = new UITabPane
        {
            Width = 440,
            Height = 700,
            TabHeight = 36,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(38, 38, 48),
                BorderColor = new SKColor(60, 60, 70),
                BorderThickness = 1,
                CornerRadius = 10,
                Padding = new Thickness(12)
            }
        };

        // Tab 1: Info（原来的说明文字）
        var infoContent = tabPane.AddTab("Info");
        infoContent.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Column,
            Gap = 8,
            AlignItems = FlexAlign.Start
        };
        infoContent.AddChild(new UILabel
        {
            Text = "Architecture",
            OverrideStyle = new UIStyle { FontSize = 16, TextColor = new SKColor(200, 200, 210) }
        });
        infoContent.AddChild(new UILabel
        {
            Text = "- Retained UI Tree (Component-based)",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        });
        infoContent.AddChild(new UILabel
        {
            Text = "- Flexbox + Anchor + Absolute Layout",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        });
        infoContent.AddChild(new UILabel
        {
            Text = "- Screen / World Dual Space",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        });
        infoContent.AddChild(new UILabel
        {
            Text = "- Event Bubbling + Focus Manager",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(170, 170, 180) }
        });
        infoContent.AddChild(new UILabel
        {
            Text = "- 12 Widgets, 305 Tests",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(100, 200, 255) }
        });

        // Tab 2: Widgets（展示新组件）
        var widgetsContent = tabPane.AddTab("Widgets");
        widgetsContent.LayoutEngine = new FlexLayout
        {
            Direction = FlexDirection.Column,
            Gap = 12,
            AlignItems = FlexAlign.Start
        };

        // ProgressBar
        var progressLabel = new UILabel
        {
            Text = "Loading:",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(200, 200, 210) }
        };
        var progressBar = new UIProgressBar
        {
            Width = 400,
            Height = 8,
            Value = 0.65f,
            TrackRadius = 4,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(50, 50, 60),
                ForegroundColor = new SKColor(100, 200, 255)
            }
        };

        // Dropdown
        var dropdownLabel = new UILabel
        {
            Text = "Theme:",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(200, 200, 210) }
        };
        var dropdown = new UIDropdown
        {
            Width = 260,
            Height = 32,
            ItemHeight = 28,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(45, 45, 55),
                BorderColor = new SKColor(70, 70, 80),
                BorderThickness = 1,
                TextColor = new SKColor(220, 220, 230),
                CornerRadius = 4,
                Padding = new Thickness(8, 4, 8, 4)
            }
        };
        dropdown.SetItems(new[] { "Dark", "Light", "High Contrast" });
        dropdown.SelectedIndex = 0;

        // List
        var listLabel = new UILabel
        {
            Text = "Layers:",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(200, 200, 210) }
        };
        var list = new UIList
        {
            Width = 260,
            Height = 140,
            ItemHeight = 28,
            SelectedColor = new SKColor(60, 100, 160),
            OverrideStyle = new UIStyle
            {
                TextColor = new SKColor(200, 200, 210),
                Padding = new Thickness(8, 0, 8, 0)
            }
        };
        list.SetItems(new[] { "Background", "Midground", "Foreground", "UI", "Effects" });
        list.SelectedIndex = 2;

        // ScrollView（包含一个长列表）
        var scrollLabel = new UILabel
        {
            Text = "Log (scrollable):",
            OverrideStyle = new UIStyle { FontSize = 13, TextColor = new SKColor(200, 200, 210) }
        };
        var scrollView = new UIScrollView
        {
            Width = 400,
            Height = 100,
            ShowHorizontalScrollbar = false,
            OverrideStyle = new UIStyle
            {
                BackgroundColor = new SKColor(28, 28, 35),
                BorderColor = new SKColor(50, 50, 60),
                BorderThickness = 1,
                CornerRadius = 4,
                Padding = new Thickness(8)
            }
        };
        for (int i = 1; i <= 10; i++)
        {
            scrollView.Content.AddChild(new UILabel
            {
                Text = $"[{i:D2}] System initialized...",
                OverrideStyle = new UIStyle { FontSize = 12, TextColor = new SKColor(140, 140, 150) }
            });
        }

        widgetsContent.AddChild(progressLabel);
        widgetsContent.AddChild(progressBar);
        widgetsContent.AddChild(dropdownLabel);
        widgetsContent.AddChild(dropdown);
        widgetsContent.AddChild(listLabel);
        widgetsContent.AddChild(list);
        widgetsContent.AddChild(scrollLabel);
        widgetsContent.AddChild(scrollView);

        contentRow.AddChild(tabPane);

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
        {
            _scene.UIManager.ProcessInput(SP5.Input);

            // 键盘事件分发
            foreach (var key in SP5.Input.KeysDown)
            {
                _scene.UIManager.ProcessKeyEvent(
                    UIEvent.KeyPressed('\0', (int)key));
            }
            foreach (var ch in SP5.Input.TextInputQueue)
            {
                _scene.UIManager.ProcessKeyEvent(
                    UIEvent.KeyTyped(ch));
            }
        }

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
