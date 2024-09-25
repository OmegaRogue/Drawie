﻿using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public sealed class SkiaImgDataImplementation : SkObjectImplementation<SKData>, IImgDataImplementation
    {
        public void Dispose(IntPtr objectPointer)
        {
            if (ManagedInstances.ContainsKey(objectPointer))
            {
                ManagedInstances[objectPointer].Dispose();
                ManagedInstances.TryRemove(objectPointer, out _);
            }
        }

        public void SaveTo(ImgData imgData, FileStream stream)
        {
            SKData data = ManagedInstances[imgData.ObjectPointer];
            data.SaveTo(stream);
        }

        public Stream AsStream(ImgData imgData)
        {
            SKData data = ManagedInstances[imgData.ObjectPointer];
            return data.AsStream();
        }

        public ReadOnlySpan<byte> AsSpan(ImgData imgData)
        {
            SKData data = ManagedInstances[imgData.ObjectPointer];
            return data.AsSpan();
        }
        
        public ImgData Create(ReadOnlySpan<byte> buffer)
        {
            SKData data = SKData.CreateCopy(buffer.ToArray());
            ManagedInstances.TryAdd(data.Handle, data);
            return new ImgData(data.Handle);
        }

        public object GetNativeImgData(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }

        public void SaveTo(ImgData imgData, Stream stream)
        {
            SKData data = ManagedInstances[imgData.ObjectPointer];
            data.SaveTo(stream);
        }
    }
}
