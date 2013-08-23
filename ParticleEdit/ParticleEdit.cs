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

		//fonts
		Dictionary<string, SpriteFont>	mFonts;

		ParticleForm				mPF;
		SharedForms.TextureElements	mTexForm;

		ParticleLib.ParticleBoss	mPB;

		string	mCurTex;
		bool	mbActive		=true;
		int		mCurSelection	=-1;

		//camera controls
		GameCamera		mGCam;
		PlayerSteering	mPS;
		Input			mInput;

		//debug stuff
		PrimObject	mXAxis, mYAxis, mZAxis;
		BasicEffect	mBFX;

		//constants
		const float	AxisSize	=50f;


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
			GraphicsDevice	gd	=GraphicsDevice;

			mSB	=new SpriteBatch(gd);

			mFX	=mSLib.Load<Effect>("Shaders/Static");

			mFonts	=FileUtil.LoadAllFonts(Content);

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
			mPF.eItemNuked				+=OnEmitterNuked;
			mPF.eCellChanged			+=OnCellChanged;
			mPF.eValueChanged			+=OnValueChanged;
			mPF.eSelectionChanged		+=OnEmitterSelChanged;
			mPF.eCopyEmitterToClipBoard	+=OnCopyEmitterToClipBoard;
			mTexForm.eTexDictChanged	+=OnTexDictChanged;
			mTexForm.eTexChanged		+=OnTexChanged;

			//debug axis boxes
			BoundingBox	xBox	=Misc.MakeBox(AxisSize, 1f, 1f);
			BoundingBox	yBox	=Misc.MakeBox(1f, AxisSize, 1f);
			BoundingBox	zBox	=Misc.MakeBox(1f, 1f, AxisSize);

			mXAxis	=PrimFactory.CreateCube(gd, xBox, null);
			mYAxis	=PrimFactory.CreateCube(gd, yBox, null);
			mZAxis	=PrimFactory.CreateCube(gd, zBox, null);

			mBFX	=new BasicEffect(gd);
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


		protected override void UnloadContent()
		{
		}


		protected override void Update(GameTime gameTime)
		{
			int	msDelta	=gameTime.ElapsedGameTime.Milliseconds;

			if(IsActive)
			{
				mInput.Update();
			}

			Input.PlayerInput	pi	=mInput.Player1;

			Vector3	newPos	=mPS.Update(msDelta, mGCam.Position, mGCam, pi.mKBS, pi.mMS, pi.mGPS);

			mGCam.Update(-newPos, mPS.Pitch, mPS.Yaw, mPS.Roll);

			if(mbActive && mPB != null)
			{
				mPB.Update(msDelta);
			}

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice	gd	=GraphicsDevice;

			gd.Clear(Color.CornflowerBlue);

			if(mbActive && mPB != null)
			{
				mPB.Draw(mGCam.View, mGCam.Projection);
			}

			//X axis red
			mBFX.AmbientLightColor	=Vector3.UnitX;
			mXAxis.Draw(gd, mBFX, mGCam.View, mGCam.Projection);

			//Y axis green
			mBFX.AmbientLightColor	=Vector3.UnitY;
			mYAxis.Draw(gd, mBFX, mGCam.View, mGCam.Projection);

			//Z axis blue
			mBFX.AmbientLightColor	=Vector3.UnitZ;
			mZAxis.Draw(gd, mBFX, mGCam.View, mGCam.Projection);

			mSB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			mSB.DrawString(mFonts.Values.First(), "Coords: " + mGCam.Position, Vector2.One * 20.0f, Color.Yellow);

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
			float	str		=mPF.GravStrength;

			Mathery.WrapAngleDegrees(ref yaw);
			Mathery.WrapAngleDegrees(ref pitch);

			yaw		=MathHelper.ToRadians(yaw);
			pitch	=MathHelper.ToRadians(pitch);

			mPB.CreateEmitter(mCurTex, mPF.PartColor, mPF.IsCell,
				mPF.EmShape, mPF.EmShapeSize,
				mPF.MaxParts, Vector3.Zero,
				mPF.GravYaw, mPF.GravPitch, mPF.GravStrength,
				mPF.StartingSize, mPF.StartingAlpha, mPF.EmitMS,
				mPF.SpinMin, mPF.SpinMax, mPF.VelMin, mPF.VelMax,
				mPF.SizeMin, mPF.SizeMax, mPF.AlphaMin,
				mPF.AlphaMax, mPF.LifeMin, mPF.LifeMax);

			UpdateListView();
		}


		void OnCellChanged(object sender, EventArgs ea)
		{
			Nullable<bool>	bOn	=sender as Nullable<bool>;
			if(bOn == null || mCurSelection < 0)
			{
				return;
			}

			mPB.SetCellByIndex(mCurSelection, bOn.Value);
		}


		void OnValueChanged(object sender, EventArgs ea)
		{
			if(mCurSelection < 0)
			{
				return;
			}

			ParticleLib.Emitter	em	=mPB.GetEmitterByIndex(mCurSelection);
			if(em == null)
			{
				return;
			}

			mPF.UpdateEmitter(em);

			mPB.SetColorByIndex(mCurSelection, mPF.PartColor);
		}


		void OnCopyEmitterToClipBoard(object sender, EventArgs ea)
		{
			Nullable<int>	index	=sender as Nullable<int>;
			if(index == null)
			{
				return;
			}

			string	ent	=mPB.GetEmitterEntityString(index.Value);
			if(ent != null && ent != "")
			{
				System.Windows.Forms.Clipboard.SetText(ent);
			}
		}


		void OnEmitterNuked(object sender, EventArgs ea)
		{
			Nullable<int>	index	=sender as Nullable<int>;
			if(index == null)
			{
				return;
			}
			mPB.NukeEmitter(index.Value);

			UpdateListView();
		}


		void OnEmitterSelChanged(object sender, EventArgs ea)
		{
			Nullable<int>	index	=sender as Nullable<int>;
			if(index == null)
			{
				return;
			}

			mCurSelection	=index.Value;

			UpdateControls(index.Value);

			//set the texture form selection
			if(mPB != null && mCurSelection >= 0)
			{
				mTexForm.SetCurrentTexElement(mPB.GetTexturePathByIndex(mCurSelection));
			}
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
				Texture2D	tex2D	=tex.Value.GetTexture(0);
				tex2D.Name			=tex.Key;

				texs.Add(tex.Key, tex2D);
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

			if(mPB != null && mCurSelection >= 0)
			{
				mPB.SetTextureByIndex(mCurSelection, te.GetTexture(0));
			}
		}


		void OnAppDeactivated(object sender, EventArgs e)
		{
			mbActive	=false;
		}


		void OnAppActivated(object sender, EventArgs e)
		{
			mbActive	=true;
		}


		void UpdateListView()
		{
			int	count	=mPB.GetEmitterCount();
			if(count <= 0)
			{
				return;
			}

			List<string>	emitters	=new List<string>();

			for(int i=0;i < count;i++)
			{
				ParticleLib.Emitter	em	=mPB.GetEmitterByIndex(i);

				if(em == null)
				{
					continue;
				}

				emitters.Add("Emitter" + string.Format("{0:000}", i));
			}

			mPF.UpdateListView(emitters);
		}


		void UpdateControls(int index)
		{
			if(index < 0)
			{
				return;
			}

			mPF.UpdateControls(mPB.GetEmitterByIndex(index),
				mPB.GetColorByIndex(index), mPB.GetCellByIndex(index));
		}
	}
}