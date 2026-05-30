using SkiaSharp;
using SpriteCore.Graphics;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

/// <summary>
/// 集成测试：验证 FlexLayout + UICanvas + UILabel 的完整渲染流程。
/// 验收标准：能创建一个带 FlexLayout 的 UICanvas，里面放两个 UILabel，
/// 正确渲染在屏幕上。
/// </summary>
public class UIIntegrationTests
{
    [Fact]
    public void FlexCanvas_WithTwoLabels_GeneratesDrawCommands()
    {
        // ── 1. 构建 UI 树 ──
        var canvas = new UICanvas
        {
            Width = 800,
            Height = 600,
            Space = CanvasSpace.Screen,
            Padding = new Thickness(20),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Gap = 16,
                AlignItems = FlexAlign.Center
            }
        };

        var label1 = new UILabel
        {
            Text = "Hello",
            OverrideStyle = new UIStyle { FontSize = 24, TextColor = new SKColor(255, 255, 255) }
        };

        var label2 = new UILabel
        {
            Text = "World",
            OverrideStyle = new UIStyle { FontSize = 24, TextColor = new SKColor(200, 200, 200) }
        };

        canvas.AddChild(label1);
        canvas.AddChild(label2);

        // ── 2. 模拟 UIManager.Update 流程（Measure + Layout）──
        var mgr = new UIManager();
        mgr.RegisterCanvas(canvas);
        mgr.Update(0.016f);

        // ── 3. 验证 Measure 结果 ──
        Assert.True(label1.DesiredWidth > 0, "label1 should have measured width");
        Assert.True(label1.DesiredHeight > 0, "label1 should have measured height");
        Assert.True(label2.DesiredWidth > 0, "label2 should have measured width");
        Assert.True(label2.DesiredHeight > 0, "label2 should have measured height");

        // ── 4. 验证 Layout 结果 ──
        // Row 方向：label1 在左，label2 在右
        Assert.Equal(20, label1.LocalX); // Padding.Left
        Assert.True(label2.LocalX > label1.LocalX, "label2 should be to the right of label1");

        // ── 5. 验证 BuildDrawList + Render 生成正确的绘制调用 ──
        var mockGfx = new MockGraphics();
        mgr.Render(mockGfx);

        Assert.True(mockGfx.TextCalls.Count >= 2, "should have at least 2 text draw calls");
        Assert.Contains(mockGfx.TextCalls, c => c.text == "Hello");
        Assert.Contains(mockGfx.TextCalls, c => c.text == "World");
    }

    [Fact]
    public void FlexCanvas_ColumnLayout_LabelsStackVertically()
    {
        var canvas = new UICanvas
        {
            Width = 400,
            Height = 600,
            Padding = new Thickness(10),
            LayoutEngine = new FlexLayout
            {
                Direction = FlexDirection.Column,
                Gap = 8
            }
        };

        var label1 = new UILabel { Text = "Top", OverrideStyle = new UIStyle { FontSize = 16 } };
        var label2 = new UILabel { Text = "Bottom", OverrideStyle = new UIStyle { FontSize = 16 } };
        canvas.AddChild(label1);
        canvas.AddChild(label2);

        var mgr = new UIManager();
        mgr.RegisterCanvas(canvas);
        mgr.Update(0.016f);

        // Column 方向：label2 在 label1 下方
        Assert.True(label2.LocalY > label1.LocalY, "label2 should be below label1");
    }

    [Fact]
    public void Canvas_Render_ScreenSpace_ProducesDrawCalls()
    {
        var canvas = new UICanvas
        {
            Width = 800,
            Height = 600,
            LayoutEngine = new FlexLayout { Direction = FlexDirection.Row }
        };
        canvas.AddChild(new UILabel { Text = "Test", OverrideStyle = new UIStyle { FontSize = 14 } });

        var mgr = new UIManager();
        mgr.RegisterCanvas(canvas);
        mgr.Update(0.016f);

        var mockGfx = new MockGraphics();
        mgr.Render(mockGfx);

        Assert.True(mockGfx.TextCalls.Count > 0, "render should produce text draw calls");
    }

    [Fact]
    public void Canvas_Render_WorldSpace_NotRenderedByUIManagerRender()
    {
        var canvas = new UICanvas
        {
            Width = 800,
            Height = 600,
            Space = CanvasSpace.World,
            LayoutEngine = new FlexLayout { Direction = FlexDirection.Row }
        };
        canvas.AddChild(new UILabel { Text = "WorldUI", OverrideStyle = new UIStyle { FontSize = 14 } });

        var mgr = new UIManager();
        mgr.RegisterCanvas(canvas);
        mgr.Update(0.016f);

        var mockGfx = new MockGraphics();
        mgr.Render(mockGfx); // 只渲染 Screen 空间

        Assert.Empty(mockGfx.TextCalls);
    }

    [Fact]
    public void Canvas_RenderWorldSpace_RendersWorldCanvas()
    {
        var canvas = new UICanvas
        {
            Width = 800,
            Height = 600,
            Space = CanvasSpace.World,
            LayoutEngine = new FlexLayout { Direction = FlexDirection.Row }
        };
        canvas.AddChild(new UILabel { Text = "WorldUI", OverrideStyle = new UIStyle { FontSize = 14 } });

        var mgr = new UIManager();
        mgr.RegisterCanvas(canvas);
        mgr.Update(0.016f);

        var mockGfx = new MockGraphics();
        mgr.RenderWorldSpace(mockGfx);

        Assert.Contains(mockGfx.TextCalls, c => c.text == "WorldUI");
    }

    // MockGraphics 已提取到 TestHelpers/MockGraphics.cs
}
