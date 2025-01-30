﻿using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Text;

public class Font : NativeObject
{
    public Font(IntPtr objPtr) : base(objPtr)
    {
    }

    public override object Native => DrawingBackendApi.Current.FontImplementation.GetNative(ObjectPointer);

    public double Size
    {
        get => DrawingBackendApi.Current.FontImplementation.GetFontSize(ObjectPointer);
        set => DrawingBackendApi.Current.FontImplementation.SetFontSize(ObjectPointer, value);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.FontImplementation.Dispose(ObjectPointer);
    }

    public static Font FromStream(Stream stream, float fontSize = 12f, float scaleX = 1f, float skewY = 0f)
    {
        return DrawingBackendApi.Current.FontImplementation.FromStream(stream, fontSize, scaleX, skewY);
    }

    public double MeasureText(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.MeasureText(ObjectPointer, text);
    }

    public double MeasureText(string text, out RectD rectD, Paint? paint = null)
    {
        return DrawingBackendApi.Current.FontImplementation.MeasureText(ObjectPointer, text, out rectD, paint);
    }

    public int BreakText(string text, double maxWidth, out float measuredWidth)
    {
        return DrawingBackendApi.Current.FontImplementation.BreakText(ObjectPointer, text, maxWidth, out measuredWidth);
    }

    public VectorPath GetTextPath(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.GetTextPath(ObjectPointer, text);
    }

    public static Font CreateDefault(float fontSize = 12f)
    {
        return DrawingBackendApi.Current.FontImplementation.CreateDefault(fontSize);
    }

    public static Font? FromFamilyName(string familyName)
    {
        return DrawingBackendApi.Current.FontImplementation.FromFamilyName(familyName);
    }

    public static Font? FromFontFamily(FontFamilyName familyName)
    {
        if (familyName.FontUri != null)
        {
            bool isFile = familyName.FontUri.IsFile;
            if (isFile)
            {
                using var stream = File.OpenRead(familyName.FontUri.LocalPath);
                return FromStream(stream);
            }
        }

        return DrawingBackendApi.Current.FontImplementation.FromFamilyName(familyName.Name);
    }

    public VecF[] GetGlyphPositions(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.GetGlyphPositions(ObjectPointer, text);
    }

    public float[] GetGlyphWidths(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.GetGlyphWidths(ObjectPointer, text);
    }
}
