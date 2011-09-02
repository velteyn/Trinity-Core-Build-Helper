using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Trinity_Core_Build_Helper_2008
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
            System.Console.Beep();
            //salvo i settings
            Properties.Settings.Default.Save();
        }
    }
}
