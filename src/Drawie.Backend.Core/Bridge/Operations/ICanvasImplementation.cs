﻿using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Surfaces.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.Operations
{
    public interface ICanvasImplementation
    {
        public void DrawPixel(IntPtr objPtr, int posX, int posY, Paint drawingPaint);
        public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, int x, int y, Paint? paint);
        public void DrawImage(IntPtr objPtr, Image image, int x, int y);
        public int Save(IntPtr objPtr);
        public void Restore(IntPtr objPtr);
        public void Scale(IntPtr objPtr, float sizeX, float sizeY);
        public void Translate(IntPtr objPtr, float translationX, float translationY);
        public void DrawPath(IntPtr objPtr, VectorPath path, Paint paint);
        public void DrawPoint(IntPtr objPtr, VecI pos, Paint paint);
        public void DrawPoints(IntPtr objPtr, PointMode pointMode, Point[] points, Paint paint);
        public void DrawRect(IntPtr objPtr, int x, int y, int width, int height, Paint paint);
        public void DrawCircle(IntPtr objPtr, int cx, int cy, int radius, Paint paint);
        public void DrawOval(IntPtr objPtr, int cx, int cy, int width, int height, Paint paint);
        public void ClipPath(IntPtr objPtr, VectorPath clipPath, ClipOperation clipOperation, bool antialias);
        public void ClipRect(IntPtr objPtr, RectD rect, ClipOperation clipOperation);
        public void Clear(IntPtr objPtr);
        public void Clear(IntPtr objPtr, Color color);
        public void DrawLine(IntPtr objPtr, VecI from, VecI to, Paint paint);
        public void Flush(IntPtr objPtr);
        public void SetMatrix(IntPtr objPtr, Matrix3X3 finalMatrix);
        public void RestoreToCount(IntPtr objPtr, int count);
        public void DrawColor(IntPtr objPtr, Color color, BlendMode paintBlendMode);
        public void RotateRadians(IntPtr objPtr, float radians, float centerX, float centerY);
        public void RotateDegrees(IntPtr objectPointer, float degrees, float centerX, float centerY);
        public void DrawImage(IntPtr objPtr, Image image, RectD destRect, Paint paint);
        public void DrawImage(IntPtr objPtr, Image image, RectD sourceRect, RectD destRect, Paint paint);
        public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, int x, int y);
        public void Dispose(IntPtr objectPointer);
        public object GetNativeCanvas(IntPtr objectPointer);
        public void DrawPaint(IntPtr objectPointer, Paint paint);
        public void DrawImage(IntPtr objectPointer, Image image, int x, int y, Paint paint);
    }
}
