﻿using Drawie.Core.Surfaces.ImageData;

namespace Drawie.Core.Bridge.NativeObjectsImpl;

public interface IImgDataImplementation
{
    public void Dispose(IntPtr objectPointer);
    public void SaveTo(ImgData imgData, FileStream stream);
    public Stream AsStream(ImgData imgData);
    public ReadOnlySpan<byte> AsSpan(ImgData imgData);
    public ImgData Create(ReadOnlySpan<byte> buffer);
    public object GetNativeImgData(IntPtr objectPointer);
}
