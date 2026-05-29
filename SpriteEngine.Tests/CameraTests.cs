using SpriteCore.Graphics;
using SpriteCore.Math;
using SkiaSharp;
using SpriteEngine.Scenes;
using Xunit;

namespace SpriteEngine.Tests;

public class CameraTests
{
    public CameraTests()
    {
        SP5.Width = 800;
        SP5.Height = 600;
    }

    private class TestGraphics : SPGraphics
    {
        public override void BeginFrame() { }
        public override void EndFrame() { }
        public override void Resize(int width, int height) { Width = width; Height = height; }
        public override void Background(float r, float g, float b, float a) { }
        public override void Rect(float x, float y, float w, float h) { }
        public override void Ellipse(float x, float y, float w, float h) { }
        public override void Circle(float x, float y, float r) { }
        public override void Line(float x1, float y1, float x2, float y2) { }
        public override void Triangle(float x1, float y1, float x2, float y2, float x3, float y3) { }
        public override void TextSize(float size) { }
        public override void Text(string str, float x, float y) { }
        public override void Image(SKBitmap bitmap, float x, float y) { }
        public override void Present(IntPtr sdlRenderer, IntPtr sdlTexture) { }
    }
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var c = new Camera();
        Assert.Equal(Vector2.Zero, c.Position);
        Assert.Equal(0f, c.Rotation);
        Assert.Equal(1f, c.Zoom);
    }

    [Fact]
    public void WorldToScreen_WithIdentityCamera_ReturnsCentered()
    {
        // Camera at (0,0), no rotation, zoom=1, viewport 800x600
        // World (0,0) should map to screen center (400, 300)
        var c = new Camera { Zoom = 1f };
        var screen = c.WorldToScreen(new Vector2(0, 0));
        Assert.Equal(400f, screen.X, 1f);
        Assert.Equal(300f, screen.Y, 1f);
    }

    [Fact]
    public void WorldToScreen_WithPositionOffset()
    {
        // Camera at (100, 0), zoom=1
        // World (100, 0) should map to screen center
        var c = new Camera { Position = new Vector2(100, 0), Zoom = 1f };
        var screen = c.WorldToScreen(new Vector2(100, 0));
        Assert.Equal(400f, screen.X, 1f);
        Assert.Equal(300f, screen.Y, 1f);
    }

    [Fact]
    public void WorldToScreen_WithZoom()
    {
        // Camera at (0,0), zoom=2
        // World (10, 0) → (20, 0) from center → screen (420, 300)
        var c = new Camera { Zoom = 2f };
        var screen = c.WorldToScreen(new Vector2(10, 0));
        Assert.Equal(420f, screen.X, 1f);
        Assert.Equal(300f, screen.Y, 1f);
    }

    [Fact]
    public void WorldToScreen_WithRotation()
    {
        // Camera at (0,0), rotate 90°
        // World (10, 0) rotated by -90° → (0, -10)
        // scaled by 1 → (0, -10) + center(400,300) → (400, 290)
        var c = new Camera { Rotation = 90 };
        var screen = c.WorldToScreen(new Vector2(10, 0));
        Assert.Equal(400f, screen.X, 1f);
        Assert.Equal(290f, screen.Y, 1f);
    }

    [Fact]
    public void ScreenToWorld_IsInverse()
    {
        var c = new Camera
        {
            Position = new Vector2(100, 50),
            Rotation = 45,
            Zoom = 2f
        };

        var world = new Vector2(200, 150);
        var screen = c.WorldToScreen(world);
        var back = c.ScreenToWorld(screen);

        Assert.Equal(world.X, back.X, 1f);
        Assert.Equal(world.Y, back.Y, 1f);
    }

    [Fact]
    public void LookAt_SetsPosition()
    {
        var c = new Camera();
        c.LookAt(new Vector2(500, 400));
        Assert.Equal(new Vector2(500, 400), c.Position);
    }

    [Fact]
    public void Follow_MovesTowardsTarget()
    {
        var c = new Camera { Position = new Vector2(0, 0) };
        c.Follow(new Vector2(100, 0), smoothTime: 0.1f);

        // Should have moved closer to target
        Assert.True(c.Position.X > 0);
        Assert.True(c.Position.X < 100);
    }

    [Fact]
    public void Apply_UsesTransform_WhenAttachedToGameObject()
    {
        var go = new GameObject("Camera");
        go.Transform.Position = new Vector2(50, 60);
        go.Transform.Rotation = 30;
        go.Transform.LocalScale = new Vector2(1.5f, 1.5f);

        var cam = go.AddComponent<Camera>();
        // Apply syncs Position/Rotation/Zoom from Transform
        var g = new TestGraphics();
        g.Resize(800, 600);
        cam.Apply(g);
        cam.Restore(g);

        Assert.Equal(go.Transform.Position, cam.Position);
        Assert.Equal(go.Transform.Rotation, cam.Rotation);
        Assert.Equal(go.Transform.Scale.X, cam.Zoom);
    }
}
