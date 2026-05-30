using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIDropdownTests
{
    [Fact]
    public void DefaultState_IsNotExpandedAndNoSelection()
    {
        var dd = new UIDropdown();
        Assert.False(dd.IsExpanded);
        Assert.Equal(-1, dd.SelectedIndex);
        Assert.Null(dd.SelectedText);
    }

    [Fact]
    public void SetItems_PopulatesList()
    {
        var dd = new UIDropdown();
        dd.SetItems(new[] { "A", "B", "C" });

        Assert.Equal(3, dd.Items.Count);
        Assert.Equal("A", dd.Items[0]);
    }

    [Fact]
    public void Measure_Collapsed_ReturnsHeaderHeight()
    {
        var dd = new UIDropdown { Width = 120, ItemHeight = 28 };
        dd.Measure();

        Assert.True(dd.DesiredWidth > 0);
        Assert.True(dd.DesiredHeight > 0);
    }

    [Fact]
    public void Measure_Always_ReturnsHeaderHeight()
    {
        var dd = new UIDropdown { Width = 120, ItemHeight = 28 };
        dd.SetItems(new[] { "A", "B", "C", "D", "E" });
        dd.IsExpanded = true;
        dd.Measure();

        // Measure 始终只返回头部高度，不随展开状态变化
        float headerH = Math.Max(28, 14 * 1.4f); // fontSize=14 default
        Assert.True(dd.DesiredHeight <= headerH + 1);
    }

    [Fact]
    public void OnEvent_ClickHeader_Expands()
    {
        var dd = new UIDropdown { Width = 100, Height = 30 };

        var consumed = dd.OnEvent(UIEvent.MousePressed(50, 15, 0));

        Assert.True(consumed);
        Assert.True(dd.IsExpanded);
    }

    [Fact]
    public void OnEvent_ClickHeaderAgain_Collapses()
    {
        var dd = new UIDropdown { Width = 100, Height = 30 };
        dd.OnEvent(UIEvent.MousePressed(50, 15, 0)); // expand

        var consumed = dd.OnEvent(UIEvent.MousePressed(50, 15, 0)); // collapse

        Assert.True(consumed);
        Assert.False(dd.IsExpanded);
    }

    [Fact]
    public void OnEvent_ClickOutside_Collapses()
    {
        var dd = new UIDropdown { Width = 100, Height = 30 };
        dd.OnEvent(UIEvent.MousePressed(50, 15, 0)); // expand
        Assert.True(dd.IsExpanded);

        dd.OnEvent(UIEvent.MousePressed(200, 200, 0)); // outside

        Assert.False(dd.IsExpanded);
    }

    [Fact]
    public void OnEvent_ClickOption_SelectsItem()
    {
        var dd = new UIDropdown { Width = 100, Height = 120, ItemHeight = 28 };
        dd.SetItems(new[] { "Apple", "Banana", "Cherry" });
        dd.OnEvent(UIEvent.MousePressed(50, 15, 0)); // expand
        Assert.True(dd.IsExpanded);

        string? selected = null;
        dd.OnSelectionChanged += s => selected = s;

        // 点击第一个选项（在列表区域，headerH=28，第一个选项 y=28~56）
        dd.OnEvent(UIEvent.MousePressed(50, 42, 0));

        Assert.Equal("Apple", selected);
    }

    [Fact]
    public void SelectedIndex_FiresEvent()
    {
        var dd = new UIDropdown();
        dd.SetItems(new[] { "X", "Y", "Z" });

        string? selected = null;
        dd.OnSelectionChanged += s => selected = s;

        dd.SelectedIndex = 1;

        Assert.Equal("Y", selected);
    }

    [Fact]
    public void IsExpanded_FiresEvent()
    {
        var dd = new UIDropdown();

        bool? expanded = null;
        dd.OnExpandedChanged += e => expanded = e;

        dd.IsExpanded = true;

        Assert.True(expanded);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var dd = new UIDropdown { Width = 100, Height = 30, Enabled = false };

        var consumed = dd.OnEvent(UIEvent.MousePressed(50, 15, 0));

        Assert.False(consumed);
        Assert.False(dd.IsExpanded);
    }

    [Fact]
    public void SetItems_ClearsInvalidSelection()
    {
        var dd = new UIDropdown();
        dd.SetItems(new[] { "A", "B" });
        dd.SelectedIndex = 1;
        Assert.Equal("B", dd.SelectedText);

        dd.SetItems(new[] { "X" });

        Assert.Equal(-1, dd.SelectedIndex);
    }
}
