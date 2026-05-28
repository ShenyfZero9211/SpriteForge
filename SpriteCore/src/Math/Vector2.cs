using System.Numerics;

namespace SpriteCore.Math;

public struct Vector2
{
    public float X;
    public float Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float Length => MathF.Sqrt(X * X + Y * Y);
    public float LengthSquared => X * X + Y * Y;

    public Vector2 Normalized()
    {
        float len = Length;
        return len > 0 ? new Vector2(X / len, Y / len) : new Vector2(0, 0);
    }

    public void Normalize()
    {
        float len = Length;
        if (len > 0)
        {
            X /= len;
            Y /= len;
        }
    }

    public float Dot(Vector2 other) => X * other.X + Y * other.Y;
    public float Cross(Vector2 other) => X * other.Y - Y * other.X;
    public float Distance(Vector2 other) => (this - other).Length;
    public float DistanceSquared(Vector2 other) => (this - other).LengthSquared;

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        => new Vector2(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t
        );

    public static Vector2 Zero => new Vector2(0, 0);
    public static Vector2 One => new Vector2(1, 1);
    public static Vector2 Up => new Vector2(0, -1);
    public static Vector2 Down => new Vector2(0, 1);
    public static Vector2 Left => new Vector2(-1, 0);
    public static Vector2 Right => new Vector2(1, 0);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator -(Vector2 v) => new Vector2(-v.X, -v.Y);
    public static Vector2 operator *(Vector2 v, float s) => new Vector2(v.X * s, v.Y * s);
    public static Vector2 operator *(float s, Vector2 v) => new Vector2(v.X * s, v.Y * s);
    public static Vector2 operator /(Vector2 v, float s) => new Vector2(v.X / s, v.Y / s);
    public static bool operator ==(Vector2 a, Vector2 b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

    public static Vector2 LuaCreate(float x, float y) => new Vector2(x, y);

    public override bool Equals(object? obj) => obj is Vector2 v && this == v;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X:F2}, {Y:F2})";
}
