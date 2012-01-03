using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VisServer
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

			SharedForms.VisForm	visForm	=new SharedForms.VisForm();
			SharedForms.Output	outForm	=new SharedForms.Output();

			VisApp	va	=new VisApp(visForm, outForm);

			outForm.Show();

			Application.Run(visForm);
		}
	}
}
