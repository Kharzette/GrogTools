using System;

namespace ZoneBuild
{
#if WINDOWS || XBOX
    static class Program
    {
		[STAThread]
        static void Main(string[] args)
        {
            using (ZoneBuild game = new ZoneBuild())
            {
                game.Run();
				ZBSettings.Default.Save();
            }
        }
    }
#endif
}

