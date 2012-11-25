using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace ParticleEdit
{
	public class ParticleEdit : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSLib;

		ParticleForm				mPF;
		SharedForms.TextureElements	mTexForm;

		ParticleLib.ParticleBoss	mPB;

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
			mView	=Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
			mProj	=Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0.01f, 1000.0f);
			mWorld	=Matrix.Identity;

			base.Initialize();
		}


		protected override void LoadContent()
		{
			mSB	=new SpriteBatch(GraphicsDevice);

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