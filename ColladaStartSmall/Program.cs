﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InputLib;
using MaterialLib;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;


namespace ColladaStartSmall
{
	public class IncludeFX : CallbackBase, Include
	{
		static string includeDirectory = "";
		public void Close(Stream stream)
		{
			stream.Close();
			stream.Dispose();
		}

		public Stream Open(IncludeType type, string fileName, Stream parentStream)
		{
			return	new FileStream(includeDirectory + fileName, FileMode.Open);
		}
	}

	internal static class Program
	{
		static bool	mbResized;

		const int	ResX	=1280;
		const int	ResY	=720;

		enum MyActions
		{
			MoveForward, MoveBack, MoveLeft, MoveRight,
			TurnLeft, TurnRight,
			PitchUp, PitchDown
		};

		static void OnRenderFormResize(object sender, EventArgs ea)
		{
			mbResized	=true;
		}

		static void HandleResize(int width, int height,
			Device dev, DeviceContext dctx,
			ref UtilityLib.GameCamera gcam,
			ref SwapChain swChain, SwapChainDescription scDesc,
			ref Texture2D backBuf, ref RenderTargetView rtView,
			ref Texture2D depthBuf, ref DepthStencilView dsView)
		{
			Utilities.Dispose(ref backBuf);
			Utilities.Dispose(ref rtView);
			Utilities.Dispose(ref depthBuf);
			Utilities.Dispose(ref dsView);

			swChain.ResizeBuffers(scDesc.BufferCount, width, height, Format.Unknown, SwapChainFlags.None);

			backBuf	=Texture2D.FromSwapChain<Texture2D>(swChain, 0);
			rtView	=new RenderTargetView(dev, backBuf);

			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
				Format				=Format.D32_Float_S8X24_UInt,
				ArraySize			=1,
				MipLevels			=1,
				Width				=width,
				Height				=height,
				SampleDescription	=new SampleDescription(1, 0),
				Usage				=ResourceUsage.Default,
				BindFlags			=BindFlags.DepthStencil,
				CpuAccessFlags		=CpuAccessFlags.None,
				OptionFlags			=ResourceOptionFlags.None
			};

			depthBuf	=new Texture2D(dev, depthDesc);
			dsView		=new DepthStencilView(dev, depthBuf);

			Viewport	vp	=new Viewport(0, 0, width, height, 0.1f, 2000f);

			dctx.Rasterizer.SetViewport(vp);
			dctx.OutputMerger.SetTargets(dsView, rtView);

			gcam	=new UtilityLib.GameCamera(width, height, 16f/9f, 0.1f, 2000f);

			mbResized	=false;
		}

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			RenderForm	renderForm	=new RenderForm("Goblins");

			renderForm.UserResized	+=OnRenderFormResize;

			//figure out the client size stuff
			int	curWidth	=renderForm.ClientRectangle.Width;
			int	curHeight	=renderForm.ClientRectangle.Height;

			int	adjustX	=curWidth - renderForm.Size.Width;
			int	adjustY	=curHeight - renderForm.Size.Height;

			System.Drawing.Size	rfSize	=new System.Drawing.Size(ResX + adjustX, ResY + adjustY);

//			renderForm.Size	=rfSize;

			Device					device;
			SwapChain				swapChain;
			SwapChainDescription	scDesc	=new SwapChainDescription();

			scDesc.BufferCount			=1;
			scDesc.Flags				=SwapChainFlags.None;
			scDesc.IsWindowed			=true;
//			scDesc.ModeDescription		=new ModeDescription(ResX, ResY, new Rational(60, 1), Format.R8G8B8A8_UNorm);
			scDesc.ModeDescription		=new ModeDescription(renderForm.ClientSize.Width, renderForm.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
			scDesc.OutputHandle			=renderForm.Handle;
			scDesc.SampleDescription	=new SampleDescription(1, 0);
			scDesc.SwapEffect			=SwapEffect.Discard;
			scDesc.Usage				=Usage.RenderTargetOutput;

			

			SharpDX.DXGI.Factory	fact	=new Factory();

			Adapter	adpt	=fact.GetAdapter(0);

			FeatureLevel	[]features	=new FeatureLevel[1];

			features[0]	=new FeatureLevel();

//			features[0]	=FeatureLevel.Level_9_3;
			features[0]	=FeatureLevel.Level_11_0;

			Device.CreateWithSwapChain(adpt, DeviceCreationFlags.Debug, features,
				scDesc, out device, out swapChain);

			DeviceContext	dc	=device.ImmediateContext;

			MaterialLib.MaterialLib	matLib	=new MaterialLib.MaterialLib(device, MaterialLib.MaterialLib.ShaderModel.SM5);

			Random	rand	=new Random();

			Vector4	col0	=Vector4.UnitY * 0.8f;
			Vector4	col1	=Vector4.One * 0.3f;
			Vector4	col2	=Vector4.One * 0.4f;

			col0.W	=col1.W	=col2.W	=1f;

			matLib.CreateMaterial("TestMat");

			matLib.SetMaterialEffect("TestMat", "Static.fx");
			matLib.SetMaterialTechnique("TestMat", "TriSolidSpec");

			matLib.SetMaterialParameter("TestMat", "mSolidColour", col0);
			matLib.SetMaterialParameter("TestMat", "mSpecColor", Vector4.One);
			matLib.SetMaterialParameter("TestMat", "mLightColor0", Vector4.One);
			matLib.SetMaterialParameter("TestMat", "mLightColor1", col1);
			matLib.SetMaterialParameter("TestMat", "mLightColor2", col2);
			matLib.SetMaterialParameter("TestMat", "mLightDirection", UtilityLib.Mathery.RandomDirection(rand));
			matLib.SetMaterialParameter("TestMat", "mSpecPower", 15f);

			//Layout from VertexShader input signature
			InputLayout	layout	=new InputLayout(device, matLib.GetMaterialSignature("TestMat", 0), new[]
			{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
				new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
				new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0)
			});
			
			//Create Constant Buffer
/*			Buffer	constBuffer	=new Buffer(device,
				(64 * 4) + (16 * 8),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None, 0);
*/			
			//Prepare All the stages
			dc.InputAssembler.InputLayout		=layout;
			dc.InputAssembler.PrimitiveTopology	=PrimitiveTopology.TriangleList;
//			dc.VertexShader.SetConstantBuffer(0, constBuffer);
//			dc.VertexShader.Set(vs);
//			dc.PixelShader.Set(ps);
			
			//Get the backbuffer from the swapchain
			Texture2D	backBuffer	=Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

			//Renderview on the backbuffer
			RenderTargetView	renderView	=new RenderTargetView(device, backBuffer);
			
			//Create the depth buffer
			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
//				Format				=Format.D32_Float_S8X24_UInt,
				Format				=Format.D16_UNorm,
				ArraySize			=1,
				MipLevels			=1,
//				Width				=ResX,
//				Height				=ResY,
				Width				=renderForm.ClientSize.Width,
				Height				=renderForm.ClientSize.Height,
				SampleDescription	=new SampleDescription(1, 0),
				Usage				=ResourceUsage.Default,
				BindFlags			=BindFlags.DepthStencil,
				CpuAccessFlags		=CpuAccessFlags.None,
				OptionFlags			=ResourceOptionFlags.None
			};

			Texture2D	depthBuffer	=new Texture2D(device, depthDesc);
			
			//Create the depth buffer view
			DepthStencilView	depthView	=new DepthStencilView(device, depthBuffer);
			
			//Setup targets and viewport for rendering
//			dc.Rasterizer.SetViewport(new Viewport(0, 0, ResX, ResY, 0.1f, 2000.0f));
//			dc.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.1f, 2000.0f));
			dc.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));
			dc.OutputMerger.SetTargets(depthView, renderView);

//			UtilityLib.GameCamera	gcam	=new UtilityLib.GameCamera(ResX, ResY, 16f/9f, 0.1f, 2000f);
			UtilityLib.GameCamera	gcam	=new UtilityLib.GameCamera(renderForm.ClientSize.Width, renderForm.ClientSize.Height, 16f/9f, 0.1f, 2000f);

			Input	inp	=new InputLib.Input();

			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveLeft, MyActions.MoveRight,
				MyActions.MoveForward, MyActions.MoveBack);

			pSteering.SetTurnEnums(MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.PitchUp, MyActions.PitchDown);
			
			inp.MapAction(MyActions.PitchUp, 16);
			inp.MapAction(MyActions.MoveForward, 17);
			inp.MapAction(MyActions.PitchDown, 18);
			inp.MapAction(MyActions.TurnLeft, 30);
			inp.MapAction(MyActions.MoveBack, 31);
			inp.MapAction(MyActions.TurnRight, 32);

			StartSmall		ss		=new StartSmall(device, matLib);
			MaterialForm	matForm	=new MaterialForm(matLib);

			ss.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
			matForm.eNukedMeshPart	+=(sender, args) => ss.NukeMeshPart(sender as MeshLib.Mesh);

			ss.Visible		=true;
			matForm.Visible	=true;

			Vector3	pos		=Vector3.Zero;
			Matrix	world	=Matrix.Identity;
			Matrix	view	=Matrix.Identity;
			Matrix	proj	=Matrix.Identity;

			RasterizerStateDescription	rsd	=new RasterizerStateDescription();
			rsd.CullMode					=CullMode.Back;
			rsd.DepthBias					=0;
			rsd.DepthBiasClamp				=0f;
			rsd.FillMode					=FillMode.Solid;
			rsd.IsAntialiasedLineEnabled	=false;
			rsd.IsDepthClipEnabled			=true;
			rsd.IsFrontCounterClockwise		=false;
			rsd.IsMultisampleEnabled		=false;
			rsd.IsScissorEnabled			=false;
			rsd.SlopeScaledDepthBias		=0f;

			RasterizerState	rs	=new RasterizerState(device, rsd);

			dc.Rasterizer.State	=rs;

			BlendStateDescription	bsd	=new BlendStateDescription();
			bsd.AlphaToCoverageEnable					=false;
			bsd.IndependentBlendEnable					=false;
			bsd.RenderTarget[0].IsBlendEnabled			=true;
			bsd.RenderTarget[0].SourceBlend				=BlendOption.SourceAlpha;
			bsd.RenderTarget[0].DestinationBlend		=BlendOption.InverseSourceAlpha;
			bsd.RenderTarget[0].BlendOperation			=BlendOperation.Add;
			bsd.RenderTarget[0].SourceAlphaBlend		=BlendOption.One;
			bsd.RenderTarget[0].DestinationAlphaBlend	=BlendOption.Zero;
			bsd.RenderTarget[0].AlphaBlendOperation		=BlendOperation.Add;
			bsd.RenderTarget[0].RenderTargetWriteMask	=ColorWriteMaskFlags.All;

			BlendState	bs	=new BlendState(device, bsd);

			DepthStencilOperationDescription	dsod	=new DepthStencilOperationDescription();
			dsod.Comparison			=Comparison.Less;
			dsod.DepthFailOperation	=StencilOperation.Keep;
			dsod.FailOperation		=StencilOperation.Keep;
			dsod.PassOperation		=StencilOperation.Replace;

			DepthStencilStateDescription	dssd	=new DepthStencilStateDescription();
			dssd.BackFace			=dsod;
			dssd.DepthComparison	=Comparison.Less;
			dssd.DepthWriteMask		=DepthWriteMask.All;
			dssd.FrontFace			=dsod;
			dssd.IsDepthEnabled		=true;
			dssd.IsStencilEnabled	=false;
			dssd.StencilReadMask	=0;
			dssd.StencilWriteMask	=0;

			DepthStencilState	dss	=new DepthStencilState(device, dssd);

			dc.OutputMerger.BlendState			=bs;
			dc.OutputMerger.DepthStencilState	=dss;

			matLib.SetMaterialParameter("TestMat", "mProjection", gcam.Projection);

			RenderLoop.Run(renderForm, () =>
			{
				if(mbResized)
				{
					HandleResize(renderForm.ClientSize.Width, renderForm.ClientSize.Height,
						device, dc, ref gcam, ref swapChain, scDesc,
						ref backBuffer, ref renderView, ref depthBuffer, ref depthView);
					matLib.SetMaterialParameter("TestMat", "mView", view);
				}

				List<Input.InputAction>	actions	=inp.GetAction();
				
				pos	=pSteering.Update(pos, gcam.Forward, gcam.Left, gcam.Up, actions);
				
				gcam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				view	=gcam.View;
				proj	=gcam.Projection;

				matLib.SetMaterialParameter("TestMat", "mView", gcam.View);
				matLib.SetMaterialParameter("TestMat", "mEyePos", gcam.Position);

				//Clear views
				dc.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1f, 0);
				dc.ClearRenderTargetView(renderView, Color.CornflowerBlue);

				dc.InputAssembler.InputLayout		=layout;

				ss.Render(dc);

//				fxWorld.SetMatrix(world);
				
				// Present!
				swapChain.Present(0, PresentFlags.None);
			});

			
			//Release all resources
			layout.Dispose();
			renderView.Dispose();
			backBuffer.Dispose();
			device.Dispose();
			swapChain.Dispose();
		}
	}
}
