﻿using System.Runtime.InteropServices;

namespace Drawie.Numerics;
[StructLayout(LayoutKind.Sequential)]
public struct RectI : IEquatable<RectI>
{
    public static RectI Empty { get; } = new RectI();

    private int left;
    private int top;
    private int right;
    private int bottom;

    public int Left { readonly get => left; set => left = value; }
    public int Top { readonly get => top; set => top = value; }
    public int Right { readonly get => right; set => right = value; }
    public int Bottom { readonly get => bottom; set => bottom = value; }
    public int X { readonly get => left; set => left = value; }
    public int Y { readonly get => top; set => top = value; }
    public VecI Pos
    {
        readonly get => new VecI(left, top);
        set
        {
            right = (right - left) + value.X;
            bottom = (bottom - top) + value.Y;
            left = value.X;
            top = value.Y;
        }
    }
    public VecI TopLeft
    {
        readonly get => new VecI(left, top);
        set
        {
            left = value.X;
            top = value.Y;
        }
    }
    public VecI TopRight
    {
        readonly get => new VecI(right, top);
        set
        {
            right = value.X;
            top = value.Y;
        }
    }
    public VecI BottomLeft
    {
        readonly get => new VecI(left, bottom);
        set
        {
            left = value.X;
            bottom = value.Y;
        }
    }
    public VecI BottomRight
    {
        readonly get => new VecI(right, bottom);
        set
        {
            right = value.X;
            bottom = value.Y;
        }
    }

    public VecI Size
    {
        readonly get => new VecI(right - left, bottom - top);
        set
        {
            right = left + value.X;
            bottom = top + value.Y;
        }
    }
    public VecD Center { get => new VecD((left + right) / 2.0, (top + bottom) / 2.0); }
    public int Width { readonly get => right - left; set => right = left + value; }
    public int Height { readonly get => bottom - top; set => bottom = top + value; }
    public readonly bool IsZeroArea => left == right || top == bottom;
    public readonly bool IsZeroOrNegativeArea => left >= right || top >= bottom || Width < 0 || Height < 0; // checking Width and Height too as they can overflow in large rectangles 

    public RectI()
    {
        left = 0;
        top = 0;
        right = 0;
        bottom = 0;
    }

    public RectI(int x, int y, int width, int height)
    {
        left = x;
        top = y;
        right = x + width;
        bottom = y + height;
    }

    public RectI(VecI pos, VecI size)
    {
        left = pos.X;
        top = pos.Y;
        right = pos.X + size.X;
        bottom = pos.Y + size.Y;
    }

    public static RectI FromSides(int left, int right, int top, int bottom)
    {
        return new RectI()
        {
            Left = left,
            Right = right,
            Top = top,
            Bottom = bottom
        };
    }

    public static RectI FromTwoPoints(VecI point, VecI opposite)
    {
        return new RectI()
        {
            Left = Math.Min(point.X, opposite.X),
            Right = Math.Max(point.X, opposite.X),
            Top = Math.Min(point.Y, opposite.Y),
            Bottom = Math.Max(point.Y, opposite.Y)
        };
    }

    public static RectI FromTwoPixels(VecI pixel, VecI oppositePixel)
    {
        return new RectI(pixel, new(1, 1)).Union(new RectI(oppositePixel, new(1, 1)));
    }

    /// <summary>
    /// Converts rectangle with negative dimensions into a normal one
    /// </summary>
    public readonly RectI Standardize()
    {
        (int newLeft, int newRight) = left > right ? (right, left) : (left, right);
        (int newTop, int newBottom) = top > bottom ? (bottom, top) : (top, bottom);
        return new RectI()
        {
            Left = newLeft,
            Right = newRight,
            Top = newTop,
            Bottom = newBottom
        };
    }

    public readonly RectD ReflectX(double verLineX)
    {
        return RectD.FromTwoPoints(Pos.ReflectX(verLineX), (Pos + Size).ReflectX(verLineX));
    }

    public readonly RectD ReflectY(double horLineY)
    {
        return RectD.FromTwoPoints(Pos.ReflectY(horLineY), (Pos + Size).ReflectY(horLineY));
    }

    public readonly RectI ReflectX(int verLineX)
    {
        return RectI.FromTwoPoints(Pos.ReflectX(verLineX), (Pos + Size).ReflectX(verLineX));
    }

    public readonly RectI ReflectY(int horLineY)
    {
        return RectI.FromTwoPoints(Pos.ReflectY(horLineY), (Pos + Size).ReflectY(horLineY));
    }

    public readonly RectI Offset(VecI offset) => Offset(offset.X, offset.Y);
    public readonly RectI Offset(int x, int y)
    {
        return new RectI()
        {
            Left = left + x,
            Right = right + x,
            Top = top + y,
            Bottom = bottom + y
        };
    }

    public readonly RectI Inflate(VecI amount) => Inflate(amount.Y, amount.Y);
    public readonly RectI Inflate(int x, int y)
    {
        return new RectI()
        {
            Left = left - x,
            Right = right + x,
            Top = top - y,
            Bottom = bottom + y,
        };
    }

    public readonly RectI Inflate(int amount)
    {
        return new RectI()
        {
            Left = left - amount,
            Right = right + amount,
            Top = top - amount,
            Bottom = bottom + amount,
        };
    }

    public readonly RectD Scale(double multiplier)
    {
        return new RectD()
        {
            Left = left * multiplier,
            Right = right * multiplier,
            Top = top * multiplier,
            Bottom = bottom * multiplier
        };
    }

    public readonly RectD Scale(double multiplier, VecD relativeTo)
    {
        return new RectD()
        {
            Left = (left - relativeTo.X) * multiplier + relativeTo.X,
            Right = (right - relativeTo.X) * multiplier + relativeTo.X,
            Top = (top - relativeTo.Y) * multiplier + relativeTo.Y,
            Bottom = (bottom - relativeTo.Y) * multiplier + relativeTo.Y
        };
    }

    public readonly RectI Translate(VecI delta)
    {
        return new RectI()
        {
            Left = left + delta.X,
            Right = right + delta.X,
            Top = top + delta.Y,
            Bottom = bottom + delta.Y
        };
    }

    public readonly RectD Translate(VecD delta)
    {
        return new RectD()
        {
            Left = left + delta.X,
            Right = right + delta.X,
            Top = top + delta.Y,
            Bottom = bottom + delta.Y
        };
    }

    /// <summary>
    /// Fits passed rectangle into this rectangle while maintaining aspect ratio
    /// </summary>
    public readonly RectI AspectFit(RectI rect)
    {
        RectD rectD = (RectD)rect;
        RectD thisD = (RectD)this;
        RectD aspect = thisD.AspectFit(rectD);
        return new RectI((int)aspect.Left, (int)aspect.Top, (int)aspect.Width, (int)aspect.Height);
    }

    public readonly bool ContainsInclusive(VecI point) => ContainsInclusive(point.X, point.Y);
    public readonly bool ContainsInclusive(int x, int y)
    {
        return x >= left && x <= right && y >= top && y <= bottom;
    }

    public readonly bool ContainsExclusive(VecI point) => ContainsExclusive(point.X, point.Y);
    public readonly bool ContainsExclusive(int x, int y)
    {
        return x > left && x < right && y > top && y < bottom;
    }

    public readonly bool ContainsInclusive(RectI rect)
    {
        return ContainsInclusive(rect.TopLeft) && ContainsInclusive(rect.BottomRight);
    }


    public readonly bool ContainsExclusive(RectI rect)
    {
        return ContainsExclusive(rect.TopLeft) && ContainsExclusive(rect.BottomRight);
    }


    public readonly bool ContainsPixel(VecI pixelTopLeft) => ContainsPixel(pixelTopLeft.X, pixelTopLeft.Y);
    public readonly bool ContainsPixel(int pixelTopLeftX, int pixelTopLeftY)
    {
        return
            pixelTopLeftX >= left &&
            pixelTopLeftX < right &&
            pixelTopLeftY >= top &&
            pixelTopLeftY < bottom;
    }

    public readonly bool IntersectsWithInclusive(RectI rect)
    {
        return left <= rect.right && right >= rect.left && top <= rect.bottom && bottom >= rect.top;
    }

    public readonly bool IntersectsWithExclusive(RectI rect)
    {
        return left < rect.right && right > rect.left && top < rect.bottom && bottom > rect.top;
    }

    /// <summary>
    ///     Returns the intersection of two rectangles.
    /// </summary>
    /// <param name="other">Rectangle to intersect with.</param>
    /// <returns>Intersected rect.</returns>
    public readonly RectI Intersect(RectI other)
    {
        int left = Math.Max(this.left, other.left);
        int top = Math.Max(this.top, other.top);

        int right = Math.Min(this.right, other.right);
        int bottom = Math.Min(this.bottom, other.bottom);

        if (left >= right || top >= bottom)
            return RectI.Empty;

        return new RectI()
        {
            Left = left,
            Right = right,
            Top = top,
            Bottom = bottom
        };
    }
    
    public bool Intersects(RectI rectI)
    {
        return left < rectI.right && right > rectI.left && top < rectI.bottom && bottom > rectI.top;
    }

    public readonly RectI Union(RectI other)
    {
        int left = Math.Min(this.left, other.left);
        int top = Math.Min(this.top, other.top);

        int right = Math.Max(this.right, other.right);
        int bottom = Math.Max(this.bottom, other.bottom);

        if (left >= right || top >= bottom)
            return RectI.Empty;

        return new RectI()
        {
            Left = left,
            Right = right,
            Top = top,
            Bottom = bottom
        };
    }

    public static bool operator ==(RectI left, RectI right)
    {
        return left.left == right.left && left.right == right.right && left.top == right.top && left.bottom == right.bottom;
    }

    public static bool operator !=(RectI left, RectI right)
    {
        return !(left.left == right.left && left.right == right.right && left.top == right.top && left.bottom == right.bottom);
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is RectI rect && rect.left == left && rect.right == right && rect.top == top && rect.bottom == bottom;
    }

    public readonly bool Equals(RectI other)
    {
        return left == other.left && top == other.top && right == other.right && bottom == other.bottom;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(left, top, right, bottom);
    }

    public override string ToString()
    {
        return $"{{X: {X}, Y: {Y}, W: {Width}, H: {Height}}}";
    }

    public static RectI Create(int width, int height)
    {
        return new RectI()
        {
            Left = 0,
            Right = width,
            Top = 0,
            Bottom = height
        };
    }
    
    public static RectI Create(int x, int y, int width, int height)
    {
        return new RectI()
        {
            Left = x,
            Right = width,
            Top = y,
            Bottom = height
        };
    }
}
