using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UtilityLib;
using MeshLib;


namespace ColladaConvert
{
	public class ColladaConvert : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSLib;
		BasicEffect				mBFX;
		PrimObject				mBoundPrim;
		PrimObject				mXAxis, mYAxis, mZAxis;

		//fonts
		Dictionary<string, SpriteFont>	mFonts;
		SpriteFont						mFirstFont;

		MaterialLib.MaterialLib	mMatLib;
		AnimLib					mAnimLib;
		Character				mCharacter;
		StaticMeshObject		mStaticMesh;
		bool					mbCharacterLoaded;	//so I know which mesh obj to use
		bool					mbDrawAxis	=true;
		
		PlayerSteering	mSteering;
		GameCamera		mGameCam;
		Input			mInput;

		//material gui
		SharedForms.MaterialForm	mMF;

		//animation gui
		AnimForm	mCF;
		string		mCurrentAnimName;
		float		mTimeScale;			//anim playback speed
		Vector3		mLightDir;
		Random		mRand	=new Random();

		//spinning light
		float	mLX, mLY, mLZ;

		public static event EventHandler	eAnimsUpdated;

		const float	LightXRot	=0.00003f;
		const float	LightYRot	=0.00009f;
		const float	LightZRot	=0.00006f;
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

			mFonts	=FileUtil.LoadAllFonts(Content);
			foreach(KeyValuePair<string, SpriteFont> font in mFonts)
			{
				mFirstFont	=font.Value;
				break;
			}

			mSB			=new SpriteBatch(gd);
			mSLib		=new ContentManager(Services, "ShaderLib");
			mMatLib		=new MaterialLib.MaterialLib(gd, Content, mSLib, true);
			mAnimLib	=new AnimLib();
			mBFX		=new BasicEffect(gd);
			mCharacter	=new Character(mMatLib, mAnimLib);
			mStaticMesh	=new StaticMeshObject(mMatLib);

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

			mCF	=new AnimForm(mAnimLib);
			mCF.Visible	=true;

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
			mCF.eLoadMotionDat			+=OnLoadMotionDat;
			mCF.eLoadBoneMap			+=OnLoadBoneMap;
			mCF.eBoundMesh				+=OnBoundMesh;
			mCF.eShowBound				+=OnShowBound;
			mCF.eShowAxis				+=OnShowAxis;

			mMF	=new SharedForms.MaterialForm(gd, mMatLib, true);
			mMF.Visible	=true;

			//bind matform window position
			mMF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::ColladaConvert.Settings.Default,
				"MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			//bind animform window position
			mCF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::ColladaConvert.Settings.Default,
				"AnimFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mMF.eNukedMeshPart	+=OnNukedMeshPart;

			mSteering	=new PlayerSteering(gd.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height);
			mSteering.Method	=PlayerSteering.SteeringMethod.Fly;
			mSteering.Speed		=0.2f;
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


		void OnLoadMotionDat(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			mAnimLib.LoadKinectMotionDat(fileName);

			Misc.SafeInvoke(eAnimsUpdated, mAnimLib.GetAnims());
		}


		void OnLoadBoneMap(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			mAnimLib.LoadBoneMap(fileName);
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

			mMF.UpdateMeshPartList(mCharacter.GetMeshPartList(), null);
		}


		void OnLoadStatic(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mStaticMesh.ReadFromFile(path, mGDM.GraphicsDevice, true);

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

			mInput.Update();

			Input.PlayerInput	pi	=mInput.Player1;

			mSteering.Update(msDelta, mGameCam, pi.mKBS, pi.mMS, pi.mGPS);
			
			mGameCam.Update(-mSteering.Position, mSteering.Pitch, mSteering.Yaw, mSteering.Roll);

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

			mMatLib.UpdateWVP(Matrix.Identity, mGameCam.View, mGameCam.Projection, mSteering.Position);

			//put in some keys for messing with bones
			float	time		=(float)gameTime.ElapsedGameTime.TotalMilliseconds;

			mCharacter.Animate(mCurrentAnimName, (float)(gameTime.TotalGameTime.TotalSeconds) * mTimeScale);

			//hotkeys
			if(mInput.Player1.WasKeyPressed(Keys.M))
			{
				//this goes off when typing in text fields!
//				mMF.ApplyMat();
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
			mSB.DrawString(mFirstFont, "Coords: " + mSteering.Position,
					Vector2.One * 20.0f, Color.Yellow);
			mSB.End();

			base.Draw(gameTime);
		}
	}
}
