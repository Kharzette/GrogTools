using System;

namespace ParticleEdit
{
#if WINDOWS || XBOX
static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[STAThread]
        static void Main(string[] args)
        {
            using (ParticleEdit game = new ParticleEdit())
            {
                game.Run();
				Properties.Settings.Default.Save();
            }
        }
    }
#endif
}

