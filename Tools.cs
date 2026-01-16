using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

public interface ISchetsTool
{
    void MuisVast(SchetsControl s, Point p);
    void MuisDrag(SchetsControl s, Point p);
    void MuisLos(SchetsControl s, Point p);
    void Letter(SchetsControl s, char c);
}

public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Point eindpunt;
    protected Brush kwast;

    public virtual void MuisVast(SchetsControl s, Point p)
    {   startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {   kwast = new SolidBrush(s.PenKleur);
    }
    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c)
    {
        if (c >= 32)
        {
            Graphics gr = s.MaakBitmapGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = 
            gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
            gr.DrawString   (tekst, font, kwast, 
                                            this.startpunt, StringFormat.GenericTypographic);
            // gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
            startpunt.X += (int)sz.Width;
            s.Invalidate();
        }
    }
}
public abstract class TweepuntTool : StartpuntTool
{ 
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                            , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                            );
    }
    public static Pen MaakPen(Brush b, int dikte)
    {   Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    public override void MuisVast(SchetsControl s, Point p)
    {   base.MuisVast(s, p);
        kwast = Brushes.Gray;
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {
        s.HuidigePreviewVorm = new Vormen
        {
            soort = this.ToString(),
            startpunt = startpunt,
            eindpunt = p,
            kleur = s.PenKleur
        };

        s.Refresh();
    }
    public override void MuisLos(SchetsControl s, Point p)
    {   base.MuisLos(s, p);
        eindpunt = p;
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2);
        
    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {   this.Bezig(g, p1, p2);
    }
}

public class RechthoekTool : TweepuntTool
{
    public override string ToString() { return "kader"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        // punten opslaan
        startpunt = this.startpunt;
        eindpunt = p;

        // 1. teken op de bitmap
        Graphics g = s.MaakBitmapGraphics();

        // 2. sla op in de lijst
        s.Lijst.GetekendeVormen.Add(new Vormen
        {
            soort = "kader",
            startpunt = startpunt,
            eindpunt = eindpunt,
            kleur = s.PenKleur
        });

        // 3. preview wissen
        s.HuidigePreviewVorm = null;

        s.Invalidate();
    }
}


public class VolRechthoekTool : RechthoekTool
{

    public override string ToString() { return "vlak"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        startpunt = p1;
        Point eindpunt = p2;
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        // punten opslaan
        startpunt = this.startpunt;
        eindpunt = p;

        // 1. teken op de bitmap
        Graphics g = s.MaakBitmapGraphics();

        // 2. sla op in de lijst
        s.Lijst.GetekendeVormen.Add(new Vormen
        {
            soort = "vlak",
            startpunt = startpunt,
            eindpunt = eindpunt,
            kleur = s.PenKleur
        });

        // 3. preview wissen
        s.HuidigePreviewVorm = null;

        s.Invalidate();
    }
}


    public class CirkelTool : TweepuntTool
{
    public override string ToString() { return "omtrek"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        // punten opslaan
        startpunt = this.startpunt;
        eindpunt = p;

        // 1. teken op de bitmap
        Graphics g = s.MaakBitmapGraphics();

        // 2. sla op in de lijst
        s.Lijst.GetekendeVormen.Add(new Vormen
        {
            soort = "omtrek",
            startpunt = startpunt,
            eindpunt = eindpunt,
            kleur = s.PenKleur
        });

        // 3. preview wissen
        s.HuidigePreviewVorm = null;

        s.Invalidate();
    }
}

public class VolCirkelTool : CirkelTool
{
    public override string ToString() { return "cirkel"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        // punten opslaan
        startpunt = this.startpunt;
        eindpunt = p;

        // 1. teken op de bitmap
        Graphics g = s.MaakBitmapGraphics();

        // 2. sla op in de lijst
        s.Lijst.GetekendeVormen.Add(new Vormen
        {
            soort = "cirkel",
            startpunt = startpunt,
            eindpunt = eindpunt,
            kleur = s.PenKleur
        });

        // 3. preview wissen
        s.HuidigePreviewVorm = null;

        s.Invalidate();
    }
}

    public class LijnTool : TweepuntTool
{
    public override string ToString() { return "lijn"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   g.DrawLine(MaakPen(this.kwast,3), p1, p2);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        // punten opslaan
        startpunt = this.startpunt;
        eindpunt = p;

        // 1. teken op de bitmap
        Graphics g = s.MaakBitmapGraphics();

        // 2. sla op in de lijst
        s.Lijst.GetekendeVormen.Add(new Vormen
        {
            soort = "lijn",
            startpunt = startpunt,
            eindpunt = eindpunt,
            kleur = s.PenKleur
        });

        // 3. preview wissen
        s.HuidigePreviewVorm = null;

        s.Invalidate();
    }
}

public class PenTool : LijnTool
{
    public override string ToString() { return "pen"; }

    public override void MuisDrag(SchetsControl s, Point p)
    {   this.MuisLos(s, p);
        this.MuisVast(s, p);
    }
}
    
public class GumTool : PenTool
{
    public override string ToString() { return "gum"; }

    public override void MuisDrag(SchetsControl s, Point p)
    {
        // 1. gum tekenen op bitmap
        Graphics g = s.MaakBitmapGraphics();
        g.DrawLine(MaakPen(Brushes.White, 10), startpunt, p);

        // 2. check welke vormen geraakt worden
        List<Vormen> teVerwijderen = new List<Vormen>();

        foreach (var v in s.Lijst.GetekendeVormen)
        {
            if (s.RaaktVorm(v, startpunt, p))
                teVerwijderen.Add(v);
        }

        // 3. verwijder vormen
        foreach (var v in teVerwijderen)
            s.Lijst.GetekendeVormen.Remove(v);

        // 4. startpunt updaten voor volgende gum-stukje
        startpunt = p;

        // 5. opnieuw tekenen
        s.Invalidate();
    }
}
public class LijstVormen
{
    public List<Vormen> GetekendeVormen = new List<Vormen>();
}

public class Vormen 
{
   public string soort { get; set; }
   public Point startpunt { get; set; }
   public Point eindpunt { get; set; }
   public Color kleur { get; set; }
}

public class RechthoekVorm : Vormen
{
 
}
public class GevuldeRechthoekVorm : RechthoekVorm
{
   
}

public class CirkelVorm : Vormen
{
    
}
public class GevuldeCirkelVorm : CirkelVorm
{
    
}