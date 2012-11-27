using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteMapLib;


namespace ParticleEdit
{
	public class ParticleEdit : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSLib;
		Effect					mFX;

		ParticleForm				mPF;
		SharedForms.TextureElements	mTexForm;

		ParticleLib.ParticleBoss	mPB;

		string	mCurTex;
		bool	mbActive	=true;
		Matrix	mWorld, mView, mProj;

		MouseState	mCurMS, mPrevMS;


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
		}


		protected override void Initialize()
		{
			mView	=Matrix.CreateLookAt(Vector3.Backward * 10f, Vector3.Zero, Vector3.Up);
			mProj	=Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 800f / 600f, 0.01f, 1000f);
			mWorld	=Matrix.Identity;

			base.Initialize();
		}


		protected override void LoadContent()
		{
			mSB	=new SpriteBatch(GraphicsDevice);

			mFX	=mSLib.Load<Effect>("Shaders/Static");

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
			KeyboardState	keyState	=Keyboard.GetState();

			mPrevMS	=mCurMS;
			mCurMS	=Mouse.GetState();

			int	msDelta	=gameTime.ElapsedGameTime.Milliseconds;

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
				mPB.Draw(mView, mProj);
			}

			base.Draw(gameTime);
		}


		void OnCreate(object sender, EventArgs ea)
		{
			if(mCurTex == null || mCurTex == "")
			{
				return;
			}

			mPB.CreateEmitter(mCurTex,
				mPF.MaxParts, Vector3.Zero, mPF.StartingSize,
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