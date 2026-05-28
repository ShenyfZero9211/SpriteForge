using OpenTK.Audio.OpenAL;
using SpriteCore.Utils;

namespace SpriteCore.Audio;

public class AudioSystem : IDisposable
{
    private ALDevice _device;
    private ALContext _context;
    private readonly Dictionary<string, int> _soundBuffers = new();
    private readonly List<ActiveSource> _activeSources = new();
    private float _masterVolume = 1.0f;

    private class ActiveSource
    {
        public int SourceId;
        public string Name = "";
        public bool IsMusic;
    }

    public void Initialize()
    {
        _device = ALC.OpenDevice(null);
        if (_device.Handle == IntPtr.Zero)
        {
            Log.Error("Audio", "Failed to open OpenAL audio device");
            throw new Exception("Failed to open OpenAL audio device");
        }

        _context = ALC.CreateContext(_device, (int[])null!);
        ALC.MakeContextCurrent(_context);

        AL.Listener(ALListenerf.Gain, _masterVolume);
    }

    public void LoadSound(string name, string filePath)
    {
        if (_soundBuffers.ContainsKey(name))
        {
            Log.Debug("Audio", $"Sound '{name}' already loaded, skipping");
            return;
        }

        Log.Info("Audio", $"Loading sound '{name}' from {filePath}");

        var (data, channels, sampleRate, bitsPerSample) = LoadWav(filePath);
        var format = GetAlFormat(channels, bitsPerSample);

        int buffer = AL.GenBuffer();
        AL.BufferData(buffer, format, data, sampleRate);
        _soundBuffers[name] = buffer;
        Log.Info("Audio", $"Sound '{name}' loaded ({channels}ch, {sampleRate}Hz, {bitsPerSample}bit)");
    }

    public void PlaySound(string name)
    {
        PlaySoundInternal(name, 1.0f, false);
    }

    public void PlaySound(string name, float volume)
    {
        PlaySoundInternal(name, volume, false);
    }

    public void PlaySound(string name, float volume, bool loop)
    {
        PlaySoundInternal(name, volume, loop);
    }

    private void PlaySoundInternal(string name, float volume, bool loop)
    {
        if (!_soundBuffers.TryGetValue(name, out int buffer))
        {
            Log.Warning("Audio", $"Sound '{name}' not found, cannot play");
            return;
        }

        Log.Debug("Audio", $"Playing '{name}' (vol={volume:F2}, loop={loop})");

        int source = AL.GenSource();
        AL.Source(source, ALSourcei.Buffer, buffer);
        AL.Source(source, ALSourcef.Gain, volume * _masterVolume);
        AL.Source(source, ALSourceb.Looping, loop);
        AL.SourcePlay(source);

        _activeSources.Add(new ActiveSource { SourceId = source, Name = name, IsMusic = loop });
        Log.Debug("Audio", $"Source {source} started for '{name}'");
    }

    public void PlayMusic(string name)
    {
        PlaySoundInternal(name, 0.5f, true);
    }

    public void PlayMusic(string name, float volume)
    {
        PlaySoundInternal(name, volume, true);
    }

    public void StopAll()
    {
        foreach (var s in _activeSources)
        {
            AL.SourceStop(s.SourceId);
            AL.DeleteSource(s.SourceId);
        }
        _activeSources.Clear();
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = volume < 0f ? 0f : volume > 1f ? 1f : volume;
        AL.Listener(ALListenerf.Gain, _masterVolume);
    }

    public void Update()
    {
        for (int i = _activeSources.Count - 1; i >= 0; i--)
        {
            AL.GetSource(_activeSources[i].SourceId, ALGetSourcei.SourceState, out int state);
            if (state == (int)ALSourceState.Stopped && !_activeSources[i].IsMusic)
            {
                AL.DeleteSource(_activeSources[i].SourceId);
                _activeSources.RemoveAt(i);
            }
        }
    }

    public void Dispose()
    {
        StopAll();
        foreach (var b in _soundBuffers.Values)
            AL.DeleteBuffer(b);
        _soundBuffers.Clear();
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);
    }

    private static ALFormat GetAlFormat(int channels, int bits)
    {
        return channels switch
        {
            1 => bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16,
            _ => bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16,
        };
    }

    private static (byte[] data, int channels, int sampleRate, int bits) LoadWav(string path)
    {
        using var fs = File.OpenRead(path);
        using var reader = new BinaryReader(fs);

        // RIFF header
        var riff = reader.ReadChars(4);
        if (new string(riff) != "RIFF")
            throw new Exception("Not a WAV file (no RIFF header)");

        reader.ReadInt32(); // file size
        var wave = reader.ReadChars(4);
        if (new string(wave) != "WAVE")
            throw new Exception("Not a WAV file (no WAVE header)");

        int channels = 0, sampleRate = 0, bitsPerSample = 0;
        byte[] audioData = Array.Empty<byte>();

        while (fs.Position < fs.Length)
        {
            var chunkId = reader.ReadChars(4);
            var chunkSize = reader.ReadInt32();
            var id = new string(chunkId);

            if (id == "fmt ")
            {
                reader.ReadInt16(); // audio format (1 = PCM)
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byte rate
                reader.ReadInt16(); // block align
                bitsPerSample = reader.ReadInt16();
                // skip any extra fmt bytes
                if (chunkSize > 16)
                    reader.ReadBytes(chunkSize - 16);
            }
            else if (id == "data")
            {
                audioData = reader.ReadBytes(chunkSize);
            }
            else
            {
                reader.ReadBytes(chunkSize);
            }
        }

        if (audioData.Length == 0)
            throw new Exception("WAV file has no audio data");

        return (audioData, channels, sampleRate, bitsPerSample);
    }
}
