using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InputLib;
using MaterialLib;
using UtilityLib;

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
	internal static class Program
	{
		static bool	mbResized;

		const int	ResX		=1280;
		const int	ResY		=720;
		const float	AxisSize	=50f;

		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			LightX, LightY, LightZ,
			ToggleMouseLook
		};

		static void OnRenderFormResize(object sender, EventArgs ea)
		{
			mbResized	=true;
		}

		static void ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
		{
			foreach(Input.InputAction act in acts)
			{
				if(act.mAction.Equals(MyActions.LightX))
				{
					Matrix	rot	=Matrix.RotationX(act.mMultiplier * 0.001f);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightY))
				{
					Matrix	rot	=Matrix.RotationY(act.mMultiplier * 0.001f);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightZ))
				{
					Matrix	rot	=Matrix.RotationZ(act.mMultiplier * 0.001f);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
			}
		}

		static void DeleteVertElement(Device gd, List<int> inds, List<MeshLib.Mesh> meshes)
		{
			Type	firstType	=meshes[0].VertexType;

			foreach(MeshLib.Mesh m in meshes)
			{
				if(m.VertexType == firstType)
				{
					MeshLib.EditorMesh	em	=m as MeshLib.EditorMesh;

					em.NukeVertexElement(inds, gd);
				}
			}
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

			RenderForm	renderForm	=new RenderForm("Collada Conversion Tool");

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

			features[0]	=FeatureLevel.Level_11_0;
//			features[0]	=FeatureLevel.Level_10_1;
//			features[0]	=FeatureLevel.Level_10_0;
//			features[0]	=FeatureLevel.Level_9_3;

			Device.CreateWithSwapChain(adpt, DeviceCreationFlags.Debug, features,
				scDesc, out device, out swapChain);

			DeviceContext	dc	=device.ImmediateContext;

			MatLib.ShaderModel	shaderModel;

			switch(features[0])
			{
				case	FeatureLevel.Level_11_0:
					shaderModel	=MatLib.ShaderModel.SM5;
					break;
				case	FeatureLevel.Level_10_1:
					shaderModel	=MatLib.ShaderModel.SM41;
					break;
				case	FeatureLevel.Level_10_0:
					shaderModel	=MatLib.ShaderModel.SM4;
					break;
				case	FeatureLevel.Level_9_3:
					shaderModel	=MatLib.ShaderModel.SM2;
					break;
				default:
					Debug.Assert(false);	//only support the above
					shaderModel	=MatLib.ShaderModel.SM2;
					break;
			}

			MatLib	matLib		=new MatLib(device,	shaderModel, true);
			MatLib	axisMatLib	=new MatLib(device,	shaderModel, true);

			matLib.InitCelShading(1);
			matLib.GenerateCelTexturePreset(device,
				features[0] == FeatureLevel.Level_9_3, false, 0);

			Random	rand	=new Random();

			//I always use this, hope it doesn't change somehow
			dc.InputAssembler.PrimitiveTopology	=PrimitiveTopology.TriangleList;
			
			//Get the backbuffer from the swapchain
			Texture2D	backBuffer	=Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

			//Renderview on the backbuffer
			RenderTargetView	renderView	=new RenderTargetView(device, backBuffer);
			
			//Create the depth buffer
			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
				//pick depth format based on feature level
				Format				=(shaderModel != MatLib.ShaderModel.SM2)?
										Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt,
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
			dc.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));
			dc.OutputMerger.SetTargets(depthView, renderView);

			UtilityLib.GameCamera	gcam	=new UtilityLib.GameCamera(renderForm.ClientSize.Width, renderForm.ClientSize.Height, 16f/9f, 0.1f, 2000f);

			Input	inp	=new InputLib.Input();

			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveLeftRight, MyActions.MoveLeft, MyActions.MoveRight,
				MyActions.MoveForwardBack, MyActions.MoveForward, MyActions.MoveBack);

			pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);
			
			inp.MapAction(MyActions.PitchUp, 16);
			inp.MapAction(MyActions.MoveForward, 17);
			inp.MapAction(MyActions.PitchDown, 18);
			inp.MapAction(MyActions.MoveLeft, 30);
			inp.MapAction(MyActions.MoveBack, 31);
			inp.MapAction(MyActions.MoveRight, 32);
			inp.MapAction(MyActions.LightX, 36);
			inp.MapAction(MyActions.LightY, 37);
			inp.MapAction(MyActions.LightZ, 38);

			inp.MapToggleAction(MyActions.ToggleMouseLook, Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
			inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

			inp.MapAction(MyActions.LightX, Input.VariousButtons.GamePadDPadLeft);
			inp.MapAction(MyActions.LightY, Input.VariousButtons.GamePadDPadDown);
			inp.MapAction(MyActions.LightZ, Input.VariousButtons.GamePadDPadRight);

			MeshLib.AnimLib	animLib	=new MeshLib.AnimLib();

			StartSmall		ss		=new StartSmall(device, matLib, animLib);
			MaterialForm	matForm	=new MaterialForm(matLib);
			StripElements	se		=new StripElements();

			//save positions
			matForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			ss.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "AnimFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			renderForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			renderForm.Location	=Settings.Default.MainWindowPos;

			ss.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
			matForm.eNukedMeshPart	+=(sender, args) => ss.NukeMeshPart(sender as MeshLib.Mesh);
			matForm.eStripElements	+=(sender, args) =>
				{	if(se.Visible){	return;	}
					se.Populate(sender as List<MeshLib.Mesh>);	};
			se.eDeleteElement		+=(sender, args) =>
				{	DeleteVertElement(device, sender as List<int>, se.GetMeshes());
					se.Populate(null);	se.Visible	=false;
					matForm.RefreshMeshPartList();	};
			se.eEscape				+=(sender, args) =>
				{	se.Populate(null);	se.Visible	=false;	};

			ss.Visible		=true;
			matForm.Visible	=true;

			Vector3	pos		=Vector3.Zero;

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

			Vector3	lightDir		=-Vector3.UnitY;
			bool	bMouseLookOn	=false;
			long	lastTime		=Stopwatch.GetTimestamp();

			//keep track of mouse pos during mouse look
			System.Drawing.Point		StoredMousePos	=System.Drawing.Point.Empty;
			System.Drawing.Rectangle	StoredClipRect	=Cursor.Clip;

			//axis boxes
			BoundingBox	xBox	=Misc.MakeBox(AxisSize, 1f, 1f);
			BoundingBox	yBox	=Misc.MakeBox(1f, AxisSize, 1f);
			BoundingBox	zBox	=Misc.MakeBox(1f, 1f, AxisSize);

			PrimObject	mXAxis	=PrimFactory.CreateCube(device, xBox);
			PrimObject	mYAxis	=PrimFactory.CreateCube(device, yBox);
			PrimObject	mZAxis	=PrimFactory.CreateCube(device, zBox);

			Vector4	redColor	=Vector4.One;
			Vector4	greenColor	=Vector4.One;
			Vector4	blueColor	=Vector4.One;
			Vector4	lightColor2	=Vector4.One * 0.8f;
			Vector4	lightColor3	=Vector4.One * 0.6f;

			lightColor2.W	=lightColor3.W	=1f;

			redColor.Y	=redColor.Z	=greenColor.X	=greenColor.Z	=blueColor.X	=blueColor.Y	=0f;

			axisMatLib.CreateMaterial("RedAxis");
			axisMatLib.SetMaterialEffect("RedAxis", "Static.fx");
			axisMatLib.SetMaterialTechnique("RedAxis", "TriSolidSpec");
			axisMatLib.SetMaterialParameter("RedAxis", "mLightColor0", Vector4.One);
			axisMatLib.SetMaterialParameter("RedAxis", "mLightColor1", lightColor2);
			axisMatLib.SetMaterialParameter("RedAxis", "mLightColor2", lightColor3);
			axisMatLib.SetMaterialParameter("RedAxis", "mSolidColour", redColor);
			axisMatLib.SetMaterialParameter("RedAxis", "mSpecPower", 1);
			axisMatLib.SetMaterialParameter("RedAxis", "mSpecColor", Vector4.One);

			axisMatLib.CloneMaterial("RedAxis", "GreenAxis");
			axisMatLib.CloneMaterial("RedAxis", "BlueAxis");

			axisMatLib.SetMaterialParameter("GreenAxis", "mSolidColour", blueColor);
			axisMatLib.SetMaterialParameter("BlueAxis", "mSolidColour", greenColor);

			axisMatLib.SetParameterForAll("mWorld", Matrix.Identity);

			RenderLoop.Run(renderForm, () =>
			{
				if(mbResized)
				{
					HandleResize(renderForm.ClientSize.Width, renderForm.ClientSize.Height,
						device, dc, ref gcam, ref swapChain, scDesc,
						ref backBuffer, ref renderView, ref depthBuffer, ref depthView);
				}

				if(bMouseLookOn)
				{
					Cursor.Position	=StoredMousePos;
				}

				List<Input.InputAction>	actions	=inp.GetAction();
				if(!renderForm.Focused)
				{
					actions.Clear();
				}
				else
				{
					foreach(Input.InputAction act in actions)
					{
						if(act.mAction.Equals(MyActions.ToggleMouseLook))
						{
							bMouseLookOn	=!bMouseLookOn;
							Debug.WriteLine("Mouse look: " + bMouseLookOn);

							renderForm.Capture	=bMouseLookOn;

							//find a way to hide cursor
							if(bMouseLookOn)
							{
								inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
								inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
								Cursor.Hide();

								StoredMousePos	=Cursor.Position;

								Cursor.Clip	=renderForm.RectangleToScreen(renderForm.ClientRectangle);
							}
							else
							{
								inp.UnMapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
								inp.UnMapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
								Cursor.Show();
								Cursor.Clip	=StoredClipRect;
							}
						}
					}
				}

				ChangeLight(actions, ref lightDir);

				//light direction is backwards now for some strange reason
				matLib.SetParameterForAll("mLightDirection", -lightDir);
				axisMatLib.SetParameterForAll("mLightDirection", -lightDir);
				
				pos	=pSteering.Update(pos, gcam.Forward, gcam.Left, gcam.Up, actions);
				
				gcam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				matLib.SetParameterForAll("mView", gcam.View);
				matLib.SetParameterForAll("mEyePos", gcam.Position);
				matLib.SetParameterForAll("mProjection", gcam.Projection);

				axisMatLib.SetParameterForAll("mView", gcam.View);
				axisMatLib.SetParameterForAll("mEyePos", gcam.Position);
				axisMatLib.SetParameterForAll("mProjection", gcam.Projection);

				matLib.SetCelTexture(0);

				//Clear views
				dc.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1f, 0);
				dc.ClearRenderTargetView(renderView, Color.CornflowerBlue);

				long	timeNow	=Stopwatch.GetTimestamp();
				long	delta	=timeNow - lastTime;

				ss.Render(dc, (float)delta / (float)Stopwatch.Frequency);
				
				if(true)//mbDrawAxis)
				{
					//X axis red
					axisMatLib.ApplyMaterialPass("RedAxis", dc, 0);
					mXAxis.Draw(dc);

					//Y axis green
					axisMatLib.ApplyMaterialPass("GreenAxis", dc, 0);
					mYAxis.Draw(dc);

					//Z axis blue
					axisMatLib.ApplyMaterialPass("BlueAxis", dc, 0);
					mZAxis.Draw(dc);
				}
				// Present!
				swapChain.Present(0, PresentFlags.None);

				lastTime	=timeNow;
			});

			Settings.Default.Save();
			
			//Release all resources
//			layout.Dispose();
			renderView.Dispose();
			backBuffer.Dispose();
			device.Dispose();
			swapChain.Dispose();
		}
	}
}
