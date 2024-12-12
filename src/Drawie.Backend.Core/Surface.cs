﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core;

public class Surface : IDisposable, ICloneable, IPixelsMap
{
    private bool disposed;
    public IntPtr PixelBuffer { get; }
    public DrawingSurface DrawingSurface { get; }
    public int BytesPerPixel { get; }
    public VecI Size { get; }

    public bool IsDisposed => disposed;

    private static ImageInfo DefaultImageInfo =>
        new(0, 0, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgb());

    public event SurfaceChangedEventHandler? Changed;

    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };

    private Paint nearestNeighborReplacingPaint =
        new() { BlendMode = BlendMode.Src, FilterQuality = FilterQuality.None };

    public ImageInfo ImageInfo { get; }

    public Surface(ImageInfo info)
    {
        var size = info.Size;

        if (size.X < 1 || size.Y < 1)
            throw new ArgumentException("Width and height must be >=1");

        Size = size;

        BytesPerPixel = info.BytesPerPixel;
        PixelBuffer = CreateBuffer(size.X, size.Y, BytesPerPixel);
        DrawingSurface = CreateDrawingSurface(info);
        ImageInfo = info;
    }

    public static Surface ForDisplay(VecI size)
    {
        return new Surface(
            new ImageInfo(size.X, size.Y, ColorType.Rgba8888, AlphaType.Premul, ColorSpace.CreateSrgb()));
    }

    public static Surface ForProcessing(VecI size)
    {
        return new Surface(
            new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgbLinear()));
    }

    public Surface(VecI size) : this(DefaultImageInfo.WithSize(size))
    {
    }

    public Surface(Surface original) : this(original.ImageInfo)
    {
        DrawingSurface.Canvas.DrawSurface(original.DrawingSurface, 0, 0);
    }

    public static Surface UsingColorType(VecI size, ColorType type = ColorType.RgbaF16)
    {
        if (type == ColorType.Unknown)
            throw new ArgumentException("Can't use unknown color type for surface", nameof(type));

        return new Surface(DefaultImageInfo.WithSize(size).WithColorType(type));
    }

    public static Surface Combine(int width, int height, List<(Image img, VecI offset)> images)
    {
        Surface surface = new Surface(new VecI(width, height));
        foreach (var (img, offset) in images)
        {
            surface.DrawingSurface.Canvas.DrawImage(img, offset.X, offset.Y);
        }

        return surface;
    }

    public static Surface Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);
        using var image = Image.FromEncodedData(path);
        if (image is null)
            throw new ArgumentException($"The image with path {path} couldn't be loaded");

        var surface = new Surface(new VecI(image.Width, image.Height));
        surface.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return surface;
    }

    public static Surface Load(byte[] encoded)
    {
        using var image = Image.FromEncodedData(encoded);
        if (image is null)
            throw new ArgumentException($"The passed byte array does not contain a valid image");

        var surface = new Surface(new VecI(image.Width, image.Height));
        surface.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return surface;
    }

    public static Surface? Load(byte[] encoded, ColorType colorType, VecI imageSize)
    {
        using var image = Image.FromPixels(new ImageInfo(imageSize.X, imageSize.Y, colorType), encoded);
        if (image is null)
            return null;

        var surface = new Surface(new VecI(image.Width, image.Height));
        surface.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return surface;
    }

    public unsafe void DrawBytes(VecI size, byte[] bytes, ColorType colorType, AlphaType alphaType)
    {
        ImageInfo info = new ImageInfo(size.X, size.Y, colorType, alphaType);

        fixed (void* pointer = bytes)
        {
            using Pixmap map = new(info, new IntPtr(pointer));
            using DrawingSurface surface = DrawingSurface.Create(map);
            surface.Draw(DrawingSurface.Canvas, 0, 0, drawingPaint);
        }

        DrawingSurfaceChanged(new RectD(0, 0, size.X, size.Y));
    }

    public Surface Resize(VecI newSize, ResizeMethod resizeMethod)
    {
        using Image image = DrawingSurface.Snapshot();
        Surface newSurface = new(newSize);
        using Paint paint = new();

        FilterQuality filterQuality = resizeMethod switch
        {
            ResizeMethod.HighQuality => FilterQuality.High,
            ResizeMethod.MediumQuality => FilterQuality.Medium,
            ResizeMethod.LowQuality => FilterQuality.Low,
            _ => FilterQuality.None
        };

        paint.FilterQuality = filterQuality;

        newSurface.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y), paint);
        return newSurface;
    }

    public Surface ResizeNearestNeighbor(VecI newSize)
    {
        using Image image = DrawingSurface.Snapshot();
        Surface newSurface = new(newSize);
        newSurface.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y),
            nearestNeighborReplacingPaint);
        return newSurface;
    }

    public unsafe void CopyTo(Surface other)
    {
        if (other.Size != Size)
            throw new ArgumentException("Target Surface must have the same dimensions");
        int bytesC = Size.X * Size.Y * BytesPerPixel;
        using var pixmap = other.DrawingSurface.PeekPixels();
        Buffer.MemoryCopy((void*)PixelBuffer, (void*)pixmap.GetPixels(), bytesC, bytesC);
    }

    /// <summary>
    /// Consider getting a pixmap from SkiaSurface.PeekPixels().GetPixels() and writing into it's buffer for bulk pixel get/set. Don't forget to dispose the pixmap afterwards.
    /// </summary>
    public unsafe Color GetSRGBPixel(VecI pos)
    {
        Half* ptr = (Half*)(PixelBuffer + (pos.X + pos.Y * Size.X) * BytesPerPixel);
        float a = (float)ptr[3];
        return (Color)new ColorF((float)ptr[0] / a, (float)ptr[1] / a, (float)ptr[2] / a, (float)ptr[3]);
    }

    public void SetSRGBPixel(VecI pos, Color color)
    {
        drawingPaint.Color = color;
        DrawingSurface.Canvas.DrawPixel(pos.X, pos.Y, drawingPaint);
        DrawingSurfaceChanged(new RectD(pos.X, pos.Y, 1, 1));
    }

    public unsafe bool IsFullyTransparent()
    {
        ulong* ptr = (ulong*)PixelBuffer;
        for (int i = 0; i < Size.X * Size.Y; i++)
        {
            // ptr[i] actually contains 4 16-bit floats. We only care about the first one which is alpha.
            // An empty pixel can have alpha of 0 or -0 (not sure if -0 actually ever comes up). 0 in hex is 0x0, -0 in hex is 0x8000
            if ((ptr[i] & 0x1111_0000_0000_0000) != 0 && (ptr[i] & 0x1111_0000_0000_0000) != 0x8000_0000_0000_0000)
                return false;
        }

        return true;
    }

#if DEBUG
    public void SaveToDesktop(string filename = "savedSurface.png")
    {
        using var final = DrawingSurface.Create(new ImageInfo(Size.X, Size.Y, ColorType.Rgba8888, AlphaType.Premul,
            ColorSpace.CreateSrgb()));
        final.Canvas.DrawSurface(DrawingSurface, 0, 0);
        using (var snapshot = final.Snapshot())
        {
            using var stream =
                File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    filename));
            using var png = snapshot.Encode();
            png.SaveTo(stream);
        }
    }
#endif

    private DrawingSurface CreateDrawingSurface(ImageInfo info)
    {
        var surface = DrawingSurface.Create(info, PixelBuffer);
        surface.Changed += DrawingSurfaceChanged;
        if (surface is null)
            throw new InvalidOperationException($"Could not create surface (Size:{Size})");
        return surface;
    }

    private static unsafe IntPtr CreateBuffer(int width, int height, int bytesPerPixel)
    {
        int byteC = width * height * bytesPerPixel;
        var buffer = Marshal.AllocHGlobal(byteC);
        Unsafe.InitBlockUnaligned((byte*)buffer, 0, (uint)byteC);
        return buffer;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        DrawingSurface.Changed -= DrawingSurfaceChanged;
        disposed = true;
        drawingPaint.Dispose();
        nearestNeighborReplacingPaint.Dispose();
        DrawingSurface.Dispose();
        Marshal.FreeHGlobal(PixelBuffer);
        GC.SuppressFinalize(this);
    }

    public void AddDirtyRect(RectI dirtyRect)
    {
        DrawingSurfaceChanged(new RectD(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height));
    }

    private void DrawingSurfaceChanged(RectD? changedRect)
    {
        Changed?.Invoke(changedRect);
    }

    ~Surface()
    {
        Marshal.FreeHGlobal(PixelBuffer);
    }

    public Pixmap PeekPixels()
    {
        return DrawingSurface.PeekPixels();
    }

    public object Clone()
    {
        return new Surface(this);
    }
}

public enum ResizeMethod
{
    NearestNeighbor,
    HighQuality,
    MediumQuality,
    LowQuality,
}
