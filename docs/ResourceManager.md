# ResourceManager 资源管理器

## 概述

`ResourceManager` 是 SpriteEngine 的引用计数式资源管理器，设计参考 libgdx 的 `AssetManager`。核心目标：

- **统一管理**：所有纹理、音频、模型等资源通过单一入口加载和释放。
- **引用计数**：同一资源被多个对象共享时，避免重复加载和过早释放。
- **线程安全**：同步/异步加载均内部加锁，可在多线程环境安全使用。
- **与 SpriteCore 解耦**：引擎层 (`Texture2D`) 包装核心层 (`SPTexture`)，职责清晰。

## 架构

```
┌─────────────────┐     Load<T>()      ┌──────────────────┐
│  PhysicsPlayground│ ─────────────────> │ ResourceManager  │
│  (Game / Demo)   │     Get<T>()       │  - _resources    │
│                  │     Unload()       │  - _refCounts    │
│  SpriteRenderer  │ <───────────────── │  - _lock         │
│  .Texture        │   Texture2D        └──────────────────┘
└─────────────────┘         │
                            │ wraps
                            ▼
                    ┌──────────────────┐
                    │   SPTexture      │  ← SpriteCore 层
                    │   (SKBitmap)     │
                    └──────────────────┘
```

## 核心 API

### 加载

```csharp
var rm = new ResourceManager();
var tex = rm.Load<Texture2D>("path/to/image.png");
```

- 首次加载：解码文件 → 创建 `Texture2D` → 加入缓存 → 引用计数 = 1
- 重复加载：返回缓存实例 → 引用计数 +1

### 异步加载

```csharp
var tex = await rm.LoadAsync<Texture2D>("path/to/image.png");
```

后台线程执行文件 IO，完成后加入缓存。线程安全通过 `lock` 保证。

### 获取（不增加引用）

```csharp
var tex = rm.Get<Texture2D>("path/to/image.png");
if (tex != null) { /* use */ }
```

### 卸载

```csharp
rm.Unload("path/to/image.png");
// 或
rm.Unload(tex);
```

引用计数 -1，归零时调用 `Dispose()` 并从缓存移除。

### 强制清空

```csharp
rm.DisposeAll();
```

无视引用计数，释放所有资源。适用于场景切换时的强制清理。

## 引用计数规则

| 操作 | 引用计数变化 | 结果 |
|------|-------------|------|
| `Load<T>()` (首次) | 0 → 1 | 创建并缓存 |
| `Load<T>()` (重复) | +1 | 返回现有实例 |
| `Unload()` | -1 | 若归零则释放 |
| `DisposeAll()` | 忽略 | 全部释放 |

## 与 SpriteRenderer 的集成

```csharp
var renderer = go.AddComponent<SpriteRenderer>();
renderer.Texture = rm.Load<Texture2D>("ruins_0.png");
renderer.Width = 64;
renderer.Height = 64;
```

`SpriteRenderer.Render()` 逻辑：
- `Texture != null` → 绘制纹理（拉伸到 Width × Height）
- `Texture == null` → 绘制纯色矩形（原有行为）

## 与 SPTexture 的关系

| 层级 | 类 | 职责 |
|------|-----|------|
| SpriteCore | `SPTexture` | 文件解码、SkiaSharp 位图包装、弱引用缓存 |
| SpriteEngine | `Texture2D` | 实现 `IResource`、引用计数生命周期、引擎语义 |

`SPTexture` 的弱引用缓存避免同一路径在内存中重复解码；`ResourceManager` 的强引用缓存保证引擎对象持有期间纹理不被 GC 回收。

## 线程安全

所有公共方法内部使用 `lock (_lock)` 保护 `_resources` 和 `_refCounts`。异步加载使用 `Task.Run` 在后台线程执行文件 IO，完成后在 `lock` 内更新缓存。

## 扩展资源类型

当前仅支持 `Texture2D`。扩展步骤：

1. 实现 `IResource` 接口
2. 在 `ResourceManager.CreateResource<T>()` 中添加分支
3. 可选：添加异步预加载逻辑

## 测试覆盖

- `Load_Texture2D_ReturnsValidTexture`：基本加载
- `Load_SamePathTwice_ReturnsSameInstance`：缓存命中
- `Unload_RefCountReachesZero_RemovesFromCache`：引用计数归零释放
- `Unload_RefCountStillPositive_KeepsInCache`：部分释放保留
- `Get_NotLoaded_ReturnsNull`：未加载查询
- `DisposeAll_ClearsEverything`：强制清空
- `LoadAsync_Texture2D_ReturnsValidTexture`：异步加载

全部 74 个 SpriteEngine 测试通过。
