using SDL2;

namespace SpriteCore.Time;

public class GameTimer
{
    private ulong _lastTime;
    private ulong _currentTime;
    private ulong _frequency;

    public float DeltaTime { get; private set; }
    public float TotalTime { get; private set; }
    public int FrameCount { get; private set; }
    public float Fps { get; private set; }

    private float _fpsAccumulator;
    private int _fpsFrames;

    public GameTimer()
    {
        _frequency = SDL.SDL_GetPerformanceFrequency();
        _lastTime = SDL.SDL_GetPerformanceCounter();
    }

    public void Update()
    {
        _currentTime = SDL.SDL_GetPerformanceCounter();
        var freq = (double)_frequency;

        if (_lastTime != 0)
        {
            DeltaTime = (float)((_currentTime - _lastTime) / freq);
        }
        else
        {
            DeltaTime = 1.0f / 60.0f;
        }

        TotalTime += DeltaTime;
        FrameCount++;
        _lastTime = _currentTime;

        _fpsAccumulator += DeltaTime;
        _fpsFrames++;
        if (_fpsAccumulator >= 1.0f)
        {
            Fps = _fpsFrames / _fpsAccumulator;
            _fpsAccumulator = 0;
            _fpsFrames = 0;
        }
    }

    public void CapFrameRate(int targetFps)
    {
        float frameTime = 1.0f / targetFps;
        if (DeltaTime < frameTime)
        {
            uint delayMs = (uint)((frameTime - DeltaTime) * 1000);
            if (delayMs > 0)
                SDL.SDL_Delay(delayMs);
        }
    }
}
