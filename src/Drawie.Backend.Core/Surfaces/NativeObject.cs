﻿namespace Drawie.Backend.Core.Surfaces;

public abstract class NativeObject : IDisposable
{
    public abstract object Native { get; }
    public IntPtr ObjectPointer { get; protected set; }
    public abstract void Dispose();

    internal NativeObject(IntPtr objPtr)
    {
        ObjectPointer = objPtr;
    }
}
