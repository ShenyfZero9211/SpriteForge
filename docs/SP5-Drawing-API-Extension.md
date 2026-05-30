# SpriteCore 绘图 API 扩展 —— 技术文档

> 版本：incubation-0.0.11  
> 关联 Issue：补齐 Processing 风格基础绘图方法（RectMode、TextAlign、Arc、Quad 等）

---

## 1. 背景与目标

SpriteCore 的 `SP5` 静态门面与 `Sketch` 基类原本只提供了最基础的绘图原语（`Rect`、`Ellipse`、`Circle`、`Line`、`Triangle`）和样式方法（`Fill`、`Stroke`、`StrokeWeight`）。随着框架演进到 `SPframework` 层，GameDemo 等示例已经直接遇到了 **RectMode** 和 **TextAlign** 缺失的问题。

本扩展的目标：
1. 补齐 Processing 4 中常用的基础绘图 API，降低 p5.js / Processing 开发者的迁移成本。
2. 保持与 Processing 语义一致（常量值、坐标模式、默认值）。
3. 同时暴露给 C#（`SP5` / `Sketch`）和 Lua（`ScriptEngine`）两层。
4. 默认行为完全向后兼容，旧代码零改动。

---

## 2. 架构总览

```
┌─────────────────────────────────────────────────────────────┐
│  Lua 脚本层                                                  │
│  rectMode(CENTER)  textAlign(LEFT, TOP)  arc(50,50,40,40,0,90)│
├─────────────────────────────────────────────────────────────┤
│  ScriptEngine.cs  ← 注册函数 + 全局常量                      │
├─────────────────────────────────────────────────────────────┤
│  C# 应用层                                                   │
│  Sketch / SP5 / SPframework                                  │
│  SP5.RectMode(3)  SP5.TextAlign(37, 101)                    │
├─────────────────────────────────────────────────────────────┤
│  SPGraphics（抽象渲染器）                                    │
│  RectMode / EllipseMode / ImageMode / TextAlign             │
│  Point / Quad / Arc / RoundRect                             │
├─────────────────────────────────────────────────────────────┤
│  SkiaGraphics（SkiaSharp 实现）                              │
│  ResolveRect / ResolveEllipseRect / ResolveImageRect        │
│  文字基线偏移 / 新图形绘制                                   │
└─────────────────────────────────────────────────────────────┘
```

**核心设计原则：**
- **枚举定义在 SpriteCore**：`SPRectMode`、`SPTextAlignH` 等，不依赖 SkiaSharp 原生枚举，使 API 层可移植。
- **SPStyle 是单一真相源**：所有模式状态（RectMode、EllipseMode、TextAlign、StrokeCap 等）保存在 `SPStyle` 中，支持 `Clone()` 用于 `PushStyle`/`PopStyle`。
- **坐标语义在渲染器层解析**：`SkiaGraphics` 在绘制前根据当前 `CurrentStyle` 的 Mode 将用户传入的参数转换为 Skia 需要的实际坐标。
- **Lua 用 int 传递枚举**：NLua 对 .NET enum 支持不够友好，因此 `SP5` 门面层统一暴露 `int` 参数，同时通过 `SP5.RegisterConstants()` 向 Lua 全局表注入常量值。

---

## 3. 新增 API 清单

### 3.1 枚举与常量（`SPConstants.cs`）

| 枚举 | 值 | Processing 对应 |
|------|-----|----------------|
| `SPRectMode.CORNER` | 0 | `CORNER` |
| `SPRectMode.CORNERS` | 1 | `CORNERS` |
| `SPRectMode.RADIUS` | 2 | `RADIUS` |
| `SPRectMode.CENTER` | 3 | `CENTER` |
| `SPEllipseMode.CORNER` | 0 | `CORNER` |
| `SPEllipseMode.CORNERS` | 1 | `CORNERS` |
| `SPEllipseMode.RADIUS` | 2 | `RADIUS` |
| `SPEllipseMode.CENTER` | 3 | `CENTER` |
| `SPImageMode.CORNER` | 0 | `CORNER` |
| `SPImageMode.CORNERS` | 1 | `CORNERS` |
| `SPImageMode.CENTER` | 3 | `CENTER` |
| `SPTextAlignH.LEFT` | 37 | `LEFT` |
| `SPTextAlignH.CENTER` | 3 | `CENTER` |
| `SPTextAlignH.RIGHT` | 39 | `RIGHT` |
| `SPTextAlignV.BASELINE` | 0 | `BASELINE` |
| `SPTextAlignV.TOP` | 101 | `TOP` |
| `SPTextAlignV.BOTTOM` | 102 | `BOTTOM` |
| `SPStrokeCap.SQUARE` | 1 | `SQUARE` |
| `SPStrokeCap.ROUND` | 2 | `ROUND` |
| `SPStrokeCap.PROJECT` | 4 | `PROJECT` |
| `SPStrokeJoin.MITER` | 1 | `MITER` |
| `SPStrokeJoin.BEVEL` | 2 | `BEVEL` |
| `SPStrokeJoin.ROUND` | 2 | `ROUND` |
| `SPColorMode.RGB` | 1 | `RGB` |
| `SPColorMode.HSB` | 3 | `HSB` |

> 常量值与 Processing 4 的 `PConstants.java` 保持一致，便于熟悉 Processing 的开发者直接对应。

### 3.2 形状模式方法

```csharp
// C#
SP5.RectMode((int)SPRectMode.CENTER);
SP5.EllipseMode((int)SPEllipseMode.CORNER);
SP5.ImageMode((int)SPImageMode.CENTER);

// Lua
rectMode(CENTER)
ellipseMode(CORNER)
imageMode(CENTER)
```

**语义对照表：**

| Mode | Rect 语义 | Ellipse 语义 | Image 语义 |
|------|-----------|--------------|------------|
| `CORNER` | `(x, y, w, h)` — 左上角+宽高 | `(x, y, w, h)` — 左上角+宽高 | `(x, y, w, h)` — 左上角+宽高 |
| `CORNERS` | `(x1, y1, x2, y2)` — 对角两点 | `(x1, y1, x2, y2)` — 对角两点 | `(x1, y1, x2, y2)` — 对角两点 |
| `CENTER` | `(cx, cy, w, h)` — 中心+宽高 | `(cx, cy, w, h)` — 中心+宽高 | `(cx, cy, w, h)` — 中心+宽高 |
| `RADIUS` | `(cx, cy, w, h)` — 中心+半宽高 | `(cx, cy, w, h)` — 中心+半宽高 | — |

> `Circle()` 同样受 `EllipseMode` 影响（与 Processing 一致）。

### 3.3 文字对齐

```csharp
// C# — 水平对齐
SP5.TextAlign((int)SPTextAlignH.CENTER);
// C# — 水平+垂直对齐
SP5.TextAlign((int)SPTextAlignH.LEFT, (int)SPTextAlignV.TOP);

// Lua
textAlign(CENTER)
textAlign(LEFT, TOP)
```

**垂直对齐实现细节：**

SkiaSharp 的 `SKFont` 默认以 **基线 (baseline)** 作为 `DrawText` 的 y 坐标。本扩展根据 `TextAlignV` 自动计算偏移：

| `TextAlignV` | 行为 | 实现 |
|--------------|------|------|
| `BASELINE` | y 即基线 | 直接使用 y |
| `TOP` | y 即文字顶部 | `y - fontMetrics.Ascent` |
| `BOTTOM` | y 即文字底部 | `y - fontMetrics.Descent` |

### 3.4 样式栈

```csharp
// C#
SP5.PushStyle();
SP5.Fill(255, 0, 0);
SP5.StrokeWeight(5);
SP5.RectMode((int)SPRectMode.CENTER);
// ... 绘制 ...
SP5.PopStyle();  // 恢复之前的所有样式

// Lua
pushStyle()
fill(255, 0, 0)
strokeWeight(5)
rectMode(CENTER)
-- ... 绘制 ...
popStyle()
```

`PushStyle()` / `PopStyle()` 会保存/恢复：Fill、Stroke、StrokeWeight、StrokeCap、StrokeJoin、TextSize、TextAlign（水平和垂直）、ColorMode、RectMode、EllipseMode、ImageMode、Tint 状态。

### 3.5 笔触外观

```csharp
// C#
SP5.StrokeCap((int)SPStrokeCap.ROUND);    // 默认
SP5.StrokeJoin((int)SPStrokeJoin.MITER);  // 默认

// Lua
strokeCap(ROUND)
strokeJoin(MITER)
```

### 3.6 新图形原语

| 方法 | 签名 | 说明 |
|------|------|------|
| `Point` | `(float x, float y)` | 以当前 `StrokeWeight / 2` 为半径绘制小圆 |
| `Quad` | `(x1,y1, x2,y2, x3,y3, x4,y4)` | 四边形，自动闭合 |
| `Arc` | `(x, y, w, h, start, stop)` | 椭圆弧，`start`/`stop` 为度数 |
| `RoundRect` | `(x, y, w, h, r)` | 圆角矩形，四个角半径相同 |

```csharp
// C#
SP5.Point(50, 50);
SP5.Quad(0, 0, 100, 0, 100, 100, 0, 100);
SP5.Arc(50, 50, 40, 40, 0, 90);        // 0° 到 90° 的弧
SP5.RoundRect(10, 10, 80, 80, 8);      // 8px 圆角

// Lua
point(50, 50)
quad(0, 0, 100, 0, 100, 100, 0, 100)
arc(50, 50, 40, 40, 0, 90)
roundRect(10, 10, 80, 80, 8)
```

### 3.7 图像与 Tint（Lua 层新增）

```lua
-- Lua 中现在可以加载和绘制图像
local img = loadImage("assets/player.png")
image(img, 100, 200)
image(img, 100, 200, 64, 64)  -- 指定尺寸

tint(255, 128)               -- 半透明灰度
tint(255, 0, 0, 200)         -- 红色半透明
noTint()                     -- 清除 tint
```

---

## 4. 使用示例

### 4.1 C# — Sketch / SPframework

```csharp
using SpriteCore.Graphics;
using static SpriteCore.Graphics.SP5;

public class MySketch : Sketch
{
    public override void Draw()
    {
        Background(30);

        // 矩形以中心点为原点
        RectMode((int)SPRectMode.CENTER);
        Fill(100, 200, 255);
        Rect(200, 200, 100, 60);

        // 椭圆以左上角为原点
        EllipseMode((int)SPEllipseMode.CORNER);
        Fill(255, 100, 100);
        Ellipse(300, 100, 80, 80);

        // 文字对齐
        TextAlign((int)SPTextAlignH.CENTER, (int)SPTextAlignV.CENTER);
        TextSize(24);
        Fill(255);
        Text("Hello SpriteForge", 400, 300);

        // 样式栈
        PushStyle();
        StrokeWeight(4);
        StrokeCap((int)SPStrokeCap.PROJECT);
        NoFill();
        Arc(400, 300, 120, 120, 0, 180);
        PopStyle();
    }
}
```

### 4.2 Lua

```lua
function setup()
    -- 全局常量已自动注册
end

function draw()
    background(30)

    rectMode(CENTER)
    fill(100, 200, 255)
    rect(200, 200, 100, 60)

    ellipseMode(CORNER)
    fill(255, 100, 100)
    ellipse(300, 100, 80, 80)

    textAlign(CENTER, CENTER)
    textSize(24)
    fill(255)
    text("Hello SpriteForge", 400, 300)

    pushStyle()
    strokeWeight(4)
    strokeCap(PROJECT)
    noFill()
    arc(400, 300, 120, 120, 0, 180)
    popStyle()
end
```

---

## 5. 兼容性

### 5.1 默认行为不变

| 属性 | 旧默认值 | 新默认值 | 说明 |
|------|---------|---------|------|
| `RectMode` | `CORNER` (int 0) | `CORNER` (enum) | Rect 语义不变 |
| `EllipseMode` | `CENTER` (int 0) | `CENTER` (enum) | Ellipse/Circle 语义不变 |
| `ImageMode` | `CORNER` (int 0) | `CORNER` (enum) | Image 语义不变 |
| `TextAlignH` | `LEFT` | `LEFT` | 文字水平对齐不变 |
| `TextAlignV` | `BASELINE` | `BASELINE` | 文字垂直对齐不变 |
| `StrokeCap` | `ROUND` (SKStrokeCap) | `ROUND` (enum) | 笔触不变 |
| `StrokeJoin` | `MITER` (SKStrokeJoin) | `MITER` (enum) | 连接不变 |

### 5.2 已有 API 未删除或修改签名

- 所有旧方法的签名、参数顺序、行为均未改变。
- `SPGraphics.StrokeCap(SKStrokeCap)` 被替换为 `StrokeCap(SPStrokeCap)` — 该方法是 `virtual`，此前未通过 `SP5`/`Sketch`/`Lua` 暴露给外部，因此不影响已有用户代码。
- `SPStyle.TextAlign`（`SKTextAlign` 类型）被拆分为 `TextAlignH` + `TextAlignV`。旧代码若直接访问 `SPStyle.TextAlign` 会编译失败，但 `SPStyle` 主要是内部使用，公共 API 通过 `SP5.TextAlign()` 访问。

### 5.3 Lua 脚本兼容

旧 Lua 脚本不引用新增常量或方法，不受影响。新增常量注入到 Lua 全局表，不会覆盖用户自定义变量（除非用户恰好用了同名全局变量如 `CORNER`）。

---

## 6. 测试覆盖

| 测试类 | 数量 | 覆盖内容 |
|--------|------|---------|
| `SPStyleTests` | 3 | Clone 独立性、所有字段正确复制、默认值验证 |
| `SPGraphicsTests` | 9 | Fill/Stroke/NoFill/NoStroke、StrokeWeight、Push/PopStyle、Push/PopMatrix、Background 转发 |
| `SPModeTests` | 25 | RectMode/EllipseMode/ImageMode/TextAlign/StrokeCap 默认值与克隆；SkiaGraphics 所有模式的 smoke test；Push/PopStyle 保存/恢复 Mode 状态 |
| `TintTests` | 5 | Tint/NoTint 状态机、带 Tint 的图像绘制像素验证 |
| `LuaConstantsTests` | 4 | Lua 全局常量注册验证 |
| `SPframeworkTests` | 8 | SPframework 生命周期、场景管理 |

总计 **385 tests 全部通过**（58 SpriteCore.Tests + 327 SpriteEngine.Tests）。

---

## 7. 文件变更清单

### 新建
- `SpriteCore/src/Graphics/SPConstants.cs` — 所有 Processing 风格枚举
- `SpriteCore.Tests/Graphics/SPModeTests.cs` — 模式与 smoke test
- `SpriteCore.Tests/Scripting/LuaConstantsTests.cs` — Lua 常量注册测试

### 修改
- `SpriteCore/src/Graphics/SPStyle.cs` — int 占位符 → 枚举，TextAlign 拆分为 H/V
- `SpriteCore/src/Graphics/SPGraphics.cs` — 新增 Point/Quad/Arc 抽象方法；RectMode/EllipseMode/ImageMode/TextAlign/StrokeCap/StrokeJoin 虚方法
- `SpriteCore/src/Graphics/SkiaGraphics.cs` — 坐标语义解析、Text 垂直对齐、新图形实现、枚举映射
- `SpriteCore/src/Graphics/SP5.cs` — 门面方法、Lua 适配、常量注册
- `SpriteCore/src/Graphics/Sketch.cs` — 委托方法
- `SpriteCore/src/Scripting/ScriptEngine.cs` — Lua 函数注册、全局常量注入
- `SpriteCore.Tests/SPStyleTests.cs` — 更新以匹配新枚举类型
- `SpriteCore.Tests/SPGraphicsTests.cs` / `SPMatrix2DTests.cs` / `Graphics/TintTests.cs` — 补充 Point/Quad/Arc 空实现
- `SpriteEngine.Tests/TestHelpers/MockGraphics.cs` / `CameraTests.cs` — 补充 Point/Quad/Arc 空实现
- `SpriteLauncher/game-samples/01_GameDemo/GameDemo.cs` — 改用 RectMode(CENTER) 和 TextAlign
