﻿using Drawie.Core.Bridge;

namespace Drawie.Core.Surfaces.ImageData;

/// <summary>The <see cref="ImgData" /> holds an immutable data buffer.</summary>
public class ImgData : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ImgDataImplementation.GetNativeImgData(ObjectPointer);

    public ImgData(IntPtr objPtr) : base(objPtr)
    {
    }
    
    ~ImgData()
    {
        Dispose();
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ImgDataImplementation.Dispose(ObjectPointer);
    }

    public void SaveTo(FileStream stream)
    {
        DrawingBackendApi.Current.ImgDataImplementation.SaveTo(this, stream);
    }

    public Stream AsStream()
    {
        return DrawingBackendApi.Current.ImgDataImplementation.AsStream(this);
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        return DrawingBackendApi.Current.ImgDataImplementation.AsSpan(this);
    }

    public static ImgData Create(byte[] pixelData)
    {
        return DrawingBackendApi.Current.ImgDataImplementation.Create(pixelData);
    }
}
