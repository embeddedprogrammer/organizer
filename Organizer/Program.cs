using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Organizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			Form1.runningForm = new Form1();
			Application.Run(Form1.runningForm);
        }
    }
}