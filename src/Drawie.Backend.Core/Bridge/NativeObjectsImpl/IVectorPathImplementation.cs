﻿using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IVectorPathImplementation
{
    public PathFillType GetFillType(VectorPath path);
    public void SetFillType(VectorPath path, PathFillType fillType);
    public PathConvexity GetConvexity(VectorPath path);
    public void Dispose(VectorPath path);
    public bool IsPathOval(VectorPath path);
    public bool IsRoundRect(VectorPath path);
    public bool IsLine(VectorPath path);
    public bool IsRect(VectorPath path);
    public PathSegmentMask GetSegmentMasks(VectorPath path);
    public int GetVerbCount(VectorPath path);
    public int GetPointCount(VectorPath path);
    public IntPtr Create();
    public IntPtr Clone(VectorPath other);
    public RectD GetTightBounds(VectorPath vectorPath);
    public void Transform(VectorPath vectorPath, Matrix3X3 matrix);
    public RectD GetBounds(VectorPath vectorPath);
    public void Reset(VectorPath vectorPath);
    public void AddRect(VectorPath path, RectD rect, PathDirection direction);
    public void MoveTo(VectorPath vectorPath, VecF vecF);
    public void LineTo(VectorPath vectorPath, VecF vecF);
    public void QuadTo(VectorPath vectorPath, VecF control, VecF point);
    public void CubicTo(VectorPath vectorPath, VecF mid1, VecF mid2, VecF point);
    public void ConicTo(VectorPath vectorPath, VecF mid, VecF end, float weight);
    public void ArcTo(VectorPath vectorPath, RectD oval, int startAngle, int sweepAngle, bool forceMoveTo);
    public void AddOval(VectorPath vectorPath, RectD borders);
    public VectorPath Op(VectorPath vectorPath, VectorPath ellipsePath, VectorPathOp pathOp);
    public void Close(VectorPath vectorPath);
    public VectorPath Simplify(VectorPath vectorPath);
    public string ToSvgPathData(VectorPath vectorPath);
    public bool Contains(VectorPath vectorPath, float x, float y);
    public void AddPath(VectorPath vectorPath, VectorPath path, AddPathMode mode);
    public object GetNativePath(IntPtr objectPointer);
    public VecF[] GetPoints(IntPtr objectPointer);
    public VecF GetLastPoint(VectorPath vectorPath);
    public VectorPath FromSvgPath(string svgPath);
    public PathIterator CreateIterator(IntPtr objectPointer, bool forceClose);
    public void DisposeIterator(IntPtr objectPointer);
    public object GetNativeIterator(IntPtr objectPointer);
    public bool IsCloseContour(IntPtr objectPointer);
    public PathVerb IteratorNextVerb(IntPtr objectPointer, VecF[] points);
    public RawPathIterator CreateRawIterator(IntPtr objectPointer);
    public PathVerb RawIteratorNextVerb(IntPtr objectPointer, VecF[] points);
    public void DisposeRawIterator(IntPtr objectPointer);
    public object GetNativeRawIterator(IntPtr objectPointer);
    public void Offset(VectorPath vectorPath, VecD delta);
}
