using NLua;
using SpriteCore.Graphics;
using SpriteCore.Scripting;
using Xunit;

namespace SpriteCore.Tests.Scripting;

public class LuaConstantsTests
{
    [Fact]
    public void RegisterConstants_ExposesShapeModes()
    {
        using var lua = new Lua();
        lua.LoadCLRPackage();
        SP5.RegisterConstants(lua);

        Assert.Equal((double)(int)SPRectMode.CORNER, lua["CORNER"]);
        Assert.Equal((double)(int)SPRectMode.CORNERS, lua["CORNERS"]);
        Assert.Equal((double)(int)SPRectMode.RADIUS, lua["RADIUS"]);
        Assert.Equal((double)(int)SPRectMode.CENTER, lua["CENTER"]);
    }

    [Fact]
    public void RegisterConstants_ExposesTextAligns()
    {
        using var lua = new Lua();
        lua.LoadCLRPackage();
        SP5.RegisterConstants(lua);

        Assert.Equal((double)(int)SPTextAlignH.LEFT, lua["LEFT"]);
        Assert.Equal((double)(int)SPTextAlignH.RIGHT, lua["RIGHT"]);
        Assert.Equal((double)(int)SPTextAlignV.TOP, lua["TOP"]);
        Assert.Equal((double)(int)SPTextAlignV.BOTTOM, lua["BOTTOM"]);
        Assert.Equal((double)(int)SPTextAlignV.BASELINE, lua["BASELINE"]);
    }

    [Fact]
    public void RegisterConstants_ExposesStrokeStyles()
    {
        using var lua = new Lua();
        lua.LoadCLRPackage();
        SP5.RegisterConstants(lua);

        Assert.Equal((double)(int)SPStrokeCap.SQUARE, lua["SQUARE"]);
        Assert.Equal((double)(int)SPStrokeCap.ROUND, lua["ROUND"]);
        Assert.Equal((double)(int)SPStrokeCap.PROJECT, lua["PROJECT"]);
        Assert.Equal((double)(int)SPStrokeJoin.MITER, lua["MITER"]);
        Assert.Equal((double)(int)SPStrokeJoin.BEVEL, lua["BEVEL"]);
    }

    [Fact]
    public void RegisterConstants_ExposesColorModes()
    {
        using var lua = new Lua();
        lua.LoadCLRPackage();
        SP5.RegisterConstants(lua);

        Assert.Equal((double)(int)SPColorMode.RGB, lua["RGB"]);
        Assert.Equal((double)(int)SPColorMode.HSB, lua["HSB"]);
    }
}
