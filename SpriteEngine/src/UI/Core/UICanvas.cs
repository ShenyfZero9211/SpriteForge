using SpriteCore.Graphics;

namespace SpriteEngine.UI;

/// <summary>
/// UI 画布。作为 UI 树的最顶层容器，决定 Screen 或 World 空间渲染。
/// </summary>
public class UICanvas : UIContainer
{
    /// <summary>画布空间类型</summary>
    public CanvasSpace Space { get; set; } = CanvasSpace.Screen;

    /// <summary>Screen 空间时的渲染顺序（越大越在上层）</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>是否拦截输入（不穿透到游戏对象）</summary>
    public bool ConsumeInput { get; set; } = true;

    /// <summary>此画布专用的 DrawList</summary>
    internal UIDrawList DrawList { get; } = new();

    /// <summary>此画布专用的状态存储</summary>
    public UIStateStorage StateStorage { get; } = new();

    // ── 渲染 ──

    public void Render(SPGraphics g)
    {
        DrawList.Clear();
        BuildDrawList(DrawList);
        DrawList.Render(g);
    }

    public override void BuildDrawList(UIDrawList drawList)
    {
        // Screen 空间：覆盖整个视口
        if (Space == CanvasSpace.Screen)
        {
            // 不跟随 Transform，直接使用本地坐标
            base.BuildDrawList(drawList);
        }
        else
        {
            // World 空间：应用 Transform 的世界矩阵
            // 由调用方（Scene.Render）在 SPGraphics PushMatrix 后调用
            base.BuildDrawList(drawList);
        }
    }
}

public enum CanvasSpace
{
    /// <summary>屏幕空间覆盖层，最后渲染，不跟随相机</summary>
    Screen,

    /// <summary>世界空间，跟随 GameObject Transform，随相机移动</summary>
    World
}
