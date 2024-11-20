﻿using System.Collections;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Vector;

public class PathIterator : NativeObject, IEnumerator<(PathVerb verb, VecF[] points)>
{
    public PathIterator(IntPtr objPtr) : base(objPtr)
    {
    }

    private PathVerb currentVerb;
    private VecF[] iteratorPoints;

    public override object Native => DrawingBackendApi.Current.PathImplementation.GetNativeIterator(ObjectPointer);

    public bool IsCloseContour => DrawingBackendApi.Current.PathImplementation.IsCloseContour(ObjectPointer);

    public PathVerb Next(VecF[] points)
    {
        return DrawingBackendApi.Current.PathImplementation.IteratorNextVerb(ObjectPointer, points);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathImplementation.DisposeIterator(ObjectPointer);
    }

    bool IEnumerator.MoveNext()
    {
        iteratorPoints = new VecF[4];
        currentVerb = Next(iteratorPoints);
        bool done = currentVerb == PathVerb.Done;
        return !done;
    }

    void IEnumerator.Reset()
    {
        throw new ArgumentException("Path Iterators can't be reused");
    }

    (PathVerb verb, VecF[] points) IEnumerator<(PathVerb verb, VecF[] points)>.Current => (currentVerb, iteratorPoints);

    object? IEnumerator.Current => (currentVerb, iteratorPoints);
}
