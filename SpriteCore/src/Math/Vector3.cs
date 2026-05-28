namespace SpriteCore.Math;

public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public float LengthSquared => X * X + Y * Y + Z * Z;

    public Vector3 Normalized()
    {
        float len = Length;
        return len > 0 ? new Vector3(X / len, Y / len, Z / len) : new Vector3(0, 0, 0);
    }

    public void Normalize()
    {
        float len = Length;
        if (len > 0)
        {
            X /= len;
            Y /= len;
            Z /= len;
        }
    }

    public float Dot(Vector3 other) => X * other.X + Y * other.Y + Z * other.Z;

    public Vector3 Cross(Vector3 other)
        => new Vector3(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );

    public float Distance(Vector3 other) => (this - other).Length;
    public float DistanceSquared(Vector3 other) => (this - other).LengthSquared;

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        => new Vector3(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t
        );

    public static Vector3 Zero => new Vector3(0, 0, 0);
    public static Vector3 One => new Vector3(1, 1, 1);
    public static Vector3 Up => new Vector3(0, -1, 0);
    public static Vector3 Down => new Vector3(0, 1, 0);
    public static Vector3 Left => new Vector3(-1, 0, 0);
    public static Vector3 Right => new Vector3(1, 0, 0);
    public static Vector3 Forward => new Vector3(0, 0, 1);
    public static Vector3 Back => new Vector3(0, 0, -1);

    public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);
    public static Vector3 operator *(Vector3 v, float s) => new Vector3(v.X * s, v.Y * s, v.Z * s);
    public static Vector3 operator *(float s, Vector3 v) => new Vector3(v.X * s, v.Y * s, v.Z * s);
    public static Vector3 operator /(Vector3 v, float s) => new Vector3(v.X / s, v.Y / s, v.Z / s);
    public static bool operator ==(Vector3 a, Vector3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);

    public override bool Equals(object? obj) => obj is Vector3 v && this == v;
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
