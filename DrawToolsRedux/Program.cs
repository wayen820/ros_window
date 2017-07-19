using System;
using System.Windows.Forms;

namespace DrawTools
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
            new DrawTools.ROS.TouchControl.Window2();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
		}
	}
}