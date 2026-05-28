# SpriteForge 开发计划书

> **SpriteCore** — 类似 Processing 的创意编程框架  
> **SpriteEngine** — 类似 p5engine 的 2D 游戏引擎  
> *For Sprite, who makes every frame worth rendering.*

---

## 1. 项目愿景与架构总览

### 1.1 设计哲学

| 层级 | 名称 | 定位 | 灵感来源 |
|------|------|------|----------|
| 底层框架 | **SpriteCore** | Processing-like 创意编程框架 | p5.js / Processing 4 |
| 上层引擎 | **SpriteEngine** | p5engine-like 2D 游戏引擎 | p5engine (ShenyfZero9211) |

**核心原则**：
1. **渐进式暴露复杂度** — Lua 层像 Processing 一样简单，C# 层像专业引擎一样强大
2. **示例驱动开发** — 每个 Phase 必须产出可运行的 Demo
3. **热重载优先** — 修改 Lua 脚本后 1 秒内看到效果
4. **跨平台原生** — Windows/Linux/macOS，单文件发布

### 1.2 技术栈

| 功能 | 库 | 协议 |
|------|-----|------|
| 渲染 | **SkiaSharp** (GPU/CPU 双模) | MIT |
| 窗口/输入 | **SDL2-CS** | MIT |
| 音频 | **OpenAL Soft** (OpenTK.Audio) | MIT |
| 物理 | **Aether.Physics2D** (Farseer 现代分支) | MIT |
| 脚本 | **NLua + KeraLua** | MIT |
| 数学 | System.Numerics | — |
| 目标框架 | **.NET 8** | — |

### 1.3 最终目录结构

```
SpriteForge/
├── SpriteCore/                    # Phase 1: 创意编程框架
│   ├── src/
│   │   ├── Graphics/              # SkiaSharp 渲染封装
│   │   ├── Window/                # SDL2 窗口 + 输入管理
│   │   ├── Math/                  # 向量、矩阵、随机数、噪声
│   │   ├── Time/                  # 游戏循环、计时器、帧率控制
│   │   └── Scripting/             # NLua 运行时 + Processing API 映射
│   ├── SpriteCore.csproj
│   └── tests/
│
├── SpriteEngine/                  # Phase 2: 游戏引擎
│   ├── src/
│   │   ├── Core/                  # 引擎配置、日志、对象池
│   │   ├── Scene/                 # GameObject / Component / Scene 管理
│   │   ├── ECS/                   # 实体组件系统（轻量版）
│   │   ├── Physics/               # Aether.Physics2D 集成
│   │   ├── Audio/                 # OpenAL 音频管理器
│   │   ├── Resources/             # 资源加载器、SPAK 包格式
│   │   ├── UI/                    # UI 框架 + Tween 动画
│   │   └── LuaAPI/                # 游戏级 Lua API（引擎封装）
│   ├── SpriteEngine.csproj
│   └── tests/
│
├── SpriteLauncher/                # 可执行启动器
│   ├── Program.cs
│   └── SpriteLauncher.csproj
│
├── tools/
│   ├── spak-packer/               # SPAK 资源包打包工具 (Python)
│   └── build-scripts/             # 编译/发布 PowerShell 脚本
│
├── samples/                       # 示例集合
│   ├── 01_HelloSprite/            # 最小 Processing-like 示例
│   ├── 02_PhysicsPlayground/      # 物理演示
│   ├── 03_TweenDemo/              # UI + Tween 演示
│   └── 04_MiniGame/               # 完整小游戏（类似 Starfield Rampart）
│
├── docs/
│   ├── API_Reference.md
│   └── Lua_Scripting_Guide.md
│
└── DEVELOPMENT_PLAN.md            # 本文件
```

---

## 2. Phase 1: SpriteCore 核心框架（预计 2-3 周）

**目标**：实现一个能在 Lua 脚本中用 Processing 风格 API 绘制图形、响应输入、播放音频的底层框架。

### Week 1: 窗口 + 渲染 + 游戏循环

#### 任务 1.1: 初始化 .NET 8 项目结构
- [ ] 创建 `SpriteForge.sln`
- [ ] 创建 `SpriteCore` (ClassLib, net8.0)
- [ ] 创建 `SpriteLauncher` (Console, net8.0)
- [ ] 安装 NuGet 包到 SpriteCore: `SkiaSharp`, `SDL2-CS`, `NLua`, `KeraLua`
- [ ] 配置项目引用: SpriteLauncher → SpriteCore

**验收标准**: `dotnet build` 成功，无警告。

#### 任务 1.2: SDL2 窗口与事件循环封装
- [ ] 实现 `Window` 类：创建窗口、处理事件循环
- [ ] 实现 `InputSystem` 类：键盘、鼠标、滚轮状态追踪
- [ ] 实现 `GameTimer` 类：高性能计时、DeltaTime、帧率控制
- [ ] **复用资料**: `/references/SDL2_CS_Development_Guide.md` 中的 `GameTimer`

**验收标准**: 能打开一个 800x600 窗口，按 ESC 退出，控制台打印帧率。

#### 任务 1.3: SkiaSharp + SDL2 GPU 集成
- [ ] 实现 `Renderer` 类：GPU 上下文、SKSurface、SKCanvas 管理
- [ ] 实现 `Present()`：SkiaSharp 渲染结果呈现到 SDL2 窗口
- [ ] 支持窗口大小变化时的表面重建
- [ ] 实现 GPU 失败时的 CPU 回退机制
- [ ] **复用资料**: `/references/SDL2_CS_Development_Guide.md` 中的 `SkiaSDLRenderer`

**验收标准**: 窗口中显示一个旋转的彩色矩形，60fps。

#### 任务 1.4: 游戏循环骨架
- [ ] 实现固定时间步更新（物理预留接口）
- [ ] 实现可变时间步脚本更新
- [ ] 帧率限制与睡眠优化
- [ ] **复用资料**: `/docs/Project_Architecture.md` 中的 `GameLoop`

**验收标准**: 循环稳定 60fps，CPU 占用合理。

### Week 2: Processing API + Lua 集成

#### 任务 2.1: Processing-like 绘图 API (C# 层)
- [ ] 实现 `P5` 静态类（或 `GraphicsAPI`），封装 SkiaSharp：
  - 颜色: `Background`, `Fill`, `Stroke`, `NoFill`, `NoStroke`
  - 形状: `Rect`, `Ellipse`, `Circle`, `Line`, `Triangle`, `Arc`
  - 变换: `PushMatrix`/`PopMatrix`, `Translate`, `Rotate`, `Scale`
  - 文字: `Text`, `TextSize`
  - 图像: `Image`, `LoadImage`
  - 环境: `Width`, `Height`, `FrameCount`, `Millis`, `DeltaTime`
- [ ] 实现绘图状态栈（Fill/Stroke/Transform 隔离）
- [ ] **复用资料**: `/references/NLua_KeraLua_Binding_Guide.md` 中的 `ProcessingAPI` 类

**验收标准**: C# 单元测试验证所有 API 能正确绘制到 SKSurface。

#### 任务 2.2: Lua 运行时集成
- [ ] 封装 `ScriptEngine`：NLua 实例管理、CLR 包加载
- [ ] 将 C# `P5` API 注册为 Lua 全局函数
- [ ] 实现 `setup()` / `draw()` / `update(dt)` 回调调用
- [ ] **复用资料**: `/references/NLua_KeraLua_Binding_Guide.md` 中的注册代码

**验收标准**: 以下 Lua 脚本能正常运行：
```lua
function setup()
    print("Hello SpriteCore!")
end

function draw()
    background(30, 30, 40)
    fill(255, 100, 120)
    circle(width()/2, height()/2, 50)
end
```

#### 任务 2.3: Lua 脚本热重载
- [ ] 实现 `FileSystemWatcher` 监听 `.lua` 文件变化
- [ ] 100ms 防抖，避免文件锁冲突
- [ ] 热重载失败时打印错误但不崩溃
- [ ] **复用资料**: `/references/NLua_KeraLua_Binding_Guide.md` 中的 `LuaScriptManager`

**验收标准**: 修改 Lua 文件保存后，窗口内容在 1 秒内更新。

### Week 3: 数学工具 + 音频 + 完善

#### 任务 3.1: 数学工具库
- [ ] `Vector2` / `Vector3`：运算、归一化、点乘叉乘、插值
- [ ] `Random`：种子控制、范围随机、 Perlin/Simplex 噪声（预留接口）
- [ ] `MathUtils`：弧度角度转换、Clamp、Lerp、Map（Processing 风格）

**验收标准**: Lua 中可调用 `Vector2.new(1,2)` 和 `math.map(value, 0, 10, 0, 1)`。

#### 任务 3.2: 音频系统（SpriteCore 层）
- [ ] 封装 `AudioSystem`：OpenAL 设备/上下文初始化
- [ ] `LoadSound`, `PlaySound`, `StopSound`, `SetVolume`
- [ ] 使用 NAudio 桥接加载 WAV/MP3/OGG
- [ ] **复用资料**: `/references/OpenAL_Audio_Guide.md` 中的 `AudioSystem` + `NAudioOpenALBridge`

**验收标准**: Lua 中 `playSound("bounce.wav")` 能出声。

#### 任务 3.3: 示例 01 — HelloSprite
- [ ] 创建 `samples/01_HelloSprite/main.lua`
- [ ] 展示：绘图、变换、鼠标交互、声音触发
- [ ] 编写 README 说明如何运行

**验收标准**: 新人能在 5 分钟内克隆仓库并跑起示例。

#### 任务 3.4: Phase 1 代码审查与文档
- [ ] 整理 SpriteCore 公开 API 文档
- [ ] 标记所有 `TODO` 和 `HACK`
- [ ] 运行代码格式化 (dotnet format)

---

## 3. Phase 2: SpriteEngine 游戏引擎（预计 3-4 周）

**目标**：在 SpriteCore 之上构建 GameObject/Component/Scene 系统、物理、资源管理、UI，形成完整的 2D 游戏引擎。

### Week 4: 核心架构 + 场景系统

#### 任务 4.1: 初始化 SpriteEngine 项目
- [ ] 创建 `SpriteEngine` (ClassLib, net8.0)
- [ ] 安装 NuGet 包: `Aether.Physics2D`, `OpenTK.Audio.OpenAL`
- [ ] 添加项目引用: SpriteEngine → SpriteCore
- [ ] 创建 `SpriteEngine` 主类（类似 p5engine 的 `P5Engine.create()`）

**验收标准**: `SpriteEngine` 能引用 `SpriteCore` 的 `Renderer` 和 `ScriptEngine`。

#### 任务 4.2: GameObject / Component / Scene
- [ ] `GameObject`：唯一 ID、启用状态、父子层级、Tag
- [ ] `Component` 抽象基类：`Start()`, `Update(dt)`, `OnDestroy()`
- [ ] 内建组件：`Transform` (位置/旋转/缩放), `SpriteRenderer`, `Camera`
- [ ] `Scene`：GameObject 管理、查找（ByName, ByTag, ByType）
- [ ] `SceneManager`：场景切换、加载卸载
- [ ] **参考 p5engine**: `scene/GameObject.java`, `scene/Scene.java`

**验收标准**: C# 代码能创建场景、添加 GameObject、每帧更新所有组件。

#### 任务 4.3: ECS 轻量层（可选增强）
- [ ] `Entity` + `IComponent` + `System` 接口
- [ ] `SpriteRendererSystem`：批量渲染所有带 SpriteRenderer 的实体
- [ ] `SystemManager`：系统注册与执行顺序
- [ ] **复用资料**: `/docs/Project_Architecture.md` 中的 ECS 设计

**验收标准**: 1000 个 Sprite 实体渲染帧率 > 200fps。

### Week 5: 物理集成 + 碰撞

#### 任务 5.1: Aether.Physics2D 封装
- [ ] `PhysicsWorld2D`：封装 Aether World，重力、时间步
- [ ] `RigidBody2D` 组件：Dynamic/Static/Kinematic、质量、摩擦力
- [ ] `BoxCollider2D`, `CircleCollider2D`：形状定义
- [ ] 物理单位 ↔ 像素单位转换（如 1m = 32px）
- [ ] **复用资料**: `/references/2D_Physics_Engine_Guide.md` 中的 Aether 代码

**验收标准**: GameObject 添加 RigidBody2D + BoxCollider2D 后受重力下落并碰撞。

#### 任务 5.2: 碰撞回调与触发器
- [ ] `OnCollisionEnter`, `OnCollisionExit`, `OnTriggerEnter` 回调
- [ ] 碰撞信息封装：接触点、法线、相对速度
- [ ] Lua 层可通过组件接收碰撞事件

**验收标准**: 两个物体碰撞时，Lua `onCollisionEnter(other)` 被调用。

#### 任务 5.3: 示例 02 — PhysicsPlayground
- [ ] 创建 `samples/02_PhysicsPlayground/`
- [ ] 鼠标点击生成随机形状的物理物体
- [ ] 展示：重力、碰撞、反弹、堆叠稳定性

### Week 6: 资源管理 + 音频增强

#### 任务 6.1: 资源管理器
- [ ] `ResourceManager`：统一加载纹理、声音、字体、配置文件
- [ ] 异步加载接口 + 加载进度回调
- [ ] 引用计数自动卸载
- [ ] **参考 p5engine**: `rendering/ImageManager.java`

**验收标准**: 加载 100 张图片内存稳定，重复加载返回缓存实例。

#### 任务 6.2: SPAK 资源包格式
- [ ] 设计 SPAK 格式（类似 p5engine 的 PPAK）：文件头 + 文件表 + 数据块
- [ ] 实现 C# `SpakArchive`：读取、解压、按需加载
- [ ] 实现 Python `spak_pack.py` 打包工具
- [ ] 支持 zlib 压缩
- [ ] **参考 p5engine**: `tools/ppak/ppak_pack.py`

**验收标准**: `data/` 目录打包为 `data.spak` 后，游戏从包内正常读取资源。

#### 任务 6.3: 音频管理器（引擎层）
- [ ] `AudioManager`：按名称管理音效和音乐
- [ ] 2D 空间音频：音源位置、听者位置、音量衰减
- [ ] 音频混合：背景音乐 + 音效通道独立音量
- [ ] **复用资料**: `/references/OpenAL_Audio_Guide.md` 中的 `GameAudioManager`

**验收标准**: Lua 中 `Audio.playMusic("bgm.ogg", 0.5)` 循环播放，`Audio.playSfx("shoot.wav")` 叠加播放。

### Week 7: UI 框架 + Tween 动画

#### 任务 7.1: UI 框架
- [ ] `UIElement` 基类：位置、大小、锚点、层级
- [ ] 内建控件：`UIImage`, `UILabel`, `UIButton`, `UIPanel`
- [ ] 事件系统：`onClick`, `onHover`, `onValueChanged`
- [ ] UI 坐标系：屏幕空间 + 世界空间（如血条跟随角色）
- [ ] **参考 p5engine**: `ui/` 包

**验收标准**: 能创建一个带背景图和文字的标签，点击变色。

#### 任务 7.2: Tween 动画系统
- [ ] `Tween`：对任意数值属性做动画（位置、旋转、透明度、颜色）
- [ ] 缓动函数：Linear, EaseIn/Out, Elastic, Bounce
- [ ] 序列/并行组合：`Sequence().Append().Join()`
- [ ] Lua API：`Tween.to(target, {x=100}, 1.0, "easeOutQuad")`

**验收标准**: 一个 UI 按钮在 0.5 秒内从屏幕外弹性滑入。

#### 任务 7.3: 示例 03 — TweenDemo
- [ ] 展示 UI 布局、Tween 动画、按钮交互
- [ ] 展示场景切换过渡动画

### Week 8: Lua 游戏级 API + 引擎整合

#### 任务 8.1: 游戏级 Lua API
封装面向对象 API，让 Lua 脚本更像在操作游戏引擎：
- [ ] `Engine.createGameObject(name)` — 创建空对象
- [ ] `go:addComponent("SpriteRenderer", {texture="player.png"})`
- [ ] `go:getComponent("Transform").position = Vector2.new(100, 200)`
- [ ] `Scene.load("level1")`
- [ ] `Input.getKey("space")`, `Input.getAxis("horizontal")`
- [ ] `GameObject.find("Player")`, `GameObject.findWithTag("Enemy")`
- [ ] **复用资料**: `/docs/Project_Architecture.md` 中的 Lua 示例

**验收标准**: 以下 Lua 脚本能完整运行：
```lua
function setup()
    player = Engine.createGameObject("Player")
    player:addComponent("SpriteRenderer", { texture = "player.png" })
    player:addComponent("RigidBody2D", { type = "dynamic" })
    player:addComponent("BoxCollider2D", { width = 32, height = 32 })
end

function update(dt)
    if Input.getKey("a") then
        player:getComponent("Transform").position.x = 
            player:getComponent("Transform").position.x - 200 * dt
    end
end
```

#### 任务 8.2: 引擎配置系统
- [ ] `EngineConfig`：窗口标题/大小、目标帧率、物理参数、音频音量
- [ ] 支持 YAML/JSON 配置文件
- [ ] 命令行参数覆盖：`--width 1920 --fullscreen --debug`

**验收标准**: `config.yaml` 修改窗口标题后重启生效。

#### 任务 8.3: 日志与调试
- [ ] `Log` 系统：Debug/Info/Warning/Error 分级
- [ ] 运行时统计：FPS、DrawCall、物理体数量、Lua 内存
- [ ] Debug 绘制：碰撞体轮廓、刚体速度向量、场景边界

---

## 4. Phase 3: 工具链与示例游戏（预计 2-3 周）

### Week 9: 构建脚本 + 项目模板

#### 任务 9.1: PowerShell 构建脚本
- [ ] `build-engine.ps1`：编译 SpriteCore + SpriteEngine
- [ ] `build-sample.ps1`：编译示例 + 打包资源
- [ ] `build-release.ps1`：完整发布流程（编译 → SPAK → 单文件发布）
- [ ] **参考 p5engine**: `scripts/build-release.ps1`, `compile-jar.ps1`

**验收标准**: 一条命令 `.uild-release.ps1 -Sample 04_MiniGame` 产出可执行文件夹。

#### 任务 9.2: 项目模板
- [ ] `dotnet new` 自定义模板：`dotnet new spritegame -n MyGame`
- [ ] 模板包含：基本的 `main.lua`, `config.yaml`, 空 `assets/` 目录
- [ ] VS Code 配置：launch.json, tasks.json, 代码片段

**验收标准**: 开发者运行 `dotnet new spritegame` 后立刻能 `dotnet run` 看到黑窗口。

### Week 10-11: 示例游戏 04 — SpriteGuard（星域防线风格）

**目标**：开发一个类似 p5engine 的 Starfield Rampart 的完整塔防/射击小游戏，验证引擎所有功能。

#### 任务 10.1: 游戏设计文档
- [ ] 核心玩法：太空射击 + 基地防御
- [ ] 角色：玩家飞船、3 种敌人、2 种防御塔
- [ ] 机制：移动、射击、建造、升级、波次生成
- [ ] UI：主菜单、HUD、建造面板、暂停菜单、GameOver

#### 任务 10.2: 游戏实现
- [ ] `PlayerController.lua`：WASD 移动，鼠标瞄准射击
- [ ] `EnemySpawner.lua`：波次配置，随时间难度递增
- [ ] `Tower.lua`：自动索敌， projectile 发射
- [ ] `Projectile.lua`：直线飞行，碰撞检测，粒子爆炸
- [ ] `GameManager.lua`：分数、金钱、生命值、状态机

#### 任务 10.3: 美术与音效占位
- [ ] 使用几何图形 + 程序生成视觉（无需专业美术）
- [ ] 粒子系统：爆炸、尾焰、建造特效
- [ ] 使用免费音效占位（freesound.org）

**验收标准**: 
- 游戏可完整游玩 5 分钟以上
- 无崩溃，帧率稳定 60fps
- 可作为引擎能力的最佳展示

### Week 12: 文档与 Polish

#### 任务 12.1: 开发者文档
- [ ] `docs/Lua_Scripting_Guide.md`：所有 Lua API 速查
- [ ] `docs/API_Reference.md`：C# 公开 API 文档
- [ ] `docs/Architecture.md`：引擎架构说明
- [ ] 每个示例的 README

#### 任务 12.2: 性能基线测试
- [ ] 1000 个 GameObject 更新测试
- [ ] 100 个物理体碰撞测试
- [ ] 内存泄漏检测（长时间运行）

---

## 5. Phase 4: 优化迭代（持续）

### 渲染优化
- [ ] **批处理渲染**：合并相同材质的 DrawCall
- [ ] **纹理图集 (SpriteAtlas)**：减少纹理切换
- [ ] **视锥剔除**：只渲染屏幕内的 Sprite
- [ ] **GPU Instancing**：大量相同纹理的物体

### 物理优化
- [ ] **空间分割**：四叉树/BroadPhase 优化（Aether 自带，验证配置）
- [ ] **物理层级**：不同碰撞矩阵，减少无意义检测

### 脚本优化
- [ ] **Lua 对象池**：减少 GC 压力
- [ ] **批量 API**：一次性传递多个坐标而非逐点调用
- [ ] **Coroutine 支持**：Lua 协程用于动画序列、延迟调用

### 工具链增强
- [ ] 简易场景编辑器（基于 ImGui 或纯 C# WinForms/WPF）
- [ ] 粒子编辑器
- [ ] 动画编辑器（关键帧 Tween）

---

## 6. 风险与应对

| 风险 | 影响 | 应对策略 |
|------|------|----------|
| SkiaSharp GPU 在某些显卡上初始化失败 | 高 | 优先实现 CPU 回退，自动检测并切换 |
| SDL2 原生库分发复杂 | 中 | 使用 `ppy.SDL2-CS` NuGet（含原生库），或自备 `SDL2.dll` |
| NLua 与 .NET 8 兼容性问题 | 中 | 使用最新版 NLua，若有问题换 MoonSharp（纯 C# Lua） |
| Aether.Physics2D 单位转换踩坑 | 中 | 早期建立严格的 Meters↔Pixels 转换层，写测试保护 |
| 热重载导致 Lua 状态污染 | 低 | 热重载时完整重置 Lua 状态，保留全局配置表 |

---

## 7. 可复用资料清单

开发时请直接参考以下文档中的代码片段：

| 资料文件 | 可直接复用的代码 |
|----------|-----------------|
| `docs/Project_Architecture.md` | GameLoop 骨架、P5 API 映射、ECS 设计、Lua 游戏脚本示例 |
| `references/SDL2_CS_Development_Guide.md` | SkiaSDLRenderer (GPU 集成)、GameTimer、输入事件处理 |
| `references/NLua_KeraLua_Binding_Guide.md` | ProcessingAPI 类（完整实现）、LuaScriptManager（热重载） |
| `references/OpenAL_Audio_Guide.md` | AudioSystem、NAudioOpenALBridge、GameAudioManager |
| `references/2D_Physics_Engine_Guide.md` | Aether.Physics2D 封装、碰撞回调、物理渲染集成 |
| `references/SkiaSharp_API_QuickReference.md` | API 速查、GPU 上下文创建、动画范式 |

---

## 8. 里程碑检查点

| 检查点 | 时间 | 标志性成果 |
|--------|------|-----------|
| **M1** | Phase 1 结束 | 运行 `01_HelloSprite`，Lua 热重载可用 |
| **M2** | Phase 2 中途 | 100 个物理方块碰撞，帧率稳定 |
| **M3** | Phase 2 结束 | Lua 脚本能完整控制 GameObject + 物理 + 音频 |
| **M4** | Phase 3 结束 | 发布 `SpriteGuard` 小游戏，单文件夹可运行 |
| **M5** | Phase 4 持续 | 绘制 Call < 100 时 1000+ Sprite 稳定 60fps |

---

## 9. 下一步行动

1. **立即执行**：按 Phase 1 Week 1 任务 1.1 创建项目结构
2. **本周目标**：窗口打开 + 旋转矩形 + 按 ESC 退出
3. **首个 Demo**：一周内让 `samples/01_HelloSprite/main.lua` 跑起来

---

*Plan Created: 2026-05-28*  
*Next Review: Phase 1 结束时*
