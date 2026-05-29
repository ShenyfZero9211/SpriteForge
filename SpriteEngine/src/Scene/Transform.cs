using SpriteCore.Math;

namespace SpriteEngine.Scenes;

/// <summary>
/// Transform 组件：管理 GameObject 在 2D 世界中的位置、旋转和缩放。
/// 每个 GameObject 有且仅有一个 Transform。
/// 对应 Unity 的 Transform2D / p5engine 的 Transform。
/// </summary>
public sealed class Transform : Component
{
    /// <summary>本地空间位置（相对于父 Transform）</summary>
    public Vector2 LocalPosition { get; set; } = Vector2.Zero;

    /// <summary>本地空间旋转（角度，单位：度）</summary>
    public float LocalRotation { get; set; } = 0;

    /// <summary>本地空间缩放</summary>
    public Vector2 LocalScale { get; set; } = Vector2.One;

    /// <summary>世界空间位置（自动计算，包含父级变换）</summary>
    public Vector2 Position
    {
        get
        {
            if (GameObject?.Parent?.Transform is not Transform parentT)
                return LocalPosition;
            return parentT.Position + LocalPosition;
        }
        set
        {
            if (GameObject?.Parent?.Transform is not Transform parentT)
                LocalPosition = value;
            else
                LocalPosition = value - parentT.Position;
        }
    }

    /// <summary>世界空间旋转（自动计算，包含父级旋转）</summary>
    public float Rotation
    {
        get
        {
            if (GameObject?.Parent?.Transform is not Transform parentT)
                return LocalRotation;
            return parentT.Rotation + LocalRotation;
        }
        set
        {
            if (GameObject?.Parent?.Transform is not Transform parentT)
                LocalRotation = value;
            else
                LocalRotation = value - parentT.Rotation;
        }
    }

    /// <summary>世界空间缩放（自动计算，包含父级缩放）</summary>
    public Vector2 Scale
    {
        get
        {
            if (GameObject?.Parent?.Transform is not Transform parentT)
                return LocalScale;
            return new Vector2(parentT.Scale.X * LocalScale.X, parentT.Scale.Y * LocalScale.Y);
        }
        set
        {
            if (GameObject?.Parent?.Transform is not Transform parentT)
                LocalScale = value;
            else
                LocalScale = new Vector2(value.X / parentT.Scale.X, value.Y / parentT.Scale.Y);
        }
    }

    /// <summary>向前向量（基于 Rotation）</summary>
    public Vector2 Forward
    {
        get
        {
            float rad = MathUtils.Radians(Rotation);
            return new Vector2(MathUtils.Cos(rad), MathUtils.Sin(rad));
        }
    }

    /// <summary>向右向量（基于 Rotation，2D 中即 Forward 逆时针旋转 90°）</summary>
    public Vector2 Right
    {
        get
        {
            float rad = MathUtils.Radians(Rotation);
            return new Vector2(-MathUtils.Sin(rad), MathUtils.Cos(rad));
        }
    }

    /// <summary>将本地坐标转换为世界坐标</summary>
    public Vector2 LocalToWorld(Vector2 localPoint)
    {
        float rad = MathUtils.Radians(LocalRotation);
        float cos = MathUtils.Cos(rad);
        float sin = MathUtils.Sin(rad);
        var rotated = new Vector2(
            localPoint.X * cos - localPoint.Y * sin,
            localPoint.X * sin + localPoint.Y * cos);
        var scaled = new Vector2(rotated.X * LocalScale.X, rotated.Y * LocalScale.Y);
        return scaled + Position;
    }

    /// <summary>将世界坐标转换为本地坐标</summary>
    public Vector2 WorldToLocal(Vector2 worldPoint)
    {
        var relative = worldPoint - Position;
        var unscaled = new Vector2(relative.X / LocalScale.X, relative.Y / LocalScale.Y);
        float rad = -MathUtils.Radians(LocalRotation);
        float cos = MathUtils.Cos(rad);
        float sin = MathUtils.Sin(rad);
        return new Vector2(
            unscaled.X * cos - unscaled.Y * sin,
            unscaled.X * sin + unscaled.Y * cos);
    }
}
