using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIScrollViewTests
{
    [Fact]
    public void DefaultState_HasZeroScroll()
    {
        var sv = new UIScrollView { Width = 100, Height = 100 };
        Assert.Equal(0, sv.ScrollX);
        Assert.Equal(0, sv.ScrollY);
        Assert.NotNull(sv.Content);
    }

    [Fact]
    public void ScrollX_ClampedToMax()
    {
        var sv = new UIScrollView { Width = 100, Height = 100 };
        sv.Content.Width = 300;
        sv.Content.Height = 50;

        sv.ScrollX = 999;

        Assert.Equal(sv.MaxScrollX, sv.ScrollX);
        Assert.True(sv.ScrollX > 0);
    }

    [Fact]
    public void ScrollY_ClampedToMax()
    {
        var sv = new UIScrollView { Width = 100, Height = 100 };
        sv.Content.Width = 50;
        sv.Content.Height = 300;

        sv.ScrollY = 999;

        Assert.Equal(sv.MaxScrollY, sv.ScrollY);
    }

    [Fact]
    public void Scroll_NegativeValue_ClampedToZero()
    {
        var sv = new UIScrollView { Width = 100, Height = 100 };
        sv.Content.Width = 300;
        sv.Content.Height = 300;

        sv.ScrollX = -50;
        sv.ScrollY = -50;

        Assert.Equal(0, sv.ScrollX);
        Assert.Equal(0, sv.ScrollY);
    }

    [Fact]
    public void MaxScroll_WhenContentFits_IsZero()
    {
        var sv = new UIScrollView { Width = 100, Height = 100 };
        sv.Content.Width = 50;
        sv.Content.Height = 50;

        Assert.Equal(0, sv.MaxScrollX);
        Assert.Equal(0, sv.MaxScrollY);
    }

    [Fact]
    public void Measure_SetsReasonableDefaults()
    {
        var sv = new UIScrollView();
        sv.Measure();

        Assert.True(sv.DesiredWidth > 0);
        Assert.True(sv.DesiredHeight > 0);
    }

    [Fact]
    public void Content_AddChild_GoesToContent()
    {
        var sv = new UIScrollView();
        var label = new UILabel { Text = "Hello" };
        sv.AddChild(label);

        Assert.Contains(label, sv.Content.Children);
    }

    [Fact]
    public void OnEvent_MouseWheel_Scrolls()
    {
        var sv = new UIScrollView { Width = 100, Height = 100, WheelSensitivity = 10 };
        sv.Content.Width = 100;
        sv.Content.Height = 300;

        sv.OnEvent(UIEvent.MouseWheel(50, 50, 5));

        Assert.True(sv.ScrollY > 0);
    }

    [Fact]
    public void OnEvent_MouseWheel_Outside_DoesNotScroll()
    {
        var sv = new UIScrollView { Width = 100, Height = 100 };
        sv.Content.Width = 100;
        sv.Content.Height = 300;

        sv.OnEvent(UIEvent.MouseWheel(500, 500, 5));

        Assert.Equal(0, sv.ScrollY);
    }

    [Fact]
    public void OnEvent_Disabled_ReturnsFalse()
    {
        var sv = new UIScrollView { Width = 100, Height = 100, Enabled = false };

        var consumed = sv.OnEvent(UIEvent.MouseWheel(50, 50, 5));

        Assert.False(consumed);
    }
}
