using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIListTests
{
    [Fact]
    public void DefaultState_IsEmptyAndNoSelection()
    {
        var list = new UIList();
        Assert.Empty(list.Items);
        Assert.Equal(-1, list.SelectedIndex);
        Assert.Null(list.SelectedItem);
    }

    [Fact]
    public void SetItems_PopulatesList()
    {
        var list = new UIList();
        list.SetItems(new[] { "Red", "Green", "Blue" });

        Assert.Equal(3, list.Items.Count);
        Assert.Equal("Green", list.Items[1]);
    }

    [Fact]
    public void AddItem_AppendsToList()
    {
        var list = new UIList();
        list.AddItem("First");
        list.AddItem("Second");

        Assert.Equal(2, list.Items.Count);
    }

    [Fact]
    public void Measure_CalculatesHeight()
    {
        var list = new UIList { ItemHeight = 30 };
        list.SetItems(new[] { "A", "B", "C" });
        list.Measure();

        Assert.True(list.DesiredHeight > 0);
    }

    [Fact]
    public void OnEvent_ClickItem_SelectsIt()
    {
        var list = new UIList { Width = 100, Height = 120, ItemHeight = 30 };
        list.SetItems(new[] { "Apple", "Banana", "Cherry" });

        var consumed = list.OnEvent(UIEvent.MousePressed(10, 45, 0)); // 第二个条目 (y=30~60)

        Assert.True(consumed);
        Assert.Equal(1, list.SelectedIndex);
        Assert.Equal("Banana", list.SelectedItem);
    }

    [Fact]
    public void SelectedIndex_FiresEvent()
    {
        var list = new UIList();
        list.SetItems(new[] { "X", "Y", "Z" });

        string? selected = null;
        list.OnSelectionChanged += s => selected = s;

        list.SelectedIndex = 2;

        Assert.Equal("Z", selected);
    }

    [Fact]
    public void RemoveItemAt_UpdatesSelection()
    {
        var list = new UIList();
        list.SetItems(new[] { "A", "B", "C" });
        list.SelectedIndex = 1; // B

        list.RemoveItemAt(1);

        Assert.Equal(-1, list.SelectedIndex);
        Assert.Equal(2, list.Items.Count);
    }

    [Fact]
    public void RemoveItemAt_BeforeSelection_AdjustsIndex()
    {
        var list = new UIList();
        list.SetItems(new[] { "A", "B", "C" });
        list.SelectedIndex = 2; // C

        list.RemoveItemAt(0); // remove A

        Assert.Equal(1, list.SelectedIndex); // C becomes index 1
        Assert.Equal("C", list.SelectedItem);
    }

    [Fact]
    public void ClearItems_ResetsSelection()
    {
        var list = new UIList();
        list.SetItems(new[] { "A", "B" });
        list.SelectedIndex = 0;

        list.ClearItems();

        Assert.Empty(list.Items);
        Assert.Equal(-1, list.SelectedIndex);
    }

    [Fact]
    public void OnEvent_ClickOutside_DoesNotSelect()
    {
        var list = new UIList { Width = 100, ItemHeight = 30 };
        list.SetItems(new[] { "A", "B" });

        var consumed = list.OnEvent(UIEvent.MousePressed(200, 200, 0));

        Assert.False(consumed);
        Assert.Equal(-1, list.SelectedIndex);
    }

    [Fact]
    public void Disabled_DoesNotRespondToEvents()
    {
        var list = new UIList { Width = 100, ItemHeight = 30, Enabled = false };
        list.SetItems(new[] { "A", "B" });

        var consumed = list.OnEvent(UIEvent.MouseMoved(10, 10));

        Assert.False(consumed);
        Assert.False(list.IsHovered);
    }
}
