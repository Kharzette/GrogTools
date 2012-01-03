using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BSPBuild
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

			SharedForms.BSPForm	bspForm	=new SharedForms.BSPForm();
			SharedForms.Output	outForm	=new SharedForms.Output();

			BSPBuild	bspBuild	=new BSPBuild(bspForm);

			outForm.Show();

			Application.Run(bspForm);
		}
	}
}
