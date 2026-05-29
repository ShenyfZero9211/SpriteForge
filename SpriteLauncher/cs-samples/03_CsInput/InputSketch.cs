using SpriteCore.Graphics;
using SDL2;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// 输入演示：键盘控制方块移动，鼠标留下轨迹。
/// 对应 Lua 示例：03_InputDemo
/// </summary>
public class InputSketch : Sketch
{
    private float _px = 400;
    private float _py = 300;
    private float _speed = 5;
    private readonly List<(float x, float y, float alpha)> _trail = new();
    private const int MaxTrail = 60;

    public override void Setup()
    {
        Size(800, 600);
        Background(10, 10, 15);
    }

    public override void Update(float dt)
    {
        // 键盘控制
        if (IsKeyPressed((int)SDL.SDL_Keycode.SDLK_LEFT) || IsKeyPressed((int)SDL.SDL_Keycode.SDLK_a))
            _px -= _speed;
        if (IsKeyPressed((int)SDL.SDL_Keycode.SDLK_RIGHT) || IsKeyPressed((int)SDL.SDL_Keycode.SDLK_d))
            _px += _speed;
        if (IsKeyPressed((int)SDL.SDL_Keycode.SDLK_UP) || IsKeyPressed((int)SDL.SDL_Keycode.SDLK_w))
            _py -= _speed;
        if (IsKeyPressed((int)SDL.SDL_Keycode.SDLK_DOWN) || IsKeyPressed((int)SDL.SDL_Keycode.SDLK_s))
            _py += _speed;

        // 边界限制
        _px = Clamp(_px, 20, Width - 20);
        _py = Clamp(_py, 20, Height - 20);

        // 鼠标轨迹
        if (MouseIsPressed)
        {
            _trail.Add((MouseX, MouseY, 255));
        }
        if (_trail.Count > MaxTrail)
        {
            _trail.RemoveAt(0);
        }
        // 衰减
        for (int i = 0; i < _trail.Count; i++)
        {
            var t = _trail[i];
            _trail[i] = (t.x, t.y, t.alpha - 3);
        }
        _trail.RemoveAll(t => t.alpha <= 0);
    }

    public override void Draw()
    {
        Background(10, 10, 15);

        // 绘制轨迹
        for (int i = 0; i < _trail.Count; i++)
        {
            var t = _trail[i];
            float size = 5 + i * 0.5f;
            Fill(100, 200, 255, t.alpha);
            Circle(t.x, t.y, size);
        }

        // 玩家方块
        Fill(255, 100, 100);
        Rect(_px - 15, _py - 15, 30, 30);

        // 鼠标位置
        Fill(255, 255, 0);
        Circle(MouseX, MouseY, 8);

        // 信息
        Fill(200);
        TextSize(14);
        Text($"Player: ({_px:F0}, {_py:F0})", 20, 30);
        Text($"Mouse: ({MouseX:F0}, {MouseY:F0})", 20, 50);
        Text("WASD / Arrow keys to move", 20, 70);
        Text("Click & drag for trail", 20, 90);
        Text("ESC to exit", 20, 110);
    }
}
