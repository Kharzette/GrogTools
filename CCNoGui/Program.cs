using System.Numerics;
using System.Diagnostics;
using MeshLib;
using UtilityLib;


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

		string	someArg		="";
		string	fileName	="";
		bool	bStatic		=false;
		bool	bChar		=false;
		bool	bAnim		=false;

		foreach(string arg in args)
		{
			if(arg.StartsWith("-static"))
			{
				bStatic		=true;
				fileName	=arg.Substring(8);
			}
			else if(arg.StartsWith("-character"))
			{
				bChar		=true;
				fileName	=arg.Substring(11);
			}
			else if(arg.StartsWith("-anim"))
			{
				bAnim		=true;
				fileName	=arg.Substring(6);
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
			if(!File.Exists(fileName))
			{
				Console.WriteLine("File: " + fileName + " not found.");
				return;
			}

			StaticMesh	sm;
			ColladaData.LoadStaticDAE(fileName, sf, out sm);

			sm.GenerateRoughBounds();

			//print some stats
			int	numParts	=sm.GetPartCount();

			Console.WriteLine("Static mesh " + fileName + " loaded with "
								+ numParts + " parts:");

			for(int i=0;i < numParts;i++)
			{
				string		name		=sm.GetPartName(i);
				Matrix4x4	partMat		=sm.GetPartTransform(i);
				Type		partType	=sm.GetPartVertexType(i);

				Console.WriteLine("Part " + i + ": " + name +
					" with identity " + partMat.IsIdentity +
					", and vertex type: " + partType.ToString());
			}

			string	smFile	=FileUtil.StripExtension(fileName);

			smFile	+=".Static";

			Console.WriteLine("Saving to " + smFile);

			sm.SaveToFile(smFile);

			//save base mesh too
			string	path	=FileUtil.StripFileName(fileName);
			sm.SaveParts(path);
		}
		else if(bChar)
		{
			if(!File.Exists(fileName))
			{
				Console.WriteLine("File: " + fileName + " not found.");
				return;
			}

			AnimLib	alib	=new AnimLib();

			Character	c	=ColladaData.LoadCharacterDAE(fileName, sf, alib);

			c.GenerateRoughBounds();

			//print some stats
			int	numParts	=c.GetPartCount();

			Console.WriteLine("Character mesh " + fileName + " loaded with "
								+ numParts + " parts:");

			for(int i=0;i < numParts;i++)
			{
				string		name		=c.GetPartName(i);
				Matrix4x4	partMat		=c.GetPartTransform(i);
				Type		partType	=c.GetPartVertexType(i);

				Console.WriteLine("Part " + i + ": " + name +
					" with identity " + partMat.IsIdentity +
					", and vertex type: " + partType.ToString());
			}

			string	smFile	=FileUtil.StripExtension(fileName);

			smFile	+=".Character";

			Console.WriteLine("Saving to " + smFile);

			c.SaveToFile(smFile);

			//save base mesh too
			string	path	=FileUtil.StripFileName(fileName);
			c.SaveParts(path);
		}

		Console.WriteLine("Done!");
	}
}