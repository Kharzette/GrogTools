Console.WriteLine("Hello, World!");

int	i;
int	numTypes	=MeshLib.VertexTypes.GetNumTypes();

//dump sizes
for(i=0;i < numTypes - 9;i+=10)
{
	Console.WriteLine(""
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+1) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+2) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+3) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+4) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+5) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+6) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+7) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+8) + ", "
		+ MeshLib.VertexTypes.GetSizeForTypeIndex(i+9) + ", ");
}

//last lines
for(;i < numTypes;i++)
{
	Console.WriteLine("" + MeshLib.VertexTypes.GetSizeForTypeIndex(i) + ", ");
}

//Console.WriteLine("rem:" + (i - numTypes));

//dump names
for(i=0;i < MeshLib.VertexTypes.GetNumTypes() - 4;i+=5)
{
	Console.WriteLine(""
		+ "\"" + MeshLib.VertexTypes.GetTypeForIndex(i).Name + "\", "
		+ "\"" + MeshLib.VertexTypes.GetTypeForIndex(i+1).Name + "\", "
		+ "\"" + MeshLib.VertexTypes.GetTypeForIndex(i+2).Name + "\", "
		+ "\"" + MeshLib.VertexTypes.GetTypeForIndex(i+3).Name + "\", "
		+ "\"" + MeshLib.VertexTypes.GetTypeForIndex(i+4).Name + "\", ");
}

for(;i < numTypes;i++)
{
	Console.WriteLine("" + "\"" + MeshLib.VertexTypes.GetTypeForIndex(i).Name + "\", ");
}