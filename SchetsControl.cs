using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;

public class SchetsControl : UserControl
{
    private Schets schets;
    private Color penkleur;

    public LijstVormen Lijst = new LijstVormen();
    public Vormen HuidigePreviewVorm { get; set; }

    public Color PenKleur => penkleur;
    public Schets Schets => schets;

    public bool RaaktVorm(Vormen v, Point p1, Point p2)
    {
        Point p = p1; // we testen alleen het beginpunt van de gum-lijn

        // cirkels
        if (v.soort == "cirkel" || v.soort == "omtrek")
            return RaaktCirkel(v, p, 4);

        // gevulde rechthoek
        if (v.soort == "GevuldeRechthoek")
        {
            Rectangle r = TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt);
            return r.Contains(p);
        }
        if (v.soort == "tekst")
        {
            Rectangle r = TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt);
            return r.Contains(p);
        }

        if (v.soort == "kader")
            return RaaktKader(v, p, 3);

        if (v.soort == "lijn")
            return RaaktLijn(p, v.startpunt, v.eindpunt, 4);


        // fallback (lijnen etc.)
        Rectangle gumRect = LineToRect(p1, p2, 2);
        Rectangle vorm = TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt);
        return vorm.IntersectsWith(gumRect);
    }
    private Rectangle LineToRect(Point p1, Point p2, int thickness)
    {
        int minX = Math.Min(p1.X, p2.X) - thickness;
        int minY = Math.Min(p1.Y, p2.Y) - thickness;
        int width = Math.Abs(p1.X - p2.X) + thickness * 2;
        int height = Math.Abs(p1.Y - p2.Y) + thickness * 2;

        return new Rectangle(minX, minY, width, height);
    }
    // berekening cirkel https://mathworld.wolfram.com/Ellipse.html 
    private bool RaaktCirkel(Vormen v, Point p, int marge)
    {
        Rectangle r = TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt);

        // centrum van de ellipse
        float cx = r.X + r.Width / 2f;
        float cy = r.Y + r.Height / 2f;

        // genormaliseerde afstand tot centrum
        float dx = (p.X - cx) / (r.Width / 2f);
        float dy = (p.Y - cy) / (r.Height / 2f);

        float afstand = dx * dx + dy * dy;

        if (v.soort == "cirkel") // gevulde cirkel
            return afstand <= 1f; // binnen de ellipse

        if (v.soort == "omtrek") // rand-cirkel
            return Math.Abs(afstand - 1f) <= (marge / 10f);

        return false;
    }
    private bool RaaktKader(Vormen v, Point p, int marge)
    {
        Rectangle r = TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt);

        // binnenste rechthoek (randdikte = marge)
        Rectangle binnen = Rectangle.Inflate(r, -marge, -marge);

        return r.Contains(p) && !binnen.Contains(p);
    }
    private bool RaaktLijn(Point p, Point a, Point b, int marge)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;

        if (dx == 0 && dy == 0)
            return (Math.Abs(p.X - a.X) <= marge && Math.Abs(p.Y - a.Y) <= marge);

        float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
        t = Math.Max(0, Math.Min(1, t));

        float projX = a.X + t * dx;
        float projY = a.Y + t * dy;

        float dist = (float)Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));

        return dist <= marge;
    }

    public SchetsControl()
    {
        this.DoubleBuffered = true;
        this.BorderStyle = BorderStyle.Fixed3D;
        this.schets = new Schets();
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    private void teken(object o, PaintEventArgs pea)
    {
        schets.Teken(pea.Graphics);

        // 1. opgeslagen vormen
        foreach (var vorm in Lijst.GetekendeVormen)
            TekenVorm(pea.Graphics, vorm);

        // 2. preview
        if (HuidigePreviewVorm != null)
            TekenVorm(pea.Graphics, HuidigePreviewVorm);
    }

    private void TekenVorm(Graphics g, Vormen v)
    {
        Font font = new Font("Tahoma", 40);
        switch (v.soort)
        {
            case "kader":
                g.DrawRectangle(new Pen(v.kleur, 3),
                    TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt));
                break;

            case "vlak":
                g.FillRectangle(new SolidBrush(v.kleur),
                    TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt));
                break;

            case "omtrek":
                g.DrawEllipse(new Pen(v.kleur, 3),
                    TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt));
                break;

            case "cirkel":
                g.FillEllipse(new SolidBrush(v.kleur),
                    TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt));
                break;

            case "lijn":
                g.DrawLine(new Pen(v.kleur, 3), v.startpunt, v.eindpunt);
                break;
            case "tekst":
                g.DrawRectangle(new Pen(Color.Black, 3), TweepuntTool.Punten2Rechthoek(v.startpunt, v.eindpunt));
                g.DrawString(v.tekst,font, new SolidBrush(v.kleur), v.startpunt, StringFormat.GenericTypographic);
                break;
        }
    }

    private void veranderAfmeting(object o, EventArgs ea)
    {
        schets.VeranderAfmeting(this.ClientSize);
        this.Invalidate();
    }

    public Graphics MaakBitmapGraphics()
    {
        Graphics g = schets.BitmapGraphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        return g;
    }

    public void Schoon(object o, EventArgs ea)
    {
        schets.Schoon();
        this.Invalidate();
    }

    public void Roteer(object o, EventArgs ea)
    {
        schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
        schets.Roteer();
        this.Invalidate();
    }

    public void VeranderKleur(object obj, EventArgs ea)
    {
        string kleurNaam = ((ComboBox)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }

    public void VeranderKleurViaMenu(object obj, EventArgs ea)
    {
        string kleurNaam = ((ToolStripMenuItem)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
}