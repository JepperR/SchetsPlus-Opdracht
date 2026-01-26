using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Linq;

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
    {
        startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
    }
    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }
    private string huidigeTekst = "";

    public override void Letter(SchetsControl s, char c)
    {
        if (c >= 32)
        {
            Graphics gr = s.MaakBitmapGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            huidigeTekst += tekst;   // voeg letter toe aan de string

            Point letterStart = new Point(startpunt.X, startpunt.Y);

            SizeF sz =
            gr.MeasureString(tekst, font, letterStart, StringFormat.GenericTypographic);
            gr.DrawRectangle(Pens.Transparent, startpunt.X, startpunt.Y, sz.Width, sz.Height);
            startpunt.X += (int)sz.Width;
            s.Lijst.GetekendeVormen.Add(new Vormen
            {
                soort = "tekst",
                startpunt = letterStart,
                eindpunt = new Point(letterStart.X + (int)sz.Width, letterStart.Y + (int)sz.Height),
                kleur = s.PenKleur,
                tekst = huidigeTekst
            });
            startpunt.X += (int)sz.Width;
            huidigeTekst = ""; // reset voor volgende tekst

            s.Invalidate();
        }
    }

}
public abstract class TweepuntTool : StartpuntTool
{
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {
        return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                            , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                            );
    }
    public static Pen MaakPen(Brush b, int dikte)
    {
        Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    public override void MuisVast(SchetsControl s, Point p)
    {
        base.MuisVast(s, p);
        kwast = Brushes.Gray;
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {
        s.HuidigePreviewVorm = new Vormen
        {
            soort = this.ToString(),
            startpunt = startpunt,
            eindpunt = p,
            kleur = s.PenKleur,
            tekst = ""
        };

        s.Refresh();
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
        eindpunt = p;
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2);

    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {
        this.Bezig(g, p1, p2);
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
            kleur = s.PenKleur,
            tekst = ""
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
            kleur = s.PenKleur,
            tekst = ""
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
            kleur = s.PenKleur,
            tekst = ""
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
            kleur = s.PenKleur,
            tekst = ""
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
    {
        g.DrawLine(MaakPen(this.kwast, 3), p1, p2);
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
            kleur = s.PenKleur,
            tekst = ""
        });

        // 3. preview wissen
        s.HuidigePreviewVorm = null;

        s.Invalidate();
    }
}

public class PenTool : LijnTool
{
    private Vormen huidigePen;

    public override string ToString() { return "pen"; }

    public override void MuisVast (SchetsControl s, Point p)
    {
        startpunt = p;

        huidigePen = new Vormen
        {
            soort = "pen",
            kleur = s.PenKleur,
            punten = new List<Point> { p }
        };

        s.Lijst.GetekendeVormen.Add(huidigePen);

    }

    public override void MuisDrag(SchetsControl s, Point p)
    {
        huidigePen.punten.Add(p);
        startpunt = p;

        s.Invalidate();
    }

    public override void MuisLos (SchetsControl s, Point p)
    {
        huidigePen.punten.Add(p);
        huidigePen = null;
    }
}

public class GumTool : LijnTool
{
    public override string ToString() { return "gum"; }

    public override void MuisLos(SchetsControl s, Point p)
    {
        // 1. gum tekenen op bitmap
        Graphics g = s.MaakBitmapGraphics();
        //g.DrawLine(MaakPen(Brushes.White, 1), startpunt, p);

        // 2. check welke vormen geraakt worden
        List<Vormen> teVerwijderen = new List<Vormen>();

        for (int i = s.Lijst.GetekendeVormen.Count - 1; i >= 0; i--)
        {
            if (s.RaaktVorm(s.Lijst.GetekendeVormen[i], startpunt, p))
                teVerwijderen.Add(s.Lijst.GetekendeVormen[i]);
            break;
        }

        // 3. verwijder vormen
        foreach (var v in teVerwijderen)
            s.Lijst.GetekendeVormen.Remove(v);

        startpunt = p;

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
    public string tekst { get; set; }

    // wordt alleen bij pen gebruikt
    public List<Point> punten;

    public string naarString()
    {
        string vormNaarString = "";

        switch(soort)
        {
            case "kader":
                vormNaarString = $"{soort} {startpunt.X} {startpunt.Y} {eindpunt.X} {eindpunt.Y} {kleur.R} {kleur.G} {kleur.B}";
                break;

            case "vlak":
                vormNaarString = $"{soort} {startpunt.X} {startpunt.Y} {eindpunt.X} {eindpunt.Y} {kleur.R} {kleur.G} {kleur.B}";
                break;

            case "omtrek":
                vormNaarString = $"{soort} {startpunt.X} {startpunt.Y} {eindpunt.X} {eindpunt.Y} {kleur.R} {kleur.G} {kleur.B}";
                break;

            case "cirkel":
                vormNaarString = $"{soort} {startpunt.X} {startpunt.Y} {eindpunt.X} {eindpunt.Y} {kleur.R} {kleur.G} {kleur.B}";
                break;

            case "lijn":
                vormNaarString = $"{soort} {startpunt.X} {startpunt.Y} {eindpunt.X} {eindpunt.Y} {kleur.R} {kleur.G} {kleur.B}";
                break;

            case "tekst":
                vormNaarString = $"{soort} {startpunt.X} {startpunt.Y} {eindpunt.X} {eindpunt.Y} {kleur.R} {kleur.G} {kleur.B} {tekst}";
                break;

            case "pen":
                vormNaarString = $"{soort} {kleur.R} {kleur.G} {kleur.B}";

                foreach (Point p in punten)
                {
                    vormNaarString += $" {p.X} {p.Y}";
                }
                break;
        }

        return vormNaarString;
    }

    public static Vormen naarVorm(string invoer)
    {
        string[] variablen = invoer.Split();
        Vormen stringNaarVorm = new Vormen();
        stringNaarVorm.soort = variablen[0];

        switch (invoer)
        {
            case "kader":
                stringNaarVorm.startpunt = new Point(int.Parse(variablen[1]), int.Parse(variablen[2]));
                stringNaarVorm.eindpunt = new Point(int.Parse(variablen[3]), int.Parse(variablen[4]));
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                break;

            case "vlak":
                stringNaarVorm.startpunt = new Point(int.Parse(variablen[1]), int.Parse(variablen[2]));
                stringNaarVorm.eindpunt = new Point(int.Parse(variablen[3]), int.Parse(variablen[4]));
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                break;

            case "omtrek":
                stringNaarVorm.startpunt = new Point(int.Parse(variablen[1]), int.Parse(variablen[2]));
                stringNaarVorm.eindpunt = new Point(int.Parse(variablen[3]), int.Parse(variablen[4]));
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                break;

            case "cirkel":
                stringNaarVorm.startpunt = new Point(int.Parse(variablen[1]), int.Parse(variablen[2]));
                stringNaarVorm.eindpunt = new Point(int.Parse(variablen[3]), int.Parse(variablen[4]));
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                break;

            case "lijn":
                stringNaarVorm.startpunt = new Point(int.Parse(variablen[1]), int.Parse(variablen[2]));
                stringNaarVorm.eindpunt = new Point(int.Parse(variablen[3]), int.Parse(variablen[4]));
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                break;

            case "tekst":
                stringNaarVorm.startpunt = new Point(int.Parse(variablen[1]), int.Parse(variablen[2]));
                stringNaarVorm.eindpunt = new Point(int.Parse(variablen[3]), int.Parse(variablen[4]));
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                stringNaarVorm.tekst = string.Join(" ", variablen.Skip(8));
                break; break;

            case "pen":
                stringNaarVorm.kleur = Color.FromArgb(int.Parse(variablen[5]), int.Parse(variablen[6]), int.Parse(variablen[7]));
                stringNaarVorm.punten = new List<Point>();

                for (int n = 4; n < variablen.Length; n++)
                {
                    int x = int.Parse(variablen[n]);
                    int y = int.Parse(variablen[n + 1]);
                    stringNaarVorm.punten.Add(new Point(x, y));
                }
                break;
        }

        return stringNaarVorm;
    }
}