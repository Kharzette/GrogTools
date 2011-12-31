using System;

namespace FullBuild
{
#if WINDOWS || XBOX
    static class Program
    {
		[STAThread]
        static void Main(string[] args)
        {
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            using (FullBuild game = new FullBuild())
            {
                game.Run();
				FBSettings.Default.Save();
            }
        }
    }
#endif
}

