# SpriteCore 架构重构：P5.cs → SPGraphics 抽象层

> **日期**: 2026-05-29  
> **范围**: SpriteCore 渲染子系统  
> **影响**: 所有绘图 API、C# Sketch、Lua 脚本绑定  
> **参考**: Processing 4 源码（`PApplet` → `PGraphics` → `PStyle`/`PMatrix2D`）

---

## 1. 重构背景

### 1.1 问题

重构前，`P5.cs` 是一个 **200+ 行的静态类**，直接内联了所有 SkiaSharp 绘制实现：

- **职责混杂**：API 门面 + 样式状态管理 + 具体绘制逻辑 + 矩阵变换，全部耦合在一个类中
- **难以扩展**：新增渲染后端（如 OpenGL）需要重写整个类
- **状态泄漏**：`DrawingStyle` 是私有嵌套类，外部无法访问或扩展
- **重复代码**：每个形状方法（Rect/Ellipse/Circle/Triangle）都手写 fill+stroke 双重绘制
- **无抽象层**：`Renderer.cs` 仅管理 `SKSurface` 生命周期，与绘图 API 完全分离

### 1.2 目标

参考 Processing 源码架构，将渲染系统拆分为 **四层**：

| 层级 | Processing | SpriteCore（重构后） | 职责 |
|------|-----------|---------------------|------|
| 门面层 | `PApplet` | `SP5` | 静态 API 门面，委托给 `SPGraphics` |
| 抽象渲染器 | `PGraphics` | `SPGraphics` | 样式栈、矩阵栈、抽象绘制接口 |
| 样式数据 | `PStyle` | `SPStyle` | 纯数据结构，支持 Clone/Push/Pop |
| 矩阵 | `PMatrix2D` | `SPMatrix2D` | 2D 仿射变换，独立于图形库 |
| 具体实现 | `PGraphicsJava2D` | `SkiaGraphics` | SkiaSharp 绘制 + SDL 呈现 |

---

## 2. 架构设计

### 2.1 类依赖关系

```
                    Lua 脚本 / C# Sketch
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  SP5 (static facade)                                        │
│  ├─ Width/Height/FrameCount/DeltaTime                       │
│  ├─ Background/Fill/Stroke/NoFill/NoStroke                  │
│  ├─ Rect/Ellipse/Circle/Line/Triangle                       │
│  ├─ PushMatrix/PopMatrix/Translate/Rotate/Scale             │
│  ├─ Text/TextSize                                           │
│  └─ MouseX/MouseY/MouseIsPressed/IsKeyPressed               │
│                                                             │
│  所有方法委托给 SP5.Graphics (SPGraphics?)                  │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  SPGraphics (abstract)                                      │
│  ├─ 样式栈: Stack<SPStyle> CurrentStyle                     │
│  ├─ 矩阵栈: Stack<SPMatrix2D> CurrentMatrix                 │
│  ├─ 抽象绘制: Rect/Ellipse/Circle/Line/Triangle/Text/Image  │
│  ├─ 生命周期: BeginFrame/EndFrame/Resize/Present            │
│  └─ 默认实现: Fill/Stroke/Background (灰度→RGB 转发)        │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  SkiaGraphics : SPGraphics, IDisposable                     │
│  ├─ SKSurface? _surface                                     │
│  ├─ 实现所有抽象绘制方法（统一 DrawWithFillAndStroke）      │
│  ├─ ApplyMatrix: 将 SPMatrix2D 应用到 SKCanvas              │
│  ├─ GetFillPaint/GetStrokePaint: 从 SPStyle 生成 SKPaint    │
│  └─ Present: Snapshot → PeekPixels → SDL_UpdateTexture      │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 关键设计决策

#### 决策 1：SP5 保持静态门面（而非实例化）

- **原因**：Lua 绑定通过 `typeof(SP5)` 注册静态方法，改为实例化会破坏所有 Lua 脚本
- **折中**：SP5 内部持有 `static SPGraphics? Graphics`，在运行时注入具体实现
- **收益**：C# Sketch 和 Lua 脚本的 API 调用方式完全不变

#### 决策 2：样式栈与矩阵栈放在抽象层（SPGraphics）

- **原因**：Push/Pop 逻辑与具体渲染库无关，放在抽象层避免重复实现
- **注意**：当前 `PushMatrix`/`PopMatrix` 仍直接委托给 `SPMatrix2D` 栈，SkiaGraphics 在每帧绘制前通过 `ApplyMatrix()` 将矩阵同步到 `SKCanvas`。未来若需要独立于 Skia 的矩阵运算，可完全使用 `SPMatrix2D` 自行变换顶点。

#### 决策 3：统一 `DrawWithFillAndStroke` 消除重复代码

重构前，每个形状方法都有类似的 fill+stroke 双重绘制逻辑：

```csharp
// 旧代码（重复模式）
public static void Rect(...)
{
    Canvas.DrawRect(rect, fillPaint);
    if (strokePaint.Color.Alpha > 0)
        Canvas.DrawRect(rect, strokePaint);
}
```

重构后，所有形状方法通过 `DrawWithFillAndStroke(Action<SKPaint>)` 统一处理：

```csharp
// 新代码（统一模式）
public override void Rect(float x, float y, float w, float h)
{
    var rect = new SKRect(x, y, x + w, y + h);
    DrawWithFillAndStroke(paint => _surface!.Canvas.DrawRect(rect, paint));
}
```

- **收益**：新增形状只需一行核心绘制代码，fill/stroke/矩阵/抗锯齿全部由模板处理

#### 决策 4：SkiaGraphics 同时承担 Renderer 职责

- **原因**：`Renderer.cs` 原本只管理 `SKSurface` 的创建/销毁/呈现，与绘图 API 分离导致 `SP5.Canvas` 属性成为耦合点
- **新设计**：`SkiaGraphics` 内部持有 `_surface`，提供 `Initialize/Resize/BeginFrame/EndFrame/Present` 完整生命周期
- **结果**：删除 `P5.Canvas` 属性，SketchApp/Program 不再需要手动同步 Canvas

---

## 3. 文件变更清单

### 3.1 新增文件

| 文件 | 行数 | 职责 |
|------|------|------|
| `SpriteCore/src/Graphics/SPStyle.cs` | 59 | 绘图样式纯数据结构，支持 Clone |
| `SpriteCore/src/Graphics/SPMatrix2D.cs` | 32 | 2D 变换矩阵，封装 SKMatrix |
| `SpriteCore/src/Graphics/SPGraphics.cs` | 87 | 抽象渲染器，样式栈+矩阵栈+抽象绘制接口 |
| `SpriteCore/src/Graphics/SkiaGraphics.cs` | 192 | SkiaSharp 实现，统一 fill+stroke，SDL 呈现 |
| `SpriteCore/src/Graphics/SP5.cs` | 74 | 静态 API 门面，全部委托给 SPGraphics |

### 3.2 删除文件

| 文件 | 说明 |
|------|------|
| `SpriteCore/src/Graphics/P5.cs` | 原 ~200+ 行内联实现，职责已拆分至 SP5 + SPGraphics + SkiaGraphics |

### 3.3 修改文件

| 文件 | 变更内容 |
|------|----------|
| `SpriteCore/src/Graphics/Sketch.cs` | `AppRenderer` → `AppGraphics`（SkiaGraphics）；`Size()` 中 `SP5.Graphics?.Resize()` |
| `SpriteCore/src/Graphics/SketchApp.cs` | `_renderer` → `_graphics`（SkiaGraphics）；去掉 `SP5.Canvas = ...`；改用 `BeginFrame/EndFrame/Present` |
| `SpriteLauncher/Program.cs` | Lua 模式同样改用 `SkiaGraphics`，去掉 `SP5.Canvas` 同步 |
| `SpriteCore/src/Scripting/ScriptEngine.cs` | `typeof(P5)` → `typeof(SP5)` |
| `SpriteLauncher/cs-samples/03_CsInput/InputSketch.cs` | `P5.` → `SP5.` |

### 3.4 保留文件（过渡）

| 文件 | 说明 |
|------|------|
| `SpriteCore/src/Graphics/Renderer.cs` | 保留但不再使用，后续版本可安全删除 |

---

## 4. 兼容性说明

### 4.1 Lua 脚本（✅ 完全兼容）

Lua 绑定的函数注册在 `ScriptEngine.RegisterP5API()` 中：

```csharp
_lua.RegisterFunction("background", null, typeof(SP5).GetMethod("Background", ...)!);
```

`SP5` 的静态方法签名与旧 `P5` 完全一致，Lua 脚本无需任何修改。

### 4.2 C# Sketch（✅ 完全兼容）

`Sketch.cs` 的实例方法仍委托给 `SP5`：

```csharp
public void Background(float gray) => SP5.Background(gray);
public void Rect(float x, float y, float w, float h) => SP5.Rect(x, y, w, h);
```

所有 C# 示例（Hello/Shapes/Input/Audio）编译通过，运行正常。

### 4.3 内部 API 变更（⚠️ 仅影响引擎开发者）

| 旧 API | 新 API | 影响范围 |
|--------|--------|----------|
| `P5.Canvas` (setter) | `SP5.Graphics = graphics` | 仅 SketchApp/Program |
| `Renderer` 类 | `SkiaGraphics` 类 | 仅 SketchApp/Program |
| `P5` 静态类 | `SP5` 静态类 | 仅 ScriptEngine/cs-samples |

---

## 5. 扩展预留

此次重构为以下功能预留了清晰的扩展点：

| 功能 | 扩展方式 | 说明 |
|------|----------|------|
| **HSB 颜色模式** | `SPStyle.ColorMode` 已预留字段 | 在 `SPGraphics.Fill()` 中根据 ColorMode 做 RGB↔HSB 转换 |
| **rectMode/ellipseMode** | `SPStyle.RectMode`/`EllipseMode` 已预留字段 | 在 `SkiaGraphics.Rect()`/`Ellipse()` 中根据模式调整坐标计算 |
| **OpenGL 后端** | 新建 `OpenGLGraphics : SPGraphics` | 实现抽象方法，替换 `SP5.Graphics` 即可切换后端 |
| **beginShape/endShape** | 在 `SPGraphics` 添加抽象方法 | `SkiaGraphics` 用 `SKPath` 实现 |
| **图像/纹理系统** | `SPGraphics.Image()` 已定义 | 当前实现为 `DrawBitmap`，未来可扩展为纹理图集 |
| **文字对齐** | `SPStyle.TextAlign` 已预留 | 当前默认 `Left`，未来支持 `Center`/`Right` |

---

## 6. 验证结果

### 6.1 编译

```bash
$ dotnet build SpriteForge.sln
Build succeeded.
    0 Error(s), 7 Warning(s)  ← 仅 SDL2-CS 外部警告
```

### 6.2 C# Sketch 运行

```bash
$ dotnet run --project SpriteLauncher -- --sketch HelloSketch
[INFO] [SketchApp] Audio initialized
[INFO] [SketchApp] Running sketch: HelloSketch
```

### 6.3 Lua 脚本运行

```bash
$ dotnet run --project SpriteLauncher -- samples/01_HelloSprite/main.lua
Hello from Lua!
[INFO] [Launcher] Starting Lua script: ...
```

---

## 7. 经验教训

1. **静态门面 vs 实例化的权衡**：Lua 绑定要求 `typeof()` 指向具体类，若将 SP5 改为实例化，需要重新设计 NLua 注册方式。保持静态是最小破坏的选择。
2. **Renderer 与 Graphics 的合并**：最初犹豫是否保留独立的 `Renderer` 类。实践证明，将 Surface 生命周期与绘制 API 合并在 `SkiaGraphics` 中，消除了 `P5.Canvas` 这一耦合点，代码更简洁。
3. **SPMatrix2D 的渐进实现**：当前 `SPMatrix2D` 仍依赖 `SKMatrix`（内部封装），但接口已独立于 SkiaSharp。未来若需完全脱离 Skia，只需替换内部矩阵运算为手动 3x3 矩阵乘法。
4. **命名规则的重要性**：项目约定禁止 "P" 打头的类名（避免与 Processing 混淆），新类统一使用 "SP" 前缀（SPStyle, SPMatrix2D, SPGraphics, SP5），保持命名空间清晰。

---

*Document Created: 2026-05-29*  
*Author: Kimi Code CLI*  
*Tag Reference: incubation-0.0.3*
