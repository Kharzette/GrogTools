using System.Numerics;
using System.Diagnostics;
using MeshLib;


namespace ColladaConvert;

internal static class Program
{
	static void Main(string []args)
	{
		if(args.Length == 0)
		{
			Console.WriteLine("Usage: -stuff");
			return;
		}

		//default scale factor
		MeshConverter.ScaleFactor	sf	=MeshConverter.ScaleFactor.Meters;

		string	someArg	="";
		bool	bStatic	=false;
		bool	bChar	=false;
		bool	bAnim	=false;

		foreach(string arg in args)
		{
			if(arg == "-static")
			{
				bStatic	=true;
			}
			else if(arg == "-character")
			{
				bChar	=true;
			}
			else if(arg == "-anim")
			{
				bAnim	=true;
			}
			else if(arg == "-centimeters")
			{
				sf	=MeshConverter.ScaleFactor.Centimeters;
			}
			else if(arg == "-grog")
			{
				sf	=MeshConverter.ScaleFactor.Grog;
			}
			else if(arg == "-meters")
			{
				sf	=MeshConverter.ScaleFactor.Meters;
			}
			else if(arg == "-quake")
			{
				sf	=MeshConverter.ScaleFactor.Quake;
			}
			else if(arg == "-valve")
			{
				sf	=MeshConverter.ScaleFactor.Valve;
			}
			else
			{
				someArg	=arg;
			}
		}

		if(bStatic)
		{
			if(!File.Exists(someArg))
			{
				Console.WriteLine("File: " + someArg + " not found.");
			}
		}
	}
}