namespace SpriteCore.Graphics;

/// <summary>
/// Processing-style constants for SpriteCore.
/// Values mirror Processing 4's PConstants where applicable.
/// </summary>
public enum SPRectMode
{
    CORNER = 0,
    CORNERS = 1,
    RADIUS = 2,
    CENTER = 3,
}

public enum SPEllipseMode
{
    CORNER = 0,
    CORNERS = 1,
    RADIUS = 2,
    CENTER = 3,
}

public enum SPImageMode
{
    CORNER = 0,
    CORNERS = 1,
    CENTER = 3,
}

public enum SPTextAlignH
{
    LEFT = 37,
    CENTER = 3,
    RIGHT = 39,
}

public enum SPTextAlignV
{
    BASELINE = 0,
    TOP = 101,
    BOTTOM = 102,
}

public enum SPStrokeCap
{
    SQUARE = 1,
    ROUND = 2,
    PROJECT = 4,
}

public enum SPStrokeJoin
{
    MITER = 1,
    BEVEL = 2,
    ROUND = 2,
}

public enum SPColorMode
{
    RGB = 1,
    HSB = 3,
}

public enum SPBlendMode
{
    REPLACE = 0,
    BLEND = 1,
    ADD = 2,
    SUBTRACT = 4,
    LIGHTEST = 8,
    DARKEST = 16,
    DIFFERENCE = 32,
    EXCLUSION = 64,
    MULTIPLY = 128,
    SCREEN = 256,
    OVERLAY = 512,
    HARD_LIGHT = 1024,
    SOFT_LIGHT = 2048,
    DODGE = 4096,
    BURN = 8192,
}
