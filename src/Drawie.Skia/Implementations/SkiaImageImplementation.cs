﻿using Drawie.Core.Bridge.Operations;
using Drawie.Core.Shaders;
using Drawie.Core.Surfaces;
using Drawie.Core.Surfaces.ImageData;
using PixiEditor.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaImageImplementation : SkObjectImplementation<SKImage>, IImageImplementation
    {
        private readonly SkObjectImplementation<SKData> _imgImplementation;
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private SkObjectImplementation<SKSurface>? _surfaceImplementation;
        private SkiaShaderImplementation shaderImpl;
        
        public SkiaImageImplementation(SkObjectImplementation<SKData> imgDataImplementation, SkiaPixmapImplementation pixmapImplementation,  SkiaShaderImplementation shaderImplementation)
        {
            _imgImplementation = imgDataImplementation;
            _pixmapImplementation = pixmapImplementation;
            shaderImpl = shaderImplementation;
        }
        
        public void SetSurfaceImplementation(SkObjectImplementation<SKSurface> surfaceImplementation)
        {
            _surfaceImplementation = surfaceImplementation;
        }
        
        public Image Snapshot(DrawingSurface drawingSurface)
        {
            var surface = _surfaceImplementation![drawingSurface.ObjectPointer];
            SKImage snapshot = surface.Snapshot();
            
            ManagedInstances[snapshot.Handle] = snapshot;
            return new Image(snapshot.Handle);
        }

        public Image Snapshot(DrawingSurface drawingSurface, RectI bounds)
        {
            var surface = _surfaceImplementation![drawingSurface.ObjectPointer];
            SKImage snapshot = surface.Snapshot(bounds.ToSkRectI());

            ManagedInstances[snapshot.Handle] = snapshot;
            return new Image(snapshot.Handle);
        }
        
        public Image? FromEncodedData(byte[] dataBytes)
        {
            SKImage img = SKImage.FromEncodedData(dataBytes);
            if (img is null)
                return null;
            ManagedInstances[img.Handle] = img;
            
            return new Image(img.Handle);
        }

        public void DisposeImage(Image image)
        {
            ManagedInstances[image.ObjectPointer].Dispose();
            ManagedInstances.TryRemove(image.ObjectPointer, out _);
        }

        public Image? FromEncodedData(string path)
        {
            var nativeImg = SKImage.FromEncodedData(path);
            if (nativeImg is null)
                return null;
            ManagedInstances[nativeImg.Handle] = nativeImg;
            return new Image(nativeImg.Handle);
        }

        public Image? FromPixelCopy(ImageInfo info, byte[] pixels)
        {
            var nativeImg = SKImage.FromPixelCopy(info.ToSkImageInfo(), pixels);
            if (nativeImg is null)
                return null;
            ManagedInstances[nativeImg.Handle] = nativeImg;
            return new Image(nativeImg.Handle);
        }
        
        public Pixmap PeekPixels(Image image)
        {
            var native = ManagedInstances[image.ObjectPointer];
            var pixmap = native.PeekPixels();
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public void GetColorShifts(ref int platformColorAlphaShift, ref int platformColorRedShift, ref int platformColorGreenShift,
            ref int platformColorBlueShift)
        {
            platformColorAlphaShift = SKImageInfo.PlatformColorAlphaShift;
            platformColorRedShift = SKImageInfo.PlatformColorRedShift;
            platformColorGreenShift = SKImageInfo.PlatformColorGreenShift;
            platformColorBlueShift = SKImageInfo.PlatformColorBlueShift;
        }

        public ImgData Encode(Image image)
        {
            var native = ManagedInstances[image.ObjectPointer];
            var encoded = native.Encode();
            _imgImplementation.ManagedInstances[encoded.Handle] = encoded;
            return new ImgData(encoded.Handle);
        }

        public ImgData Encode(Image image, EncodedImageFormat format, int quality)
        {
            var native = ManagedInstances[image.ObjectPointer];
            var encoded = native.Encode((SKEncodedImageFormat)format, quality);
            _imgImplementation.ManagedInstances[encoded.Handle] = encoded;
            return new ImgData(encoded.Handle);
        }

        public int GetWidth(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer].Width;
        }

        public int GetHeight(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer].Height;
        }
        
        public Image Clone(Image image)
        {
            var native = ManagedInstances[image.ObjectPointer];
            var encoded = native.Encode();
            var clone = SKImage.FromEncodedData(encoded);
            ManagedInstances[clone.Handle] = clone;
            return new Image(clone.Handle);
        }

        public Pixmap PeekPixels(IntPtr objectPointer)
        {
            var nativePixmap = ManagedInstances[objectPointer].PeekPixels();

            return _pixmapImplementation.CreateFrom(nativePixmap);
        }

        public ImageInfo GetImageInfo(IntPtr objectPointer)
        {
            var info = ManagedInstances[objectPointer].Info;
            return info.ToImageInfo();
        }

        public Shader ToShader(IntPtr objectPointer)
        {
            var shader = ManagedInstances[objectPointer].ToShader();
            shaderImpl.ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public object GetNativeImage(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }
    }
}
