using System;
using System.Windows.Forms;

static partial class Program
{
    [STAThreadAttribute]
    static void Main()
    {
        Application.Run(new SchetsEditor());
    }
}