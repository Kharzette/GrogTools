using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

		//control
		PlayerSteering	mSteering;
		GameCamera		mGameCam;
		Input			mInput;

		//material gui
		SharedForms.MaterialForm	mMF;

		//animation gui
		AnimForm	mAnimForm;
		string		mCurrentAnimName;
		float		mTimeScale;			//anim playback speed
		Int64		mCurAnimTime, mCurAnimStart;
		Vector3		mLightDir;
		Random		mRand	=new Random();

		//skeleton editor
		SkeletonEditor	mSkelEditor	=new SkeletonEditor();

		//kinect gui
//		KinectForm	mKF;

		//celshade tweaker form
		SharedForms.CelTweakForm	mCTF;

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

			//set up cel shading
			mMatLib.InitCelShading(1);

			//set to character settings
			mMatLib.GenerateCelTexturePreset(gd, true, 0);
			mMatLib.SetCelTexture(0);

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

			mCTF			=new SharedForms.CelTweakForm(gd, mMatLib);
			mCTF.Visible	=true;

			mAnimForm				=new AnimForm(mAnimLib);
			mAnimForm.Visible		=true;

			mAnimForm.eLoadAnim				+=OnOpenAnim;
			mAnimForm.eLoadModel				+=OnOpenModel;
			mAnimForm.eLoadStaticModel		+=OnOpenStaticModel;
			mAnimForm.eAnimSelectionChanged	+=OnAnimSelChanged;
			mAnimForm.eTimeScaleChanged		+=OnTimeScaleChanged;
			mAnimForm.eSaveLibrary			+=OnSaveLibrary;
			mAnimForm.eSaveCharacter			+=OnSaveCharacter;
			mAnimForm.eLoadCharacter			+=OnLoadCharacter;
			mAnimForm.eLoadLibrary			+=OnLoadLibrary;
			mAnimForm.eLoadStatic				+=OnLoadStatic;
			mAnimForm.eSaveStatic				+=OnSaveStatic;
			mAnimForm.eBoundMesh				+=OnBoundMesh;
			mAnimForm.eShowBound				+=OnShowBound;
			mAnimForm.eShowAxis				+=OnShowAxis;
			mAnimForm.ePause					+=OnPause;

			mMF	=new SharedForms.MaterialForm(gd, mMatLib, true);
			mMF.Visible	=true;

			mSkelEditor.Visible	=true;

			/*
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
			mKF.eTrimEnd		+=OnTrimEnd;*/

			//bind matform window position
			mMF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			//bind animform window position
			mAnimForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "AnimFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mSkelEditor.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "SkeletonEditorFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mSkelEditor.DataBindings.Add(new System.Windows.Forms.Binding("Size",
				Settings.Default, "SkeletonEditorFormSize", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			//bind celTweakForm window position
			mCTF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "CelTweakFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mMF.eNukedMeshPart	+=OnNukedMeshPart;
			mMF.eStripElements	+=OnStripElements;
			mMF.eWeldWeights	+=OnWeldWeights;

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

			if(ColladaFileUtils.LoadAnim(path, mAnimLib, mAnimForm.GetCheckSkeleton()))
			{
				eAnimsUpdated(mAnimLib.GetAnims(), null);
				mSkelEditor.Initialize(mAnimLib.GetSkeleton());
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


		void OnWeldWeights(object sender, EventArgs ea)
		{
			List<Mesh>	meshes	=sender as List<Mesh>;

			Debug.Assert(meshes.Count == 2);

			meshes[0].WeldWeights(GraphicsDevice, meshes[1]);
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

			mStaticMesh	=ColladaFileUtils.LoadStatic(path, mGDM.GraphicsDevice,	mMatLib);

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
			mSkelEditor.Initialize(mAnimLib.GetSkeleton());
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


		void OnPause(object sender, EventArgs ea)
		{
			mbPaused	=!mbPaused;
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

			mSB.End();

			base.Draw(gameTime);
		}
	}
}
