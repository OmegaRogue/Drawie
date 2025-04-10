﻿using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaImageFilterImplementation : SkObjectImplementation<SKImageFilter>, IImageFilterImplementation
    {
        public SkiaShaderImplementation ShaderImplementation { get; set; }
        public SkiaImageImplementation ImageImplementation { get; set; }

        public IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias,
            VecI kernelOffset, TileMode mode, bool convolveAlpha)
        {
            var skImageFilter = SKImageFilter.CreateMatrixConvolution(
                new SKSizeI(size.X, size.Y),
                kernel,
                gain,
                bias,
                new SKPointI(kernelOffset.X, kernelOffset.Y),
                (SKShaderTileMode)mode,
                convolveAlpha);

            AddManagedInstance(skImageFilter);
            return skImageFilter.Handle;
        }

        public IntPtr CreateCompose(ImageFilter outer, ImageFilter inner)
        {
            var skOuter = this[outer.ObjectPointer];
            var skInner = this[inner.ObjectPointer];

            var compose = SKImageFilter.CreateCompose(skOuter, skInner);
            AddManagedInstance(compose);

            return compose.Handle;
        }

        public object GetNativeImageFilter(IntPtr objPtr) => this[objPtr];

        public IntPtr CreateBlur(float sigmaX, float sigmaY)
        {
            var skImageFilter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
            AddManagedInstance(skImageFilter);
            return skImageFilter.Handle;
        }

        public IntPtr CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, Color color,
            ImageFilter? input)
        {
            SKImageFilter? inputFilter = null;
            if (input != null)
            {
                inputFilter = this[input.ObjectPointer];
            }

            var skImageFilter = SKImageFilter.CreateDropShadow(dx, dy, sigmaX, sigmaY, color.ToSKColor(), inputFilter);
            AddManagedInstance(skImageFilter);
            return skImageFilter.Handle;
        }

        public IntPtr CreateShader(Shader shader, bool dither)
        {
            var skShader = ShaderImplementation[shader.ObjectPointer];
            var skImageFilter = SKImageFilter.CreateShader(skShader, dither);
            AddManagedInstance(skImageFilter);
            return skImageFilter.Handle;
        }

        public IntPtr CreateImage(Image image)
        {
            if (image == null)
            {
                return IntPtr.Zero;
            }


            SKImage target = ImageImplementation[image.ObjectPointer];
            var skImageFilter = SKImageFilter.CreateImage(target);
            AddManagedInstance(skImageFilter);
            return skImageFilter.Handle;
        }

        public IntPtr CreateTile(RectD source, RectD dest, ImageFilter input)
        {
            if (input == null)
            {
                throw new System.ArgumentNullException(nameof(input));
            }

            var skImageFilter = SKImageFilter.CreateTile(source.ToSKRect(), dest.ToSKRect(),
                this[input.ObjectPointer]);
            AddManagedInstance(skImageFilter);
            return skImageFilter.Handle;
        }

        public void DisposeObject(IntPtr objectPointer)
        {
            UnmanageAndDispose(objectPointer);
        }
    }
}
