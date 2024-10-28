﻿using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core.Surfaces.Vector;

public class PathEffect : NativeObject
{
    public override object Native => DrawingBackendApi.Current.PathEffectImplementation.GetNativePathEffect(ObjectPointer);
    public PathEffect(IntPtr objPtr) : base(objPtr)
    {
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathEffectImplementation.Dispose(ObjectPointer);
    }

    public static PathEffect? CreateDash(float[] intervals, float phase)
    {
        return new PathEffect(DrawingBackendApi.Current.PathEffectImplementation.CreateDash(intervals, phase));
    }
}
