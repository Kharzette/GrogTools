using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using MatLib = MaterialLib.MaterialLib;


namespace ColladaStartSmall
{
	internal class GraphicsDevice
	{
		Device			mGD;
		DeviceContext	mDC;

		SwapChain	mSChain;

		Texture2D			mBackBuffer, mDepthBuffer;
		RenderTargetView	mBBView;
		DepthStencilView	mDSView;

		internal Device GD
		{
			get { return mGD; }
		}

		internal DeviceContext DC
		{
			get { return mDC; }
		}


		internal GraphicsDevice(RenderForm renderForm, FeatureLevel flevel)
		{
			SwapChainDescription	scDesc	=new SwapChainDescription();

			scDesc.BufferCount			=1;
			scDesc.Flags				=SwapChainFlags.None;
			scDesc.IsWindowed			=true;
			scDesc.ModeDescription		=new ModeDescription(renderForm.ClientSize.Width, renderForm.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
			scDesc.OutputHandle			=renderForm.Handle;
			scDesc.SampleDescription	=new SampleDescription(1, 0);
			scDesc.SwapEffect			=SwapEffect.Discard;
			scDesc.Usage				=Usage.RenderTargetOutput;
			
			SharpDX.DXGI.Factory	fact	=new Factory();

			Adapter	adpt	=fact.GetAdapter(0);

			FeatureLevel	[]features	=new FeatureLevel[1];

			features[0]	=new FeatureLevel();

			features[0]	=flevel;

			Device.CreateWithSwapChain(adpt, DeviceCreationFlags.Debug, features,
				scDesc, out mGD, out mSChain);

			mDC	=mGD.ImmediateContext;

			//I always use this, hope it doesn't change somehow
			mDC.InputAssembler.PrimitiveTopology	=PrimitiveTopology.TriangleList;

			//Get the backbuffer from the swapchain
			mBackBuffer	=Texture2D.FromSwapChain<Texture2D>(mSChain, 0);

			//Renderview on the backbuffer
			mBBView	=new RenderTargetView(mGD, mBackBuffer);
			
			//Create the depth buffer
			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
				//pick depth format based on feature level
				Format				=(mGD.FeatureLevel != FeatureLevel.Level_9_3)?
										Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt,
				ArraySize			=1,
				MipLevels			=1,
				Width				=renderForm.ClientSize.Width,
				Height				=renderForm.ClientSize.Height,
				SampleDescription	=new SampleDescription(1, 0),
				Usage				=ResourceUsage.Default,
				BindFlags			=BindFlags.DepthStencil,
				CpuAccessFlags		=CpuAccessFlags.None,
				OptionFlags			=ResourceOptionFlags.None
			};

			mDepthBuffer	=new Texture2D(mGD, depthDesc);
			
			//Create the depth buffer view
			mDSView	=new DepthStencilView(mGD, mDepthBuffer);
			
			//Setup targets and viewport for rendering
			mDC.Rasterizer.SetViewport(new Viewport(0, 0,
				renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));

			mDC.OutputMerger.SetTargets(mDSView, mBBView);
		}


		internal void HandleResize(int width, int height,
			ref UtilityLib.GameCamera gcam)
		{
			Utilities.Dispose(ref mBackBuffer);
			Utilities.Dispose(ref mBBView);
			Utilities.Dispose(ref mDepthBuffer);
			Utilities.Dispose(ref mDSView);

			mSChain.ResizeBuffers(1, width, height, Format.Unknown, SwapChainFlags.None);

			mBackBuffer	=Texture2D.FromSwapChain<Texture2D>(mSChain, 0);
			mBBView		=new RenderTargetView(mGD, mBackBuffer);

			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
				//pick depth format based on feature level
				Format				=(mGD.FeatureLevel != FeatureLevel.Level_9_3)?
										Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt,
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

			mDepthBuffer	=new Texture2D(mGD, depthDesc);
			mDSView			=new DepthStencilView(mGD, mDepthBuffer);

			Viewport	vp	=new Viewport(0, 0, width, height, 0f, 1f);

			mDC.Rasterizer.SetViewport(vp);
			mDC.OutputMerger.SetTargets(mDSView, mBBView);

			gcam	=new UtilityLib.GameCamera(width, height, 16f/9f, 0.1f, 2000f);
		}


		internal void Present()
		{
			mSChain.Present(0, PresentFlags.None);
		}


		internal void ClearViews()
		{
			mDC.ClearDepthStencilView(mDSView, DepthStencilClearFlags.Depth, 1f, 0);
			mDC.ClearRenderTargetView(mBBView, Color.CornflowerBlue);
		}


		internal void ReleaseAll()
		{
			mBBView.Dispose();
			mBackBuffer.Dispose();
			mDSView.Dispose();
			mDepthBuffer.Dispose();
			mDC.Dispose();
			mSChain.Dispose();
			mGD.Dispose();
		}
	}
}
