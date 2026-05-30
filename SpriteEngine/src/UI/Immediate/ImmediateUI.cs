using SkiaSharp;
using SpriteEngine.Scenes;

namespace SpriteEngine.UI;

/// <summary>
/// 即时模式 Debug Overlay API。受 Dear ImGui 启发，通过 <see cref="UIDrawList"/> 绘制调试信息，
/// 不依赖 Retained UI 树。适合 FPS 计数器、内存统计、日志输出等临时覆盖层。
/// </summary>
public static class ImmediateUI
{
    /// <summary>是否显示 Debug Overlay。需外部检测按键并设置此值。</summary>
    public static bool ShowDebugOverlay { get; set; } = false;

    /// <summary>
    /// 绘制 Debug 信息面板到左上角。
    /// </summary>
    /// <param name="drawList">渲染原语缓冲</param>
    /// <param name="fps">当前帧率</param>
    /// <param name="dt">帧间隔（秒）</param>
    /// <param name="scene">当前场景（可选，用于显示 GameObject 数量）</param>
    public static void DrawDebugPanel(UIDrawList drawList, float fps, float dt, Scene? scene = null)
    {
        if (!ShowDebugOverlay) return;

        float pad = 10;
        float lineH = 16;
        float x = pad;
        float y = pad;
        float w = 200;
        float rowCount = scene != null ? 5 : 4;
        float h = rowCount * lineH + pad * 2;

        // 背景
        drawList.AddRectFilled(x, y, w, h, new SKColor(0, 0, 0, 180), 4);
        drawList.AddRect(x, y, w, h, new SKColor(60, 60, 70), 1, 4);

        float tx = x + pad;
        float ty = y + pad + 11;
        float fontSize = 12;

        var labelColor = new SKColor(160, 160, 170);
        var valueColor = new SKColor(220, 220, 230);
        var fpsColor = fps < 30 ? new SKColor(255, 100, 100) : new SKColor(100, 255, 150);

        // FPS
        drawList.AddText("FPS: ", tx, ty, labelColor, fontSize);
        drawList.AddText($"{fps:F1}", tx + 40, ty, fpsColor, fontSize);
        ty += lineH;

        // Frame Time
        drawList.AddText("MS:  ", tx, ty, labelColor, fontSize);
        drawList.AddText($"{dt * 1000:F2}", tx + 40, ty, valueColor, fontSize);
        ty += lineH;

        // GameObjects
        if (scene != null)
        {
            drawList.AddText("Objs: ", tx, ty, labelColor, fontSize);
            int count = scene.AllObjects.Count();
            drawList.AddText($"{count}", tx + 40, ty, valueColor, fontSize);
            ty += lineH;
        }

        // Memory
        drawList.AddText("Mem: ", tx, ty, labelColor, fontSize);
        var mem = GC.GetTotalMemory(false) / 1024 / 1024f;
        drawList.AddText($"{mem:F1} MB", tx + 40, ty, valueColor, fontSize);
        ty += lineH;

        // Hint
        drawList.AddText("[F12] Toggle", tx, ty, new SKColor(120, 120, 130), fontSize);
    }

    /// <summary>
    /// 绘制所有 GameObject 的简化包围盒（调试用）。
    /// </summary>
    public static void DrawGameObjectBounds(UIDrawList drawList, Scene scene, SKColor? color = null)
    {
        if (!ShowDebugOverlay) return;

        var c = color ?? new SKColor(100, 200, 255, 120);
        foreach (var go in scene.AllObjects)
        {
            if (go == null) continue;
            float bx = go.Transform.Position.X - 16;
            float by = go.Transform.Position.Y - 16;
            drawList.AddRect(bx, by, 32, 32, c, 1, 0);
        }
    }

    /// <summary>
    /// 绘制底部日志覆盖层。
    /// </summary>
    public static void DrawLogOverlay(UIDrawList drawList, float screenW, float screenH, IEnumerable<string> lines, int maxLines = 5)
    {
        if (!ShowDebugOverlay) return;

        float lineH = 14;
        float pad = 8;
        var visible = lines.TakeLast(maxLines).ToList();
        float h = visible.Count * lineH + pad * 2;
        float y = screenH - h;

        drawList.AddRectFilled(0, y, screenW, h, new SKColor(0, 0, 0, 160), 0);

        float ty = y + pad + 10;
        foreach (var line in visible)
        {
            drawList.AddText(line, pad, ty, new SKColor(180, 180, 190), 11);
            ty += lineH;
        }
    }
}
