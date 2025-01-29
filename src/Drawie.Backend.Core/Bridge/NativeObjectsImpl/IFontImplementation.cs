﻿using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IFontImplementation
{
    public object GetNative(IntPtr objectPointer);
    public void Dispose(IntPtr objectPointer);
    public Font FromStream(Stream stream, float fontSize, float scaleX, float skewY);
    public double GetFontSize(IntPtr objectPointer);
    public void SetFontSize(IntPtr objectPointer, double value);
    public double MeasureText(IntPtr objectPointer, string text);
    public Font CreateDefault(float fontSize);
    public Font? FromFamilyName(string familyName);
    public VectorPath GetTextPath(IntPtr objectPointer, string text);
    public double MeasureText(IntPtr objectPointer, string text, out RectD bounds, Paint? paint = null);
    public int BreakText(IntPtr objectPointer, string text, double maxWidth, out float measuredWidth);
}
