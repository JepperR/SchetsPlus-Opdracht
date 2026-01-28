using System;
using System.Drawing;
using System.Windows.Forms;

public class SchetsEditor : Form
{
    private MenuStrip menuStrip;

    public SchetsEditor()
    {   
        this.ClientSize = new Size(800, 600);
        menuStrip = new MenuStrip();
        this.Controls.Add(menuStrip);
        this.maakFileMenu();
        this.maakHelpMenu();
        this.Text = "Schets editor";
        this.IsMdiContainer = true;
        this.MainMenuStrip = menuStrip;
    }
    private void maakFileMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("File");
        menu.DropDownItems.Add("Nieuw", null, this.nieuw);
        menu.DropDownItems.Add("Open", null, this.open);
        menu.DropDownItems.Add("Opslaan", null, this.opslaan);
        menu.DropDownItems.Add("Opslaan als", null, this.opslaanAls);
        menu.DropDownItems.Add("Exit", null, this.afsluiten);
        menuStrip.Items.Add(menu);
    }
    private void maakHelpMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("Help");
        menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
        menuStrip.Items.Add(menu);
    }
    private void about(object o, EventArgs ea)
    {   
        MessageBox.Show ( "Schets versie 2.0\n(c) UU Informatica 2024"
                        , "Over \"Schets\""
                        , MessageBoxButtons.OK
                        , MessageBoxIcon.Information
                        );
    }

    private void nieuw(object sender, EventArgs e)
    {   
        SchetsWin s = new SchetsWin();
        s.MdiParent = this;
        s.Show();
    }
    private void afsluiten(object sender, EventArgs e)
    {   
        this.Close();
    }

    private void open(object o, EventArgs e)
    {
        OpenFileDialog dialoog = new OpenFileDialog();
        dialoog.Filter = "Tekstfiles|*.txt|Alle files|*.*";
        dialoog.Title = "Tekst openen...";

        if (dialoog.ShowDialog() == DialogResult.OK)
        {
            SchetsWin s = new SchetsWin();
            s.MdiParent = this;
            s.huidigSchetsControl.LeesVanFile(dialoog.FileName);
            s.Show();
        }
    }

    private void opslaanAls(object o, EventArgs e)
    {
        SchetsWin actief = this.ActiveMdiChild as SchetsWin;
        
        SaveFileDialog dialoog = new SaveFileDialog();
        dialoog.Filter = "Tekstfiles|*.txt|Alle files|*.*";
        dialoog.Title = "Tekst opslaan als...";
        if (dialoog.ShowDialog() == DialogResult.OK)
        {
            actief.huidigSchetsControl.setBestandsNaam(dialoog.FileName);
            actief.huidigSchetsControl.SchrijfNaarFile();
        }
    }

    private void opslaan(object o, EventArgs e)
    {
        SchetsWin actief = this.ActiveMdiChild as SchetsWin;

        if (Text == "")
            opslaanAls(o, e);
        else actief.huidigSchetsControl.SchrijfNaarFile();
    }
}