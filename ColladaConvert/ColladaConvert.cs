using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using MeshLib;


namespace ColladaConvert
{
	public class ColladaConvert : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mGameCM, mShaderLib;
		VertexBuffer			mVB;
		IndexBuffer				mIB;
		Effect					mFX;
		BasicEffect				mBFX;
		SpriteFont				mPesc12;
		UtilityLib.PrimObject	mBoundPrim;

		MaterialLib.MaterialLib	mMatLib;
		AnimLib					mAnimLib;
		Character				mCharacter;
		StaticMeshObject		mStaticMesh;
		bool					mbCharacterLoaded;	//so I know which mesh obj to use
		
		UtilityLib.PlayerSteering	mSteering;
		UtilityLib.GameCamera		mGameCam;
		UtilityLib.Input			mInput;

		//material gui
		SharedForms.MaterialForm	mMF;

		//animation gui
		AnimForm	mCF;
		string		mCurrentAnimName;
		float		mTimeScale;			//anim playback speed

		Texture2D	mDesu;
		Texture2D	mEureka;
		Vector3		mLightDir;
		Random		mRand	=new Random();


		public static event EventHandler	eAnimsUpdated;

		public ColladaConvert()
		{
			mGDM	=new GraphicsDeviceManager(this);
			Content.RootDirectory	="Content";

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
			mGameCam	=new UtilityLib.GameCamera(mGDM.GraphicsDevice.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height,
				mGDM.GraphicsDevice.Viewport.AspectRatio,
				0.1f, 1000.0f);

			mSteering	=new UtilityLib.PlayerSteering(mGDM.GraphicsDevice.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height);

			mSteering.Method	=UtilityLib.PlayerSteering.SteeringMethod.Fly;
			mSteering.Speed		=0.001f;

			mInput	=new UtilityLib.Input();

			//default cam pos off to one side
//			Vector3	camPos	=Vector3.Zero;
//			camPos.X	=102.0f;
//			camPos.Y	=-96.0f;
//			camPos.Z	=187.0f;

//			mGameCam.CamPos	=-camPos;

			base.Initialize();
		}


		protected override void LoadContent()
		{
			mSB			=new SpriteBatch(mGDM.GraphicsDevice);
			mGameCM		=new ContentManager(Services, "GameContent");
			mShaderLib	=new ContentManager(Services, "ShaderLib");
			mMatLib		=new MaterialLib.MaterialLib(mGDM.GraphicsDevice, mGameCM, mShaderLib, true);
			mAnimLib	=new AnimLib();
			mBFX		=new BasicEffect(mGDM.GraphicsDevice);
			mCharacter	=new Character(mMatLib, mAnimLib);
			mStaticMesh	=new StaticMeshObject(mMatLib);

			mStaticMesh.SetTransform(Matrix.Identity);
			mCharacter.SetTransform(Matrix.Identity);

			//load debug shaders
			mFX		=mShaderLib.Load<Effect>("Shaders/Static");
			mPesc12	=mGameCM.Load<SpriteFont>("Fonts/Pescadero12");

			mDesu	=Content.Load<Texture2D>("Textures/desu");
			mEureka	=Content.Load<Texture2D>("Textures/Eureka");

			Point	topLeft, bottomRight;
			topLeft.X		=0;
			topLeft.Y		=0;
			bottomRight.X	=5;
			bottomRight.Y	=10;

			//fill in some verts two quads
			VertexPositionNormalTexture	[]verts	=new VertexPositionNormalTexture[8];
			verts[0].Position.X	=topLeft.X;
			verts[1].Position.X	=bottomRight.X;
			verts[2].Position.X	=topLeft.X;
			verts[3].Position.X	=bottomRight.X;

			verts[4].Position.Z	=topLeft.X;
			verts[5].Position.Z	=bottomRight.X;
			verts[6].Position.Z	=topLeft.X;
			verts[7].Position.Z	=bottomRight.X;

			verts[0].Position.Y	=topLeft.Y;
			verts[1].Position.Y	=topLeft.Y;
			verts[2].Position.Y	=bottomRight.Y;
			verts[3].Position.Y	=bottomRight.Y;

			verts[4].Position.Y	=topLeft.Y;
			verts[5].Position.Y	=topLeft.Y;
			verts[6].Position.Y	=bottomRight.Y;
			verts[7].Position.Y	=bottomRight.Y;

			verts[0].TextureCoordinate	=Vector2.UnitY;
			verts[1].TextureCoordinate	=Vector2.UnitX + Vector2.UnitY;
			verts[3].TextureCoordinate	=Vector2.UnitX;
			verts[4].TextureCoordinate	=Vector2.UnitY;
			verts[5].TextureCoordinate	=Vector2.UnitX + Vector2.UnitY;
			verts[7].TextureCoordinate	=Vector2.UnitX;

			//create vertex and index buffers
			mIB	=new IndexBuffer(mGDM.GraphicsDevice, IndexElementSize.SixteenBits, 12, BufferUsage.WriteOnly);
			mVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionNormalTexture), 8, BufferUsage.WriteOnly);

			//put our data into the vertex buffer
			mVB.SetData<VertexPositionNormalTexture>(verts);

			//mark the indexes
			ushort	[]ind	=new ushort[12];
			ind[0]	=0;
			ind[1]	=1;
			ind[2]	=2;
			ind[3]	=2;
			ind[4]	=1;
			ind[5]	=3;
			ind[6]	=4;
			ind[7]	=5;
			ind[8]	=6;
			ind[9]	=6;
			ind[10]	=5;
			ind[11]	=7;

			//fill in index buffer
			mIB.SetData<ushort>(ind);

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

			mMF	=new SharedForms.MaterialForm(mGDM.GraphicsDevice, mMatLib, true);
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
			mBFX.Alpha							=0.5f;
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

			UtilityLib.Misc.SafeInvoke(eAnimsUpdated, mAnimLib.GetAnims());
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
				mBoundPrim	=UtilityLib.PrimFactory.CreateCube(mGDM.GraphicsDevice, box, null);
			}
			else
			{
				mBoundPrim	=UtilityLib.PrimFactory.CreateSphere(mGDM.GraphicsDevice,
					sphere.Center, sphere.Radius, null);
			}
		}


		//non skinned collada model
		void OnOpenStaticModel(object sender, EventArgs ea)
		{
			string	path	=(string)sender;

			mStaticMesh	=ColladaFileUtils.LoadStatic(path, mGDM.GraphicsDevice, mMatLib);

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

			UtilityLib.Input.PlayerInput	pi	=mInput.Player1;

			mSteering.Update(msDelta, mGameCam, pi.mKBS, pi.mMS, pi.mGPS);
			
			mGameCam.Update(-mSteering.Position, mSteering.Pitch, mSteering.Yaw, mSteering.Roll);

			//rotate the light vector

			//grab a time value to use to spin the axii
			float spinAmount	=gameTime.TotalGameTime.Milliseconds;

			//scale it back a bit
			spinAmount	*=0.00001f;

			//build a matrix that spins over time
			Matrix	mat	=Matrix.CreateFromYawPitchRoll
				(spinAmount * 3.0f,
				spinAmount,
				spinAmount * 0.5f);

			//transform (rotate) the vector
			mLightDir	=Vector3.TransformNormal(mLightDir, mat);

			//update it in the shader
			mFX.Parameters["mLightDirection"].SetValue(mLightDir);
			mFX.Parameters["mTexture"].SetValue(mDesu);

			mMatLib.UpdateWVP(Matrix.Identity, mGameCam.View, mGameCam.Projection, mSteering.Position);

			//put in some keys for messing with bones
			float	time		=(float)gameTime.ElapsedGameTime.TotalMilliseconds;

			mCharacter.Animate(mCurrentAnimName, (float)(gameTime.TotalGameTime.TotalSeconds) * mTimeScale);

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice	g	=mGDM.GraphicsDevice;

			g.Clear(Color.CornflowerBlue);

			mCharacter.Draw(g);
			mStaticMesh.Draw(g);

			g.SetVertexBuffer(mVB);
			g.Indices	=mIB;

			//default light direction
			mLightDir.X	=-0.3f;
			mLightDir.Y	=-1.0f;
			mLightDir.Z	=-0.2f;
			mLightDir.Normalize();

			mFX.Parameters["mLightDirection"].SetValue(mLightDir);

			g.BlendState	=BlendState.AlphaBlend;

			mFX.CurrentTechnique	=mFX.Techniques[0];
			
			mFX.Parameters["mTexture"].SetValue(mEureka);

			mFX.CurrentTechnique.Passes[0].Apply();

			g.DrawIndexedPrimitives(PrimitiveType.TriangleList,
				4, 0, 4, 0, 2);


			mFX.Parameters["mTexture"].SetValue(mDesu);

			mFX.CurrentTechnique.Passes[0].Apply();

			g.DrawIndexedPrimitives(PrimitiveType.TriangleList,
				0, 0, 4, 0, 2);

			//draw bounds if any
			if(mBoundPrim != null)
			{
				mBoundPrim.Draw(g, mBFX, mGameCam.View, mGameCam.Projection);
			}

			mSB.Begin();
			mSB.DrawString(mPesc12, "Coords: " + mSteering.Position,
					Vector2.One * 20.0f, Color.Yellow);
			mSB.End();

			base.Draw(gameTime);
		}
	}
}
