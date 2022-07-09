using System.Numerics;
using System.Diagnostics;
using UtilityLib;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using InputLib;
using MaterialLib;
using MeshLib;

//renderform and renderloop
using SharpDX.Windows;

using MatLib	=MaterialLib.MaterialLib;
using Color		=Vortice.Mathematics.Color;


namespace ColladaConvert;

internal static class Program
{
	enum MyActions
	{
		MoveForwardBack, MoveForward, MoveBack,
		MoveLeftRight, MoveLeft, MoveRight,
		MoveForwardFast, MoveBackFast,
		MoveLeftFast, MoveRightFast,
		Turn, TurnLeft, TurnRight,
		Pitch, PitchUp, PitchDown,
		LightX, LightY, LightZ,
		ToggleMouseLookOn, ToggleMouseLookOff,
		SensitivityUp, SensitivityDown,
		BoneRadiusUp, BoneLengthUp, 
		BoneRadiusDown, BoneLengthDown, BoneDone,
		Exit
	};

	const float	MaxTimeDelta	=0.1f;
	const float	MoveScalar		=50f;


	[STAThread]
	static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		Icon	testIcon	=new Icon("1281737606553.ico");

		//turn this on for help with leaky stuff
		//Configuration.EnableObjectTracking	=true;

		GraphicsDevice	gd	=new GraphicsDevice("Collada Conversion Tool",
			testIcon, FeatureLevel.Level_11_0, 0.1f, 3000f);

		//save renderform position
		gd.RendForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Properties.Settings.Default,
				"MainWindowPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		gd.RendForm.Location	=Properties.Settings.Default.MainWindowPos;

		SharedForms.ShaderCompileHelper.mTitle	="Compiling Shaders...";

		StuffKeeper	sk		=new StuffKeeper();

		sk.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
		sk.eCompileDone		+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

		sk.Init(gd, ".");

		MatLib		matLib	=new MatLib(gd, sk);

//		matLib.InitCelShading(1);
//		matLib.GenerateCelTexturePreset(gd.GD,
//			gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
//		matLib.SetCelTexture(0);

		PlayerSteering	pSteering		=SetUpSteering();
		Input			inp				=SetUpInput(gd.RendForm);
		Random			rand			=new Random();
		CommonPrims		comPrims		=new CommonPrims(gd, sk);
		bool			bMouseLookOn	=false;
		CBKeeper		cbk				=sk.GetCBKeeper();
		UserSettings	sets			=new UserSettings();

		//turn on sprint
		pSteering.SprintEnabled	=true;

		//set up post processing module
		PostProcess	post	=new PostProcess(gd, sk);
		
		EventHandler	actHandler	=new EventHandler(
			delegate(object ?s, EventArgs ea)
			{	inp.ClearInputs();	});

		EventHandler<EventArgs>	deActHandler	=new EventHandler<EventArgs>(
			delegate(object ?s, EventArgs ea)
			{
				gd.SetCapture(false);
				bMouseLookOn	=false;
			});

		EventHandler	lostHandler	=new EventHandler(
			delegate(object ?s, EventArgs ea)
			{
				post.FreeAll(gd);
				post	=new PostProcess(gd, sk);
			});

		gd.eDeviceLost				+=lostHandler;
		gd.RendForm.Activated		+=actHandler;
		gd.RendForm.AppDeactivated	+=deActHandler;

		int	resx	=gd.RendForm.ClientRectangle.Width;
		int	resy	=gd.RendForm.ClientRectangle.Height;

		AnimForm	ss	=SetUpForms(gd.GD, matLib, sk, comPrims);

		Vector3	pos			=Vector3.One * 5f;
		Vector3	lightDir	=-Vector3.UnitY;

		UpdateTimer	time	=new UpdateTimer(true, false);

		time.SetFixedTimeStepSeconds(1f / 60f);	//60fps update rate
		time.SetMaxDeltaSeconds(MaxTimeDelta);

		cbk.SetProjection(gd.GCam.ProjectionTransposed);
		cbk.UpdateChangeLess(gd.DC);

		List<Input.InputAction>	acts	=new List<Input.InputAction>();

		RenderLoop.Run(gd.RendForm, () =>
		{
			if(!gd.RendForm.Focused)
			{
				Thread.Sleep(33);
			}

			gd.CheckResize();

			if(bMouseLookOn && gd.RendForm.Focused)
			{
				gd.ResetCursorPos();
			}

			time.Stamp();
			while(time.GetUpdateDeltaSeconds() > 0f)
			{
				acts	=UpdateInput(inp, sets, gd,
					time.GetUpdateDeltaSeconds(), ref bMouseLookOn);
				if(!gd.RendForm.Focused)
				{
					acts.Clear();
					bMouseLookOn	=false;
					gd.SetCapture(false);
					inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
					inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
				}

				Vector3	deltaMove	=pSteering.Update(pos,
					gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, acts);

				deltaMove	*=MoveScalar;
				pos			+=deltaMove;
				
				ChangeLight(acts, ref lightDir);

				time.UpdateDone();
			}

			cbk.SetCommonCBToShaders(gd.DC);

			matLib.SetLightDirection(lightDir);

			gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

			cbk.SetView(gd.GCam.ViewTransposed, gd.GCam.Position);
			cbk.UpdateFrame(gd.DC);

//			matLib.UpdateWVP(Matrix.Identity, gd.GCam.View, gd.GCam.Projection, gd.GCam.Position);

			comPrims.Update(gd.GCam, lightDir);

			ss.RenderUpdate(time.GetRenderUpdateDeltaSeconds());

			post.SetTargets(gd, "BackColor", "BackDepth");

			post.ClearTarget(gd, "BackColor",
				Misc.SystemColorToDXColor(System.Drawing.Color.CornflowerBlue));
			post.ClearDepth(gd, "BackDepth");

			ss.Render(gd.DC);

			if(ss.GetDrawAxis())
			{
				comPrims.DrawAxis(gd.DC);
			}

			if(ss.GetDrawBox())
			{
				comPrims.DrawBox(gd.DC, Matrix4x4.Identity);
			}

			if(ss.GetDrawSphere())
			{
				comPrims.DrawSphere(gd.DC, Matrix4x4.Identity);
			}

			gd.Present();

			acts.Clear();
		}, true);   //true here is slow but needed for winforms events

		sets.SaveSettings();
		Properties.Settings.Default.Save();

		gd.RendForm.Activated		-=actHandler;
		gd.RendForm.AppDeactivated	-=deActHandler;

		comPrims.FreeAll();
		inp.FreeAll(gd.RendForm);
		post.FreeAll(gd);
		matLib.FreeAll();

		sk.eCompileDone		-=SharedForms.ShaderCompileHelper.CompileDoneHandler;
		sk.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;

		sk.FreeAll();
		
		//Release all resources
		gd.ReleaseAll();
	}

	static AnimForm SetUpForms(ID3D11Device gd, MatLib matLib, StuffKeeper sk, CommonPrims ep)
	{
		MeshLib.AnimLib	animLib	=new MeshLib.AnimLib();
		AnimForm		af		=new AnimForm(gd, matLib, animLib, sk);
		StripElements	se		=new StripElements();
		SkeletonEditor	skel	=new SkeletonEditor();			

		SharedForms.MaterialForm	matForm	=new SharedForms.MaterialForm(matLib, sk);
		SharedForms.CelTweakForm	celForm	=new SharedForms.CelTweakForm(gd, matLib);
		SharedForms.Output			outForm	=new SharedForms.Output();

		//save positions
		matForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "MaterialFormPos", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		matForm.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "MaterialFormSize", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		af.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "AnimFormPos", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		skel.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "SkeletonEditorFormPos", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		skel.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "SkeletonEditorFormSize", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		celForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "CelTweakFormPos", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		outForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "OutputFormPos", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		outForm.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "OutputFormSize", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		SeamEditor	seam	=null;
		MakeSeamForm(ref seam);

		af.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
		af.ePrint				+=(sender, args) => outForm.Print(sender as string);
		skel.ePrint				+=(sender, args) => outForm.Print(sender as string);
		matForm.eNukedMeshPart	+=(sender, args) => af.NukeMeshPart(sender as List<int>);
		matForm.eStripElements	+=(sender, args) =>
			{	if(se.Visible){	return;	}
				se.Populate(args as ArchEventArgs);	};
		matForm.eGenTangents	+=(sender, args) =>
			{	ArchEventArgs	aea	=args as ArchEventArgs;
				if(aea != null)
				{
					aea.mArch.GenTangents(gd, aea.mIndexes, matForm.GetTexCoordSet());
				}	};
		matForm.eFoundSeams		+=(sender, args) =>
			{	if(seam.IsDisposed)
				{
					MakeSeamForm(ref seam);
				}
				seam.Initialize(gd);
				seam.AddSeams(sender as List<EditorMesh.WeightSeam>);
				seam.SizeColumns();
				seam.Visible	=true;	};
		se.eDeleteElement		+=(sender, args) =>
			{	List<int>	elements	=sender as List<int>;
				af.NukeVertexElement(se.GetIndexes(), elements);
				se.Populate(null);	se.Visible	=false;
				matForm.RefreshMeshPartList();	};
		se.eEscape				+=(sender, args) =>
			{	se.Populate(null);	se.Visible	=false;	};
		af.eSkeletonChanged		+=(sender, args) => skel.Initialize(sender as MeshLib.Skeleton);
		af.eBoundsChanged		+=(sender, args) => ep.ReBuildBoundsDrawData(gd, sender);			

		skel.eSelectUnUsedBones	+=(sender, args) => af.GetBoneNamesInUseByDraw(sender as List<string>);
		skel.eBonesChanged		+=(sender, args) => af.BonesChanged();
		skel.eAdjustBone		+=(sender, args) => af.AdjustBone(sender as string);

		af.Visible		=true;
		matForm.Visible	=true;
		skel.Visible	=true;
		celForm.Visible	=true;
		outForm.Visible	=true;

		return	af;
	}

	static Input SetUpInput(RenderForm hwnd)
	{
		Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency, hwnd);
		
		inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.W);
		inp.MapAction(MyActions.MoveLeft, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.A);
		inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.S);
		inp.MapAction(MyActions.MoveRight, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.D);
		inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.W);
		inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.S);
		inp.MapAction(MyActions.MoveLeftFast, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.A);
		inp.MapAction(MyActions.MoveRightFast, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.D);

		//arrow keys
		inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Up);
		inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Down);
		inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.Up);
		inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.Down);
		inp.MapAction(MyActions.TurnLeft, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Left);
		inp.MapAction(MyActions.TurnRight, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Right);
		inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Q);
		inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.E);

		inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold, Modifiers.None, 16);
		inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold, Modifiers.None, 18);
		inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, 36);
		inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, 37);
		inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, 38);

		inp.MapToggleAction(MyActions.ToggleMouseLookOn,
			MyActions.ToggleMouseLookOff, Modifiers.None,
			Input.VariousButtons.RightMouseButton);

		inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
		inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
		inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
		inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

		inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadLeft);
		inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadDown);
		inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadRight);

		//sensitivity adjust
		inp.MapAction(MyActions.SensitivityDown, ActionTypes.PressAndRelease,
			Modifiers.None, System.Windows.Forms.Keys.OemMinus);
		//for numpad
		inp.MapAction(MyActions.SensitivityUp, ActionTypes.PressAndRelease,
			Modifiers.None, System.Windows.Forms.Keys.Oemplus);
		//non numpad will have shift held too
		inp.MapAction(MyActions.SensitivityUp, ActionTypes.PressAndRelease,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.Oemplus);

		inp.MapAction(MyActions.BoneLengthUp, ActionTypes.ActivateOnce, Modifiers.None, Keys.T);
		inp.MapAction(MyActions.BoneRadiusUp, ActionTypes.ActivateOnce, Modifiers.None, Keys.R);
		inp.MapAction(MyActions.BoneLengthDown, ActionTypes.ActivateOnce, Modifiers.ShiftHeld, Keys.T);
		inp.MapAction(MyActions.BoneRadiusDown, ActionTypes.ActivateOnce, Modifiers.ShiftHeld, Keys.R);
		inp.MapAction(MyActions.BoneDone, ActionTypes.ActivateOnce, Modifiers.None, Keys.X);

		inp.MapAction(MyActions.Exit, ActionTypes.ActivateOnce, Modifiers.None, Keys.Escape);

		return	inp;
	}

	static PlayerSteering SetUpSteering()
	{
		PlayerSteering	pSteering	=new PlayerSteering();
		pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

		pSteering.SetMoveEnums(MyActions.MoveForwardBack, MyActions.MoveLeftRight,
			MyActions.MoveForward, MyActions.MoveBack, MyActions.MoveLeft,
			MyActions.MoveRight, MyActions.MoveForwardFast, MyActions.MoveBackFast,
			MyActions.MoveLeftFast, MyActions.MoveRightFast);

		pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

		pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);

		return	pSteering;
	}

	static void MakeSeamForm(ref SeamEditor seam)
	{
		seam	=new SeamEditor();

		seam.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "SeamEditorFormPos", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		seam.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "SeamEditorFormSize", true,
			System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
	}

	static List<Input.InputAction> UpdateInput(Input inp, UserSettings sets,
		GraphicsDevice gd, float delta, ref bool bMouseLookOn)
	{
		List<Input.InputAction>	actions	=inp.GetAction();

		//check for exit
		foreach(Input.InputAction act in actions)
		{
			if(act.mAction.Equals(MyActions.Exit))
			{
				gd.RendForm.Close();
				return	actions;
			}
		}

		foreach(Input.InputAction act in actions)
		{
			if(act.mAction.Equals(MyActions.ToggleMouseLookOn))
			{
				bMouseLookOn	=true;
				gd.SetCapture(true);
				inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
				inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
			}
			else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
			{
				bMouseLookOn	=false;
				gd.SetCapture(false);
				inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
				inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
			}
		}

		//delta scale analogs, since there's no timestamp stuff in gamepad code
		foreach(Input.InputAction act in actions)
		{
			if(!act.mbTime && act.mDevice == Input.InputAction.DeviceType.ANALOG)
			{
				//analog needs a time scale applied
				act.mMultiplier	*=delta;
			}
		}

		//scale inputs to user prefs
		foreach(Input.InputAction act in actions)
		{
			if(act.mAction.Equals(MyActions.Turn)
				|| act.mAction.Equals(MyActions.TurnLeft)
				|| act.mAction.Equals(MyActions.TurnRight)
				|| act.mAction.Equals(MyActions.Pitch)
				|| act.mAction.Equals(MyActions.PitchDown)
				|| act.mAction.Equals(MyActions.PitchUp))
			{
				if(act.mDevice == Input.InputAction.DeviceType.MOUSE)
				{
					act.mMultiplier	*=UserSettings.MouseTurnMultiplier
						* sets.mTurnSensitivity;
				}
				else if(act.mDevice == Input.InputAction.DeviceType.ANALOG)
				{
					act.mMultiplier	*=UserSettings.AnalogTurnMultiplier;
				}
				else if(act.mDevice == Input.InputAction.DeviceType.KEYS)
				{
					act.mMultiplier	*=UserSettings.KeyTurnMultiplier;
				}
			}
		}

		//sensitivity adjust
		foreach(Input.InputAction act in actions)
		{
			float	sense	=sets.mTurnSensitivity;
			if(act.mAction.Equals(MyActions.SensitivityUp))
			{
				sense	+=0.025f;
			}
			else if(act.mAction.Equals(MyActions.SensitivityDown))
			{
				sense	-=0.025f;
			}
			else
			{
				continue;
			}
			sets.mTurnSensitivity	=Math.Clamp(sense, 0.025f, 10f);
		}

		return	actions;
	}


	static void AdjustBone(List<Input.InputAction> acts)
	{
		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(MyActions.BoneLengthUp))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneRadiusUp))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneLengthDown))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneRadiusDown))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneDone))
			{
			}
		}
	}


	static void ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
	{
		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(MyActions.LightX))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationX(act.mMultiplier);
				lightDir		=Mathery.TransformCoordinate(lightDir, ref rot);
				lightDir		=Vector3.Normalize(lightDir);
			}
			else if(act.mAction.Equals(MyActions.LightY))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationY(act.mMultiplier);
				lightDir		=Mathery.TransformCoordinate(lightDir, ref rot);
				lightDir		=Vector3.Normalize(lightDir);
			}
			else if(act.mAction.Equals(MyActions.LightZ))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationZ(act.mMultiplier);
				lightDir		=Mathery.TransformCoordinate(lightDir, ref rot);
				lightDir		=Vector3.Normalize(lightDir);
			}
		}
	}

	static void DeleteVertElement(ID3D11Device gd, List<int> inds, List<Mesh> meshes)
	{
		Type	firstType	=meshes[0].VertexType;

		foreach(Mesh m in meshes)
		{
			if(m.VertexType == firstType)
			{
				EditorMesh	em	=m as EditorMesh;

				em.NukeVertexElement(inds, gd);
			}
		}
	}
}