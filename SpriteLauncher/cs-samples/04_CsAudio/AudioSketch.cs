using SpriteCore.Graphics;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// 音频演示：点击鼠标播放 beep 音效，背景音乐循环播放。
/// 对应 Lua 示例：04_AudioTest
/// </summary>
public class AudioSketch : Sketch
{
    private readonly string _dataDir = "E:/projects/kimi/data/";

    public override void Setup()
    {
        Size(800, 600);
        Background(20, 20, 30);

        LoadSound("beep", _dataDir + "beep.wav");
        LoadSound("bgm", _dataDir + "bgm.wav");

        PlayMusic("bgm", 0.3f);
    }

    public override void Update(float dt)
    {
        if (MouseIsPressed)
        {
            PlaySound("beep", 0.8f);
        }
    }

    public override void Draw()
    {
        Background(20, 20, 30);

        Fill(255);
        TextSize(20);
        Text("C# Audio Test", 20, 30);

        Fill(200, 200, 200);
        TextSize(14);
        Text("BGM is playing (loop, volume 0.3)", 20, 60);
        Text("Click mouse to play 'beep' sound", 20, 80);

        if (MouseIsPressed)
        {
            Fill(255, 100, 100);
            Circle(MouseX, MouseY, 30);
        }

        Fill(100, 200, 255);
        Text($"Mouse: ({MouseX:F0}, {MouseY:F0})", 20, Height - 40);
        Text($"Pressed: {MouseIsPressed}", 20, Height - 20);
    }
}
