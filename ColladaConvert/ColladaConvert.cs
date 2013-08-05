using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Kinect;
using UtilityLib;
using MeshLib;


namespace ColladaConvert
{
	public class ColladaConvert : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSLib;
		BasicEffect				mBFX;
		PrimObject				mBoundPrim;
		PrimObject				mXAxis, mYAxis, mZAxis;

		//ordered list of fonts
		IOrderedEnumerable<KeyValuePair<string, SpriteFont>>	mFonts;

		MaterialLib.MaterialLib	mMatLib;
		AnimLib					mAnimLib;
		Character				mCharacter;
		StaticMeshObject		mStaticMesh;
		bool					mbCharacterLoaded;	//so I know which mesh obj to use
		bool					mbDrawAxis	=true;
		bool					mbPaused	=true;
		double					mAnimTime;

		//kinect stuff
		KinectSensor		mSensor;
		SkeletonStream		mSkelStream;
		bool				mbCountingDown;
		int					mCountDown, mAnimNameCounter;
		bool				mbFirstTimeStamp;
		Int64				mTimeStampAdjust;

		//recorded data from the sensor
		CaptureData	mSkelFrames	=new CaptureData();
		
		//control
		PlayerSteering	mSteering;
		GameCamera		mGameCam;
		Input			mInput;

		//material gui
		SharedForms.MaterialForm	mMF;

		//animation gui
		AnimForm	mCF;
		string		mCurrentAnimName;
		float		mTimeScale;			//anim playback speed
		Int64		mCurAnimTime, mCurAnimStart;
		Vector3		mLightDir;
		Random		mRand	=new Random();

		//kinect gui
		KinectForm	mKF;

		//cellshade tweaker form
		SharedForms.CellTweakForm	mCTF;

		//vert elements gui
		StripElements	mSE	=new StripElements();

		//spinning light
		float	mLX, mLY, mLZ;

		public static event EventHandler	eAnimsUpdated;

		//constants
		const float	LightXRot	=0.003f;
		const float	LightYRot	=0.009f;
		const float	LightZRot	=0.006f;
		const float	AxisSize	=50f;


		public ColladaConvert()
		{
			mGDM	=new GraphicsDeviceManager(this);
			Content.RootDirectory	="GameContent";

			mCurrentAnimName	="";
			mTimeScale			=1.0f;

			//set window position
			if(!mGDM.IsFullScreen)
			{
				System.Windows.Forms.Control	mainWindow
					=System.Windows.Forms.Form.FromHandle(this.Window.Handle);

				//add data binding so it will save
				mainWindow.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					global::ColladaConvert.Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

				mainWindow.Location	=
					global::ColladaConvert.Settings.Default.MainWindowPos;

				IsMouseVisible	=true;
			}
		}


		protected override void Initialize()
		{
			mGameCam	=new GameCamera(mGDM.GraphicsDevice.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height,
				mGDM.GraphicsDevice.Viewport.AspectRatio,
				0.1f, 1000.0f);

			mInput	=new Input();

			if(KinectSensor.KinectSensors.Count > 0)
			{
				mSensor	=KinectSensor.KinectSensors[0];
				mSensor.SkeletonFrameReady	+=OnSkeletonFrameReady;
			}

			base.Initialize();
		}


		protected override void LoadContent()
		{
			GraphicsDevice	gd	=mGDM.GraphicsDevice;

			Dictionary<string, SpriteFont>	fonts	=UtilityLib.FileUtil.LoadAllFonts(Content);
			mFonts	=fonts.OrderBy(fnt => fnt.Value.LineSpacing);

			mSB			=new SpriteBatch(gd);
			mSLib		=new ContentManager(Services, "ShaderLib");
			mMatLib		=new MaterialLib.MaterialLib(gd, Content, mSLib, true);
			mAnimLib	=new AnimLib();
			mBFX		=new BasicEffect(gd);
			mCharacter	=new Character(mMatLib, mAnimLib);
			mStaticMesh	=new StaticMeshObject(mMatLib);

			//set up cell shading
			mMatLib.InitCellShading(1);

			//set to character settings
			mMatLib.GenerateCellTexturePreset(gd, true, 0);
			mMatLib.SetCellTexture(0);

			mStaticMesh.SetTransform(Matrix.Identity);
			mCharacter.SetTransform(Matrix.Identity);

			//axis boxes
			BoundingBox	xBox	=Misc.MakeBox(AxisSize, 1f, 1f);
			BoundingBox	yBox	=Misc.MakeBox(1f, AxisSize, 1f);
			BoundingBox	zBox	=Misc.MakeBox(1f, 1f, AxisSize);

			mXAxis	=PrimFactory.CreateCube(gd, xBox, null);
			mYAxis	=PrimFactory.CreateCube(gd, yBox, null);
			mZAxis	=PrimFactory.CreateCube(gd, zBox, null);

			InitializeEffect();

			mCTF			=new SharedForms.CellTweakForm(gd, mMatLib);
			mCTF.Visible	=true;

			mCF				=new AnimForm(mAnimLib);
			mCF.Visible		=true;

			mCF.eLoadAnim				+=OnOpenAnim;
			mCF.eLoadModel				+=OnOpenModel;
			mCF.eLoadStaticModel		+=OnOpenStaticModel;
			mCF.eAnimSelectionChanged	+=OnAnimSelChanged;
			mCF.eTimeScaleChanged		+=OnTimeScaleChanged;
			mCF.eSaveLibrary			+=OnSaveLibrary;
			mCF.eSaveCharacter			+=OnSaveCharacter;
			mCF.eLoadCharacter			+=OnLoadCharacter;
			mCF.eLoadLibrary			+=OnLoadLibrary;
			mCF.eLoadStatic				+=OnLoadStatic;
			mCF.eSaveStatic				+=OnSaveStatic;
			mCF.eBoundMesh				+=OnBoundMesh;
			mCF.eShowBound				+=OnShowBound;
			mCF.eShowAxis				+=OnShowAxis;
			mCF.ePause					+=OnPause;

			mMF	=new SharedForms.MaterialForm(gd, mMatLib, true);
			mMF.Visible	=true;

			if(mSensor != null)
			{
				mKF	=new KinectForm(mAnimLib);
				mKF.Visible	=true;
			}

			mKF.eToggleRecord	+=OnToggleRecord;
			mKF.eLoadRawData	+=OnLoadRawData;
			mKF.eSaveRawData	+=OnSaveRawData;
			mKF.eConvertToAnim	+=OnConvertToAnim;
			mKF.eTrimStart		+=OnTrimStart;
			mKF.eTrimEnd		+=OnTrimEnd;

			//bind matform window position
			mMF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			//bind animform window position
			mCF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "AnimFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			//bind kinectform window position
			mKF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "KinectFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			//bind cellTweakForm window position
			mCTF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "CellTweakFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mMF.eNukedMeshPart	+=OnNukedMeshPart;
			mMF.eStripElements	+=OnStripElements;

			mSE.eDeleteElement	+=OnDeleteVertElement;

			mSteering	=new PlayerSteering(gd.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height);
			mSteering.Method				=PlayerSteering.SteeringMethod.Fly;
			mSteering.Speed					=0.2f;
			mSteering.UseGamePadIfPossible	=false;
			mSteering.RightClickToTurn		=true;

			//for testing
//			mAnimLib.ReadFromFile("C:/Users/kharz_000/Documents/3dsmax/export/MixamoGirl.AnimLib", true);
//			mMatLib.ReadFromFile("C:/Users/kharz_000/Documents/3dsmax/export/MixamoGirl.MatLib", true, gd);
//			mMF.UpdateMaterials();
//			mCharacter.ReadFromFile("C:/Users/kharz_000/Documents/3dsmax/export/MixamoGirlBouncy.Character", gd, true);
//			mMF.UpdateMeshPartList(mCharacter.GetMeshPartList(), null);
//			eAnimsUpdated(mAnimLib.GetAnims(), null);
		}


		protected override void UnloadContent()
		{
		}


		void InitializeEffect()
		{
			Vector3	lightDir	=new Vector3(1.0f, -1.0f, 0.1f);
			lightDir.Normalize();

			mBFX.AmbientLightColor				=Vector3.One * 0.2f;
			mBFX.LightingEnabled				=true;
			mBFX.DirectionalLight0.Direction	=-lightDir;
			mBFX.DirectionalLight0.Enabled		=true;
			mBFX.DirectionalLight1.Enabled		=false;
			mBFX.DirectionalLight2.Enabled		=false;
			mBFX.DirectionalLight0.DiffuseColor	=new Vector3(0.9f, 0.9f, 0.9f);
		}


		void OnOpenAnim(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			if(ColladaFileUtils.LoadAnim(path, mAnimLib))
			{
				eAnimsUpdated(mAnimLib.GetAnims(), null);
			}
		}


		void OnOpenModel(object sender, EventArgs ea)
		{
			mCharacter	=ColladaFileUtils.LoadCharacter(sender as string, mGDM.GraphicsDevice, mMatLib, mAnimLib);

			mCharacter.SetTransform(Matrix.Identity);

			mbCharacterLoaded	=true;

			mMF.UpdateMeshPartList(mCharacter.GetMeshPartList(), null);
			eAnimsUpdated(mAnimLib.GetAnims(), null);
		}


		void OnNukedMeshPart(object sender, EventArgs ea)
		{
			Mesh	msh	=sender as Mesh;

			if(mbCharacterLoaded)
			{
				mCharacter.NukeMesh(msh);
			}
			else
			{
				mStaticMesh.NukeMesh(msh);
			}
		}


		void OnDeleteVertElement(object sender, EventArgs ea)
		{
			List<int>	indexes	=sender as List<int>;
			if(indexes == null)
			{
				return;
			}

			List<Mesh>	meshes	=mSE.GetMeshes();
			if(meshes == null)
			{
				return;
			}

			Type	firstType	=meshes[0].VertexType;

			foreach(Mesh m in meshes)
			{
				if(m.VertexType == firstType)
				{
					m.NukeVertexElement(indexes, GraphicsDevice);
				}
			}

			if(mbCharacterLoaded)
			{
				mMF.UpdateMeshPartList(mCharacter.GetMeshPartList(), null);
			}
			else
			{
				mMF.UpdateMeshPartList(null, mStaticMesh.GetMeshPartList());
			}

			mSE.Populate(null);
			mSE.Visible	=false;

			mMF.EnableMeshPartGrid();
		}


		void OnStripElements(object sender, EventArgs ea)
		{
			List<Mesh>	meshes	=sender as List<Mesh>;

			mSE.Populate(meshes);
		}


		void OnShowBound(object sender, EventArgs ea)
		{
			Nullable<int>	which	=sender as Nullable<int>;

			if(which == null)
			{
				mBoundPrim	=null;
			}
			else if(which.Value == 0)
			{
				mBoundPrim	=null;
			}
			else if(which.Value == 1)
			{
				ReBuildBoundsDrawData(true);
			}
			else if(which.Value == 2)
			{
				ReBuildBoundsDrawData(false);
			}
		}


		void OnShowAxis(object sender, EventArgs ea)
		{
			Nullable<bool>	show	=sender as Nullable<bool>;

			if(show != null)
			{
				mbDrawAxis	=show.Value;
			}
		}


		void OnBoundMesh(object sender, EventArgs ea)
		{
			if(mbCharacterLoaded)
			{
				mCharacter.UpdateBounds();
			}
			else
			{
				mStaticMesh.UpdateBounds();
			}
		}


		void ReBuildBoundsDrawData(bool bBox)
		{
			BoundingBox		box;
			BoundingSphere	sphere;

			box.Min			=Vector3.Zero;
			box.Max			=Vector3.Zero;
			sphere.Center	=Vector3.Zero;
			sphere.Radius	=0.0f;

			if(mbCharacterLoaded)
			{
				if(bBox)
				{
					box	=mCharacter.GetBoxBound();
				}
				else
				{
					sphere	=mCharacter.GetSphereBound();
				}
			}
			else
			{
				if(bBox)
				{
					box	=mStaticMesh.GetBoxBound();
				}
				else
				{
					sphere	=mStaticMesh.GetSphereBound();
				}
			}

			if(bBox)
			{
				mBoundPrim	=PrimFactory.CreateCube(mGDM.GraphicsDevice, box, null);
			}
			else
			{
				mBoundPrim	=PrimFactory.CreateSphere(mGDM.GraphicsDevice,
					sphere.Center, sphere.Radius, null);
			}
		}


		//non skinned collada model
		void OnOpenStaticModel(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mStaticMesh	=ColladaFileUtils.LoadStatic(path, mGDM.GraphicsDevice,
				mMatLib, mCF.BakeTransforms);

			mStaticMesh.SetTransform(Matrix.Identity);

			mbCharacterLoaded	=false;

			mMF.UpdateMeshPartList(null, mStaticMesh.GetMeshPartList());
		}


		void OnSaveLibrary(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mAnimLib.SaveToFile(path);
		}


		void OnLoadLibrary(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mAnimLib.ReadFromFile(path, true);
			eAnimsUpdated(mAnimLib.GetAnims(), null);
		}


		void OnSaveCharacter(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mCharacter.SaveToFile(path);
		}


		void OnLoadCharacter(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mCharacter	=new Character(mMatLib, mAnimLib);
			mCharacter.ReadFromFile(path, mGDM.GraphicsDevice, true);
			mCharacter.SetTransform(Matrix.Identity);

			mbCharacterLoaded	=true;

			mMF.UpdateMeshPartList(mCharacter.GetMeshPartList(), null);
		}


		void OnTrimStart(object sender, EventArgs ea)
		{
			Nullable<Int32>	amount	=sender as Nullable<Int32>;
			if(amount == null || !amount.HasValue)
			{
				return;
			}

			mSkelFrames.TrimStart(amount.Value);
		}


		void OnTrimEnd(object sender, EventArgs ea)
		{
			Nullable<Int32>	amount	=sender as Nullable<Int32>;
			if(amount == null || !amount.HasValue)
			{
				return;
			}

			mSkelFrames.TrimEnd(amount.Value);
		}


		void OnLoadRawData(object sender, EventArgs ea)
		{
			string	path	=(string)sender;
			if(path == null)
			{
				return;
			}

			FileStream	fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				return;
			}

			BinaryReader	br	=new BinaryReader(fs);

			UInt32	magic	=br.ReadUInt32();
			if(magic != 0x9A1FDA7A)
			{
				br.Close();
				fs.Close();
				return;
			}

			mSkelFrames.Read(br);

			br.Close();
			fs.Close();

			mKF.UpdateCapturedDataStats(mSkelFrames.mFrames.Count, mSkelFrames.mTimes.Last());
		}


		void OnSaveRawData(object sender, EventArgs ea)
		{
			string	path	=(string)sender;
			if(path == null || mSkelFrames.mFrames.Count <= 0)
			{
				return;
			}

			FileStream	fs	=new FileStream(path, FileMode.Create, FileAccess.Write);
			if(fs == null)
			{
				return;
			}

			BinaryWriter	bw	=new BinaryWriter(fs);

			UInt32	magic	=0x9A1FDA7A;

			bw.Write(magic);

			mSkelFrames.Write(bw);

			bw.Close();
			fs.Close();
		}


		void OnConvertToAnim(object sender, EventArgs ea)
		{
			BindingList<KinectMap>	mapping	=sender as BindingList<KinectMap>;
			if(mapping == null)
			{
				return;
			}

			mAnimLib.CreateKinectAnimation(mapping, mSkelFrames,
				"KinectAnim" + mAnimNameCounter++);

			eAnimsUpdated(mAnimLib.GetAnims(), null);
		}


		void OnLoadStatic(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mStaticMesh.ReadFromFile(path, mGDM.GraphicsDevice, true);

			mbCharacterLoaded	=false;

			mMF.UpdateMeshPartList(null, mStaticMesh.GetMeshPartList());
		}


		void OnSaveStatic(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mStaticMesh.SaveToFile(path);
		}


		void OnAnimSelChanged(object sender, EventArgs ea)
		{
			System.Windows.Forms.DataGridViewSelectedRowCollection
				src	=(System.Windows.Forms.DataGridViewSelectedRowCollection)sender;

			foreach(System.Windows.Forms.DataGridViewRow dgvr in src)
			{
				//eventually we'll blend these animations
				//but for now play the first
				mCurrentAnimName	=(string)dgvr.Cells[0].FormattedValue;

				float	totTime		=mAnimLib.GetAnimTime(mCurrentAnimName);
				float	startTime	=mAnimLib.GetAnimStartTime(mCurrentAnimName);
				mCurAnimTime		=(Int64)(totTime * 1000);
				mCurAnimStart		=(Int64)(startTime * 1000);
			}
		}


		void OnTimeScaleChanged(object sender, EventArgs ea)
		{
			Decimal	val	=(Decimal)sender;

			mTimeScale	=Convert.ToSingle(val);
		}


		void StartRecording()
		{
			mSkelFrames.Clear();
			mSensor.Start();
			mSkelStream	=KinectSensor.KinectSensors[0].SkeletonStream;				

			TransformSmoothParameters	tsp	=new TransformSmoothParameters();

			tsp.Correction			=0.5f;
			tsp.JitterRadius		=0.9f;
			tsp.MaxDeviationRadius	=0.15f;
			tsp.Prediction			=0.5f;
			tsp.Smoothing			=0.7f;

			mSkelStream.Enable(tsp);
		}


		void OnPause(object sender, EventArgs ea)
		{
			mbPaused	=!mbPaused;
		}


		void OnToggleRecord(object sender, EventArgs ea)
		{
			Nullable<bool>	useInferred	=sender as Nullable<bool>;
			if(mSensor == null)
			{
				return;
			}

			if(mSensor.IsRunning)
			{
				mSensor.Stop();
				mSkelStream.Disable();
				mSkelStream	=null;
				float	len	=0;

				if(mSkelFrames.mTimes.Count <= 0)
				{
					len	=0f;
				}
				else
				{
					len	=mSkelFrames.mTimes.Last();
				}

				mKF.UpdateCapturedDataStats(mSkelFrames.mFrames.Count, len);
			}
			else if(mbCountingDown)
			{
				mbCountingDown	=false;
			}
			else
			{
				mbCountingDown		=true;
				mCountDown			=5000;
				mbFirstTimeStamp	=true;
			}
		}


		void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs sfrea)
		{
			SkeletonFrame	sf	=sfrea.OpenSkeletonFrame();

			if(sf != null)
			{
				Microsoft.Kinect.Skeleton	[]data	=new Microsoft.Kinect.Skeleton[sf.SkeletonArrayLength];
				sf.CopySkeletonDataTo(data);

				Int64	timeStamp	=sf.Timestamp;

				if(mbFirstTimeStamp)
				{
					mbFirstTimeStamp	=!mbFirstTimeStamp;
					mTimeStampAdjust	=timeStamp;
				}

				mSkelFrames.Add(data, (sf.Timestamp - mTimeStampAdjust) / 1000.0f);
			}
		}


		protected override void Update(GameTime gameTime)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back
				== ButtonState.Pressed)
			{
				Exit();
			}

			int	msDelta	=gameTime.ElapsedGameTime.Milliseconds;

			if(IsActive)
			{
				mInput.Update();
			}

			Input.PlayerInput	pi	=mInput.Player1;

			Vector3	pos	=mSteering.Update(msDelta, mGameCam.Position, mGameCam, pi.mKBS, pi.mMS, pi.mGPS);
			
			mGameCam.Update(-pos, mSteering.Pitch, mSteering.Yaw, mSteering.Roll);

			//rotate the light vector
			if(GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftShoulder))
			{
				mLX	+=(LightXRot * msDelta);
				mLY	+=(LightYRot * msDelta);
				mLZ	+=(LightZRot * msDelta);
			}

			Mathery.WrapAngleDegrees(ref mLX);
			Mathery.WrapAngleDegrees(ref mLY);
			Mathery.WrapAngleDegrees(ref mLZ);

			//build a matrix that spins over time
			Matrix	mat	=Matrix.CreateFromYawPitchRoll(mLY, mLX, mLZ);

			//transform (rotate) the vector
			mLightDir	=Vector3.TransformNormal(Vector3.UnitX, mat);
			mLightDir.Normalize();

			mMatLib.SetParameterOnAll("mLightDirection", mLightDir);

			mMatLib.UpdateWVP(Matrix.Identity, mGameCam.View, mGameCam.Projection, mGameCam.Position);

			if(!mbPaused)
			{
				mAnimTime	+=gameTime.ElapsedGameTime.TotalMilliseconds * mTimeScale;

				if(mCurrentAnimName != "" && ((mCurAnimStart + mCurAnimTime) > 0))
				{
					if(mAnimTime > (mCurAnimTime + mCurAnimStart))
					{
						mAnimTime	%=(mCurAnimTime + mCurAnimStart);
					}

					if(mAnimTime < mCurAnimStart)
					{
						mAnimTime	=mCurAnimStart;
					}
				}
			}

			mCharacter.Animate(mCurrentAnimName, (float)mAnimTime / 1000.0f);

			if(mbCountingDown)
			{
				mCountDown	-=msDelta;
				if(mCountDown <= 0)
				{
					mbCountingDown	=false;
					StartRecording();
				}
			}

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice	g	=mGDM.GraphicsDevice;

			g.Clear(Color.CornflowerBlue);

			mCharacter.Draw(g);
			mStaticMesh.Draw(g);

			if(mbDrawAxis)
			{
				//X axis red
				mBFX.AmbientLightColor	=Vector3.UnitX;
				mXAxis.Draw(g, mBFX, mGameCam.View, mGameCam.Projection);

				//Y axis green
				mBFX.AmbientLightColor	=Vector3.UnitY;
				mYAxis.Draw(g, mBFX, mGameCam.View, mGameCam.Projection);

				//Z axis blue
				mBFX.AmbientLightColor	=Vector3.UnitZ;
				mZAxis.Draw(g, mBFX, mGameCam.View, mGameCam.Projection);
			}

			mBFX.AmbientLightColor	=Vector3.One;

			//draw bounds if any
			if(mBoundPrim != null)
			{
				mBoundPrim.World	=mStaticMesh.GetTransform();
				mBFX.Alpha			=0.5f;

				g.BlendState	=BlendState.NonPremultiplied;

				mBoundPrim.Draw(g, mBFX, mGameCam.View, mGameCam.Projection);
			}

			mSB.Begin();

			mSB.DrawString(mFonts.First().Value, "Coords: " + mGameCam.Position,
				Vector2.One * 20.0f, Color.Yellow);

			if(mbPaused)
			{
				mSB.DrawString(mFonts.First().Value, "Paused at " + mAnimTime / 1000.0f,
					Vector2.UnitX * 20.0f + Vector2.UnitY * 60f, Color.OrangeRed);
			}
			else
			{
				mSB.DrawString(mFonts.First().Value, "AnimTime: " + mAnimTime / 1000.0f,
					Vector2.UnitX * 20.0f + Vector2.UnitY * 60f, Color.GreenYellow);
			}

			if(mSensor != null)
			{
				if(mbCountingDown)
				{
					mSB.DrawString(mFonts.ElementAt(1).Value, "" + mCountDown,
						Vector2.One * 20.0f + Vector2.UnitY * 200, Color.Yellow);
				}
			}

			mSB.End();

			base.Draw(gameTime);
		}
	}
}
