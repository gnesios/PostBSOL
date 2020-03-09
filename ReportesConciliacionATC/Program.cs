using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ReportesCruceATC
{
    static class Program
    {
        public static string gArgumentos;
        public static int gAutomatico;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            gArgumentos = string.Empty;
            gAutomatico = 0;
            foreach (string vArgumento in args)
            {
                if ("/auto" == vArgumento.ToLower())
                {
                    gArgumentos = vArgumento;
                    gAutomatico = 1;
                }
            }
            Application.Run(new ReporteCruce());
        }
    }
}
