using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteMapLib;
using UtilityLib;


namespace ParticleEdit
{
	public class ParticleEdit : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSLib;
		Effect					mFX;
		SpriteFont				mPescadero12;

		ParticleForm				mPF;
		SharedForms.TextureElements	mTexForm;

		ParticleLib.ParticleBoss	mPB;

		string	mCurTex;
		bool	mbActive	=true;

		//camera controls
		GameCamera		mGCam;
		PlayerSteering	mPS;
		Input			mInput;


		public ParticleEdit()
		{
			mGDM	=new GraphicsDeviceManager(this);
			mSLib	=new ContentManager(Services, "ShaderLib");

			Content.RootDirectory	="GameContent";

			//turn on mouse cursor
			IsMouseVisible	=true;

			Activated	+=OnAppActivated;
			Deactivated	+=OnAppDeactivated;

			//device screen size
			mGDM.PreferredBackBufferWidth	=800;
			mGDM.PreferredBackBufferHeight	=600;

			//set window position
			if(!mGDM.IsFullScreen)
			{
				System.Windows.Forms.Control	mainWindow
					=System.Windows.Forms.Form.FromHandle(this.Window.Handle);

				//add data binding so it will save
				mainWindow.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					global::ParticleEdit.Properties.Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

				mainWindow.Location	=
					global::ParticleEdit.Properties.Settings.Default.MainWindowPos;
			}

			mGCam	=new GameCamera(800, 600, 16f/9f, 1f, 2000f);
			mPS		=new PlayerSteering(800, 600);
			mInput	=new Input();

			mPS.Method	=PlayerSteering.SteeringMethod.Fly;
		}


		protected override void Initialize()
		{
			base.Initialize();
		}


		protected override void LoadContent()
		{
			mSB	=new SpriteBatch(GraphicsDevice);

			mFX	=mSLib.Load<Effect>("Shaders/Static");

			mPescadero12	=Content.Load<SpriteFont>("Fonts/Pescadero12");

			mPF			=new ParticleForm();
			mPF.Visible	=true;

			mTexForm			=new SharedForms.TextureElements(Content);
			mTexForm.Visible	=true;

			//add data bindings for positions of forms
			mPF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::ParticleEdit.Properties.Settings.Default,
				"ParticleFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mTexForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::ParticleEdit.Properties.Settings.Default,
				"TexFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mPF.eCreate					+=OnCreate;
			mTexForm.eTexDictChanged	+=OnTexDictChanged;
			mTexForm.eTexChanged		+=OnTexChanged;


			//hack to get pix working
			Dictionary<string, TextureElement>	texLib	=new Dictionary<string,TextureElement>();
			
			TextureElement.LoadTexLib(Content.RootDirectory + "/TexLibs/Particles.TexLib", Content, texLib);

			Dictionary<string, Texture2D>	texs	=new Dictionary<string, Texture2D>();

			foreach(KeyValuePair<string, TextureElement> tex in texLib)
			{
				texs.Add(tex.Key, tex.Value.GetTexture(0));

				mCurTex	=tex.Key;
			}

			mPB	=new ParticleLib.ParticleBoss(mGDM.GraphicsDevice, mFX, texs);
		}


		protected override void UnloadContent()
		{
		}


		protected override void Update(GameTime gameTime)
		{
			int	msDelta	=gameTime.ElapsedGameTime.Milliseconds;

			mInput.Update();

			Input.PlayerInput	pi	=mInput.Player1;

			mPS.Update(msDelta, mGCam, pi.mKBS, pi.mMS, pi.mGPS);

			mGCam.Update(-mPS.Position, mPS.Pitch, mPS.Yaw, mPS.Roll);

			if(mbActive && mPB != null)
			{
				mPB.Update(msDelta);
			}

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			if(mbActive && mPB != null)
			{
				mPB.Draw(mGCam.View, mGCam.Projection);
			}

			mSB.Begin();

			mSB.DrawString(mPescadero12, "Coords: " + mPS.Position, Vector2.One * 20.0f, Color.Yellow);

			mSB.End();

			base.Draw(gameTime);
		}


		void OnCreate(object sender, EventArgs ea)
		{
			if(mCurTex == null || mCurTex == "")
			{
				return;
			}

			float	yaw		=mPF.GravYaw;
			float	pitch	=mPF.GravPitch;
			float	roll	=mPF.GravRoll;
			float	str		=mPF.GravStrength;

			Mathery.WrapAngleDegrees(ref yaw);
			Mathery.WrapAngleDegrees(ref pitch);
			Mathery.WrapAngleDegrees(ref roll);

			yaw		=MathHelper.ToRadians(yaw);
			pitch	=MathHelper.ToRadians(pitch);
			roll	=MathHelper.ToRadians(roll);

			Matrix	gravMat	=Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

			Vector3	gravity	=Vector3.TransformNormal(Vector3.UnitX, gravMat);

			gravity	*=str;

			mPB.CreateEmitter(mCurTex,
				mPF.MaxParts, Vector3.Zero, gravity,
				mPF.StartingSize, mPF.StartingAlpha,
				mPF.PartDuration * 1000, mPF.EmitMS, mPF.SpinMin,
				mPF.SpinMax, mPF.VelMin, mPF.VelMax,
				mPF.SizeMin, mPF.SizeMax, mPF.AlphaMin,
				mPF.AlphaMax, mPF.LifeMin, mPF.LifeMax);
		}


		void OnTexDictChanged(object sender, EventArgs ea)
		{
			Dictionary<string, TextureElement>	texDict	=sender as Dictionary<string, TextureElement>;
			if(texDict == null)
			{
				return;
			}

			Dictionary<string, Texture2D>	texs	=new Dictionary<string, Texture2D>();

			foreach(KeyValuePair<string, TextureElement> tex in texDict)
			{
				texs.Add(tex.Key, tex.Value.GetTexture(0));
			}

			mPB	=new ParticleLib.ParticleBoss(mGDM.GraphicsDevice, mFX, texs);
		}


		void OnTexChanged(object sender, EventArgs ea)
		{
			TextureElement	te	=sender as TextureElement;
			if(te == null)
			{
				return;
			}
			mCurTex	=te.Asset_Path;
		}


		void OnAppDeactivated(object sender, EventArgs e)
		{
			mbActive	=false;
		}


		void OnAppActivated(object sender, EventArgs e)
		{
			mbActive	=true;
		}
	}
}