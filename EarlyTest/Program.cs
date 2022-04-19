using System;
using System.IO;
using System.Drawing;
using System.Numerics;
using UtilityLib;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.D3DCompiler;
using Vortice.Dxc;
using SharpGen.Runtime;

//renderform and renderloop
using SharpDX.Windows;

namespace	EarlyTest;

internal class Program
{
	//for shader includes
	internal class IncludeFX : CallbackBase, Include
	{
		string	mRootDir;

		internal IncludeFX(string rootDir)
		{
			mRootDir	=rootDir;
		}

		static string includeDirectory = "Shaders\\";
		public void Close(Stream stream)
		{
			stream.Close();
			stream.Dispose();
		}

		public Stream Open(IncludeType type, string fileName, Stream ?parentStream)
		{
			return	new FileStream(mRootDir + "\\" + includeDirectory + fileName, FileMode.Open);
		}
	}

	struct PerObject
	{
		internal Matrix4x4	mWorld;
		internal Vector4	mSolidColour;
		internal Vector4	mSpecColor;

		//These are considered directional (no falloff)
		internal Vector4	mLightColor0;		//trilights need 3 colors
		internal Vector4	mLightColor1;		//trilights need 3 colors
		internal Vector4	mLightColor2;		//trilights need 3 colors

		internal Vector3	mLightDirection;
		internal float		mSpecPower;
	}

	struct PerFrame
	{
		internal Matrix4x4	mView;
		internal Matrix4x4	mLightViewProj;	//for shadows
		internal Vector3	mEyePos;
		internal UInt32		mPadding;
	}

	struct ChangeLess
	{
		internal Matrix4x4	mProjection;
	}

	[STAThread]
	static unsafe void Main(string []args)
	{
		Console.WriteLine("Hello, World!");

		Icon	testIcon	=new Icon("1281737606553.ico");

		FeatureLevel	f	=FeatureLevel.Level_11_0;

		GraphicsDevice	gd	=new GraphicsDevice("Goblin Test", testIcon, f, 0.1f, 2000f);

		//I have no idea what this does
		Application.EnableVisualStyles();

		IncludeFX	inc	=new IncludeFX(".");

		ShaderMacro	[]macs	=new ShaderMacro[2];

		macs[0]	=new ShaderMacro("SM5", 1);

		Blob	codeBlob, errBlob;

		//vert shader
		Result	res	=Compiler.CompileFromFile("Shaders/Static.hlsl",
			macs, inc, "WNormWPosTexVS",
			"vs_5_0", ShaderFlags.None,
			EffectFlags.None, out codeBlob, out errBlob);

		if(res != Result.Ok)
		{
			Console.WriteLine(errBlob.AsString());
			return;
		}

		System.Span<byte>	vsBytes	=codeBlob.AsSpan();

		ID3D11VertexShader	vs	=gd.GD.CreateVertexShader(vsBytes);

		//pixel shader
		res	=Compiler.CompileFromFile("Shaders/Static.hlsl",
			macs, inc, "TriTex0SpecPS",
			"ps_5_0", ShaderFlags.None,
			EffectFlags.None, out codeBlob, out errBlob);

		if(res != Result.Ok)
		{
			Console.WriteLine(errBlob.AsString());
			return;
		}

		System.Span<byte>	psBytes	=codeBlob.AsSpan();

		ID3D11PixelShader	ps	=gd.GD.CreatePixelShader(psBytes);

		//make some prims to draw
		PrimObject	prism	=PrimFactory.CreatePrism(gd.GD, vsBytes, 5f);
		PrimObject	sphere	=PrimFactory.CreateSphere(gd.GD, vsBytes, Vector3.Zero, 5f);
		PrimObject	box		=PrimFactory.CreateCube(gd.GD, vsBytes, 5f);
		PrimObject	cyl		=PrimFactory.CreateCylinder(gd.GD, vsBytes, 2f, 5f);

		//create constant buffers
		ID3D11Buffer	perObjectBuf	=MakeConstantBuffer(gd, sizeof(PerObject));
		ID3D11Buffer	perFrameBuf		=MakeConstantBuffer(gd, sizeof(PerFrame));
		ID3D11Buffer	changeLessBuf	=MakeConstantBuffer(gd, sizeof(ChangeLess));

		//alloc C# side constant buffer data
		PerObject	perObject	=new PerObject();
		PerFrame	perFrame	=new PerFrame();
		ChangeLess	changeLess	=new ChangeLess();

		perObject.mLightColor0	=Vector4.One;
		perObject.mLightColor1	=Vector4.One * 0.3f;
		perObject.mLightColor2	=Vector4.One * 0.2f;

		perObject.mLightColor1.W	=perObject.mLightColor2.W	=1f;

		perObject.mLightDirection	=-Vector3.UnitY;
		perObject.mSolidColour		=Vector4.One;
		perObject.mSpecColor		=Vector4.One;
		perObject.mSpecPower		=5f;

		changeLess.mProjection		=Matrix4x4.Transpose(gd.GCam.Projection);

		//put values in changeLess
		gd.DC.UpdateSubresource<ChangeLess>(changeLess, changeLessBuf);

		//assign cbuffers to shaders
		gd.DC.VSSetConstantBuffer(0, perObjectBuf);
		gd.DC.PSSetConstantBuffer(0, perObjectBuf);
		gd.DC.VSSetConstantBuffer(1, perFrameBuf);
		gd.DC.PSSetConstantBuffer(1, perFrameBuf);
		gd.DC.VSSetConstantBuffer(2, changeLessBuf);
		gd.DC.PSSetConstantBuffer(2, changeLessBuf);

		gd.DC.VSSetShader(vs);
		gd.DC.PSSetShader(ps);

		Random	rnd	=new Random();

		//randomize colours of the objects
		Vector4	prismColour		=Mathery.RandomColorVector4(rnd);
		Vector4	sphereColour	=Mathery.RandomColorVector4(rnd);
		Vector4	boxColour		=Mathery.RandomColorVector4(rnd);
		Vector4	cylColour		=Mathery.RandomColorVector4(rnd);

		Vector3	yawPitchRoll	=Vector3.Zero;

		prism.World		=Matrix4x4.CreateTranslation(Vector3.UnitX * 15f);
		sphere.World	=Matrix4x4.CreateTranslation(Vector3.UnitX * -15f);
		box.World		=Matrix4x4.CreateTranslation(Vector3.UnitZ * 15f);
		cyl.World		=Matrix4x4.CreateTranslation(Vector3.UnitZ * -15f);
		
		RenderLoop.Run(gd.RendForm, () =>
		{
			gd.CheckResize();

			gd.ClearViews();

			yawPitchRoll.X	+=0.0001f;
			yawPitchRoll.Y	+=0.00005f;
			yawPitchRoll.Z	+=0.00007f;

			Mathery.WrapAngleDegrees(ref yawPitchRoll.X);
			Mathery.WrapAngleDegrees(ref yawPitchRoll.Y);
			Mathery.WrapAngleDegrees(ref yawPitchRoll.Z);

			perObject.mLightDirection	=Vector3.TransformNormal(Vector3.UnitX,
				Matrix4x4.CreateFromYawPitchRoll(yawPitchRoll.X,
					yawPitchRoll.Y, yawPitchRoll.Z));

			gd.GCam.Update(Vector3.UnitZ * 35f + Vector3.UnitY * 10f, 20, 182, 0);

			//update perframe data
			perFrame.mEyePos		=gd.GCam.Position;
			perFrame.mLightViewProj	=Matrix4x4.Transpose(Matrix4x4.Identity);
			perFrame.mView			=Matrix4x4.Transpose(gd.GCam.View);

			//update values in perFrame
			gd.DC.UpdateSubresource<PerFrame>(perFrame, perFrameBuf);

			//per object shader vars
			perObject.mWorld		=Matrix4x4.Transpose(prism.World);
			perObject.mSolidColour	=prismColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			prism.Draw(gd);

			perObject.mWorld		=Matrix4x4.Transpose(sphere.World);
			perObject.mSolidColour	=sphereColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			sphere.Draw(gd);

			perObject.mWorld		=Matrix4x4.Transpose(box.World);
			perObject.mSolidColour	=boxColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			box.Draw(gd);

			perObject.mWorld		=Matrix4x4.Transpose(cyl.World);
			perObject.mSolidColour	=cylColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			cyl.Draw(gd);

			gd.Present();
		});

		gd.ReleaseAll();
	}

	static ID3D11Buffer	MakeConstantBuffer(GraphicsDevice gd, int size)
	{
		BufferDescription	cbDesc	=new BufferDescription();

		cbDesc.BindFlags			=BindFlags.ConstantBuffer;
		cbDesc.ByteWidth			=size;
		cbDesc.CPUAccessFlags		=CpuAccessFlags.None;
		cbDesc.MiscFlags			=ResourceOptionFlags.None;
		cbDesc.Usage				=ResourceUsage.Default;

		//alloc
		return	gd.GD.CreateBuffer(cbDesc);
	}
}