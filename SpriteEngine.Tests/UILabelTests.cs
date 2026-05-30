using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UILabelTests
{
    [Fact]
    public void DefaultText_IsEmpty()
    {
        var label = new UILabel();
        Assert.Equal("", label.Text);
    }

    [Fact]
    public void Measure_CalculatesDesiredSize()
    {
        var label = new UILabel
        {
            Text = "Hello",
            OverrideStyle = new UIStyle { FontSize = 10 }
        };
        label.Measure();

        Assert.True(label.DesiredWidth > 0);
        Assert.True(label.DesiredHeight > 0);
    }

    [Fact]
    public void Measure_EmptyText_ZeroWidth()
    {
        var label = new UILabel
        {
            Text = "",
            OverrideStyle = new UIStyle { FontSize = 10 }
        };
        label.Measure();

        Assert.Equal(0, label.DesiredWidth);
        Assert.True(label.DesiredHeight > 0);
    }

    [Fact]
    public void Measure_LongerText_Wider()
    {
        var label1 = new UILabel { Text = "A", OverrideStyle = new UIStyle { FontSize = 10 } };
        var label2 = new UILabel { Text = "AAAAA", OverrideStyle = new UIStyle { FontSize = 10 } };
        label1.Measure();
        label2.Measure();

        Assert.True(label2.DesiredWidth > label1.DesiredWidth);
    }

    [Fact]
    public void Measure_LargerFont_Taller()
    {
        var label1 = new UILabel { Text = "A", OverrideStyle = new UIStyle { FontSize = 10 } };
        var label2 = new UILabel { Text = "A", OverrideStyle = new UIStyle { FontSize = 20 } };
        label1.Measure();
        label2.Measure();

        Assert.True(label2.DesiredHeight > label1.DesiredHeight);
    }
}
