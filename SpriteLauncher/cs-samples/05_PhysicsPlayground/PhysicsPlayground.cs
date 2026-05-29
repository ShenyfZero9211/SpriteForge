using SkiaSharp;
using SpriteCore.Graphics;
using SpriteCore.Math;
using SpriteEngine.Physics;
using SpriteEngine.Resource;
using SpriteEngine.Scenes;

namespace SpriteLauncher.CsSamples;

/// <summary>
/// Aether.Physics2D 物理集成验证 Demo：
/// 鼠标点击生成随机形状，受重力下落并与地面/其他物体碰撞堆叠。
/// </summary>
public class PhysicsPlayground : Sketch
{
    private Scene _scene = null!;
    private float _accumulator;
    private const float FixedDt = 1f / 60f;
    private bool _mouseWasPressed;
    private readonly ResourceManager _resources = new();
    private readonly List<string> _texturePaths = new();
    private readonly SKColor[] _palette =
    {
        new SKColor(255, 80, 80),
        new SKColor(80, 255, 120),
        new SKColor(80, 160, 255),
        new SKColor(255, 200, 80),
        new SKColor(200, 80, 255),
        new SKColor(80, 255, 255),
    };

    public override void Setup()
    {
        Size(800, 600);
        SP5.Width = 800;
        SP5.Height = 600;

        _scene = new Scene("PhysicsPlayground");
        _scene.PhysicsWorld.Gravity = new Vector2(0, 500);

        // 扫描可用纹理
        var texDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "textures", "ruins");
        if (Directory.Exists(texDir))
        {
            _texturePaths.AddRange(Directory.GetFiles(texDir, "*.png"));
        }

        CreateWalls();
    }

    /// <summary>
    /// 碰撞闪烁组件：物体碰撞时瞬间变白，0.1 秒后恢复。
    /// 展示 OnCollisionEnter 回调的实际用法。
    /// </summary>
    private class CollisionFlash : Component
    {
        private SpriteRenderer? _renderer;
        private SKColor _originalColor;
        private float _timer;

        public override void Start()
        {
            _renderer = GameObject?.GetComponent<SpriteRenderer>();
            if (_renderer != null)
                _originalColor = _renderer.Color;
        }

        public override void OnCollisionEnter(Collision2D collision)
        {
            if (_renderer != null)
            {
                _renderer.Color = SKColors.White;
                _timer = 0.1f;
            }
        }

        public override void Update(float dt)
        {
            if (_timer > 0)
            {
                _timer -= dt;
                if (_timer <= 0 && _renderer != null)
                    _renderer.Color = _originalColor;
            }
        }
    }

    private void CreateWalls()
    {
        // 地面
        var ground = _scene.CreateGameObject("Ground");
        ground.Transform.LocalPosition = new Vector2(Width / 2, Height - 16);
        ground.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Static });
        ground.AddComponent(new BoxCollider2D { Width = Width, Height = 32 });
        var groundRenderer = ground.AddComponent<SpriteRenderer>();
        groundRenderer.Color = new SKColor(60, 60, 80);
        groundRenderer.Width = Width;
        groundRenderer.Height = 32;
        groundRenderer.DrawStroke = true;
        groundRenderer.StrokeColor = new SKColor(100, 100, 130);
        groundRenderer.StrokeWeight = 2;

        // 左墙
        var leftWall = _scene.CreateGameObject("LeftWall");
        leftWall.Transform.LocalPosition = new Vector2(-16, Height / 2);
        leftWall.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Static });
        leftWall.AddComponent(new BoxCollider2D { Width = 32, Height = Height });

        // 右墙
        var rightWall = _scene.CreateGameObject("RightWall");
        rightWall.Transform.LocalPosition = new Vector2(Width + 16, Height / 2);
        rightWall.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Static });
        rightWall.AddComponent(new BoxCollider2D { Width = 32, Height = Height });
    }

    public override void Update(float dt)
    {
        _accumulator += dt;
        while (_accumulator >= FixedDt)
        {
            _scene.FixedUpdate(FixedDt);
            _accumulator -= FixedDt;
        }
        _scene.Update(dt);

        // 鼠标点击生成
        if (MouseIsPressed && !_mouseWasPressed)
            SpawnObject(MouseX, MouseY);
        _mouseWasPressed = MouseIsPressed;

        // R 键重置
        if (IsKeyPressed((int)'r'))
        {
            var toRemove = _scene.AllObjects
                .Where(go => go.Name != "Ground" && go.Name != "LeftWall" && go.Name != "RightWall")
                .ToList();
            foreach (var go in toRemove)
                _scene.RemoveGameObject(go);
        }
    }

    public override void Draw()
    {
        Background(15, 15, 25);

        if (SP5.Graphics != null)
            _scene.Render(SP5.Graphics);

        // HUD
        Fill(200);
        TextSize(14);
        Text($"Objects: {_scene.AllObjects.Count() - 3}", 20, 30);
        Text("Click to spawn", 20, 50);
        Text("Press R to reset", 20, 70);
    }

    private void SpawnObject(float x, float y)
    {
        var go = _scene.CreateGameObject($"Object_{_scene.AllObjects.Count()}");
        go.Transform.LocalPosition = new Vector2(x, y);

        var rb = go.AddComponent(new RigidBody2D { BodyType = RigidBodyType.Dynamic });
        rb.LinearDamping = 0.05f;
        rb.AngularDamping = 0.05f;

        var color = _palette[(int)Random(0, _palette.Length)];
        bool isBox = Random(0f, 1f) > 0.5f;
        bool hasTexture = _texturePaths.Count > 0 && Random(0f, 1f) > 0.3f;

        if (isBox)
        {
            float w = Random(20, 60);
            float h = Random(20, 60);
            go.AddComponent(new BoxCollider2D { Width = w, Height = h, Friction = 0.4f, Restitution = 0.2f });
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.Color = color;
            renderer.Width = w;
            renderer.Height = h;
            renderer.DrawStroke = true;
            renderer.StrokeColor = SKColors.White;
            renderer.StrokeWeight = 2;
            if (hasTexture)
                renderer.Texture = _resources.Load<Texture2D>(_texturePaths[(int)Random(0, _texturePaths.Count)]);
        }
        else
        {
            float r = Random(12, 30);
            go.AddComponent(new CircleCollider2D { Radius = r, Friction = 0.4f, Restitution = 0.3f });
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.Color = color;
            renderer.Width = r * 2;
            renderer.Height = r * 2;
            renderer.DrawStroke = true;
            renderer.StrokeColor = SKColors.White;
            renderer.StrokeWeight = 2;
            if (hasTexture)
                renderer.Texture = _resources.Load<Texture2D>(_texturePaths[(int)Random(0, _texturePaths.Count)]);
        }

        // 碰撞闪烁特效
        go.AddComponent<CollisionFlash>();
    }
}
