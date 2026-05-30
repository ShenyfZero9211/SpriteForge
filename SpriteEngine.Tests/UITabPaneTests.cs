using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UITabPaneTests
{
    [Fact]
    public void DefaultState_HasNoTabs()
    {
        var pane = new UITabPane();
        Assert.Empty(pane.TabTitles);
        Assert.Equal(-1, pane.ActiveIndex);
    }

    [Fact]
    public void AddTab_CreatesContentAndSetsActive()
    {
        var pane = new UITabPane { Width = 300, Height = 200 };
        var content = pane.AddTab("First");

        Assert.NotNull(content);
        Assert.Single(pane.TabTitles);
        Assert.Equal("First", pane.TabTitles[0]);
        Assert.Equal(0, pane.ActiveIndex);
    }

    [Fact]
    public void AddTab_MultipleTabs_FirstRemainsActive()
    {
        var pane = new UITabPane { Width = 300, Height = 200 };
        pane.AddTab("A");
        pane.AddTab("B");
        pane.AddTab("C");

        Assert.Equal(3, pane.TabTitles.Count);
        Assert.Equal(0, pane.ActiveIndex);
        Assert.Equal("A", pane.ActiveTitle);
    }

    [Fact]
    public void ActiveIndex_SwitchesTab()
    {
        var pane = new UITabPane { Width = 300, Height = 200 };
        pane.AddTab("A");
        pane.AddTab("B");

        pane.ActiveIndex = 1;

        Assert.Equal(1, pane.ActiveIndex);
        Assert.Equal("B", pane.ActiveTitle);
    }

    [Fact]
    public void ActiveIndex_FiresEvent()
    {
        var pane = new UITabPane { Width = 300, Height = 200 };
        pane.AddTab("A");
        pane.AddTab("B");

        string? changed = null;
        pane.OnTabChanged += t => changed = t;

        pane.ActiveIndex = 1;

        Assert.Equal("B", changed);
    }

    [Fact]
    public void OnEvent_ClickTab_SwitchesActive()
    {
        var pane = new UITabPane { Width = 300, Height = 200, TabHeight = 36 };
        pane.AddTab("Left");
        pane.AddTab("Right");

        // 点击第二个标签（宽度 150，x=150~300）
        var consumed = pane.OnEvent(UIEvent.MousePressed(200, 18, 0));

        Assert.True(consumed);
        Assert.Equal(1, pane.ActiveIndex);
    }

    [Fact]
    public void RemoveTabAt_AdjustsActiveIndex()
    {
        var pane = new UITabPane { Width = 300, Height = 200 };
        pane.AddTab("A");
        pane.AddTab("B");
        pane.AddTab("C");
        pane.ActiveIndex = 2;

        pane.RemoveTabAt(2);

        Assert.Equal(1, pane.ActiveIndex);
        Assert.Equal(2, pane.TabTitles.Count);
    }

    [Fact]
    public void ClearTabs_ResetsEverything()
    {
        var pane = new UITabPane { Width = 300, Height = 200 };
        pane.AddTab("A");
        pane.AddTab("B");

        pane.ClearTabs();

        Assert.Empty(pane.TabTitles);
        Assert.Equal(-1, pane.ActiveIndex);
    }

    [Fact]
    public void Measure_SetsReasonableDefaults()
    {
        var pane = new UITabPane();
        pane.Measure();

        Assert.True(pane.DesiredWidth > 0);
        Assert.True(pane.DesiredHeight > 0);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var pane = new UITabPane { Width = 300, Height = 200, Enabled = false };
        pane.AddTab("A");

        var consumed = pane.OnEvent(UIEvent.MousePressed(10, 10, 0));

        Assert.False(consumed);
    }
}
