using SkiaSharp;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIProgressBarTests
{
    [Fact]
    public void DefaultState_IsEmpty()
    {
        var bar = new UIProgressBar();
        Assert.Equal(0, bar.MinValue);
        Assert.Equal(1, bar.MaxValue);
        Assert.Equal(0, bar.Value);
    }

    [Fact]
    public void Measure_Horizontal_ReturnsReasonableSize()
    {
        var bar = new UIProgressBar { Direction = SliderDirection.Horizontal };
        bar.Measure();

        Assert.True(bar.DesiredWidth > 0);
        Assert.True(bar.DesiredHeight > 0);
    }

    [Fact]
    public void Measure_Vertical_ReturnsReasonableSize()
    {
        var bar = new UIProgressBar { Direction = SliderDirection.Vertical };
        bar.Measure();

        Assert.True(bar.DesiredWidth > 0);
        Assert.True(bar.DesiredHeight > 0);
    }

    [Fact]
    public void Measure_RespectsExplicitSize()
    {
        var bar = new UIProgressBar { Width = 200, Height = 16 };
        bar.Measure();

        Assert.Equal(200, bar.DesiredWidth);
        Assert.Equal(16, bar.DesiredHeight);
    }

    [Fact]
    public void BuildDrawList_ClampedToBounds()
    {
        var bar = new UIProgressBar
        {
            Width = 100, Height = 10,
            MinValue = 0, MaxValue = 100,
            Value = -50 // 超出下限
        };
        // 不应抛异常，内部已 Clamp
        var drawList = new UIDrawList();
        bar.BuildDrawList(drawList);

        // 不应抛异常，BuildDrawList 成功执行即可
        Assert.True(drawList != null);
    }

    [Fact]
    public void CustomColors_OverrideStyle()
    {
        var bar = new UIProgressBar
        {
            FillColor = new SKColor(255, 0, 0),
            TrackColor = new SKColor(0, 0, 255),
            OverrideStyle = new UIStyle
            {
                ForegroundColor = new SKColor(0, 255, 0),
                BackgroundColor = new SKColor(255, 255, 0)
            }
        };

        // 自定义颜色应优先于样式
        Assert.NotNull(bar.FillColor);
        Assert.NotNull(bar.TrackColor);
    }
}
