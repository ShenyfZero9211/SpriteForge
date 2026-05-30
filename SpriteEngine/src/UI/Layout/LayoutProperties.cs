namespace SpriteEngine.UI;

/// <summary>
/// Flexbox 布局属性，附加到每个 UIElement 上。
/// 受 CSS Flex 启发。
/// </summary>
public class LayoutProperties
{
    /// <summary>占用剩余空间的比例（0 = 不占用）</summary>
    public float FlexGrow { get; set; } = 0;

    /// <summary>空间不足时的收缩比例（0 = 不收缩）</summary>
    public float FlexShrink { get; set; } = 1;

    /// <summary>基础尺寸（null = 使用 Measure 结果）</summary>
    public float? FlexBasis { get; set; } = null;

    /// <summary>覆盖容器的 AlignItems</summary>
    public FlexAlign? AlignSelf { get; set; } = null;
}

public enum FlexDirection { Row, Column }

public enum FlexWrap { NoWrap, Wrap }

public enum FlexJustify { Start, Center, End, SpaceBetween, SpaceAround }

public enum FlexAlign { Start, Center, End, Stretch }

/// <summary>
/// 锚点标志。受 p5engine Anchor 启发。
/// </summary>
[Flags]
public enum AnchorFlags
{
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8,
    HCenter = 16,
    VCenter = 32,
    Stretch = Top | Bottom | Left | Right
}
