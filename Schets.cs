using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

public class Schets
{
    private Bitmap bitmap;

    public Schets()
    {
        bitmap = new Bitmap(1, 1);
    }
    public Graphics BitmapGraphics
    {
        get { return Graphics.FromImage(bitmap); }
    }
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap(Math.Max(sz.Width, bitmap.Size.Width)
                                     , Math.Max(sz.Height, bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(bitmap, 0, 0);
            bitmap = nieuw;
        }
    }
    public void Teken(Graphics gr)
    {
        gr.DrawImage(bitmap, 0, 0);
    }
    public void Schoon()
    {
        Graphics gr = Graphics.FromImage(bitmap);
        gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
    }
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
    }
}
public abstract class SchetsObject
{
    public Color Kleur { get; set; }
    public abstract void Teken(Graphics g);
}

public class LijnObject : SchetsObject
{
    public Point StartPunt { get; set; }
    public Point EindPunt { get; set; }

    public override void Teken(Graphics g)
    {
        using (var pen = new Pen(Kleur, 2))
            g.DrawLine(pen, StartPunt, EindPunt);
    }
}

public class RechthoekObject
{
    public Rectangle Rect { get; set; }
    public Color Kleur { get; set; }

    public void Teken(Graphics g)
    {
        using (var pen = new Pen(Kleur, 3))
        {
            g.DrawRectangle(pen, Rect);
        }
    }
}