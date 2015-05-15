using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UtilityLib;


namespace TerrainEdit
{
	internal partial class TerrainShading : Form
	{
		internal class ShadingInfo
		{
			internal bool	mbFogEnabled;
			internal float	mFogStart, mFogEnd;
			internal int	mChunkRange;

			internal SharpDX.Color	mFogColor, mSkyColor0, mSkyColor1;
		};

		ColorDialog	mColor;

		internal event EventHandler	eApply;


		public TerrainShading()
		{
			InitializeComponent();

			mColor	=new ColorDialog();

			mColor.SolidColorOnly	=true;
			mColor.AllowFullOpen	=true;
			mColor.AnyColor			=true;
			mColor.FullOpen			=true;

			//save colors
			FogColor.DataBindings.Add(new Binding("BackColor",
				Settings.Default, "FogColor", true,
				DataSourceUpdateMode.OnPropertyChanged));
			SkyColor0.DataBindings.Add(new Binding("BackColor",
				Settings.Default, "SkyColor0", true,
				DataSourceUpdateMode.OnPropertyChanged));
			SkyColor1.DataBindings.Add(new Binding("BackColor",
				Settings.Default, "SkyColor1", true,
				DataSourceUpdateMode.OnPropertyChanged));

			//save numerics
			FogStart.DataBindings.Add(new Binding("Value",
				Settings.Default, "FogStart", true,
				DataSourceUpdateMode.OnPropertyChanged));
			FogEnd.DataBindings.Add(new Binding("Value",
				Settings.Default, "FogEnd", true,
				DataSourceUpdateMode.OnPropertyChanged));
			ChunkRange.DataBindings.Add(new Binding("Value",
				Settings.Default, "ChunkRange", true,
				DataSourceUpdateMode.OnPropertyChanged));

			//checkem
			FogEnabled.DataBindings.Add(new Binding("Checked",
				Settings.Default, "FogEnabled", true,
				DataSourceUpdateMode.OnPropertyChanged));
		}


		void OnFogColorClicked(object sender, EventArgs e)
		{
			mColor.ShowDialog();

			FogColor.BackColor	=mColor.Color;
		}


		void OnColor0Clicked(object sender, EventArgs e)
		{
			mColor.ShowDialog();

			SkyColor0.BackColor	=mColor.Color;
		}


		void OnColor1Clicked(object sender, EventArgs e)
		{
			mColor.ShowDialog();

			SkyColor1.BackColor	=mColor.Color;
		}


		void OnApply(object sender, EventArgs e)
		{
			ShadingInfo	si	=new ShadingInfo();

			si.mbFogEnabled	=FogEnabled.Checked;
			si.mFogStart	=(float)FogStart.Value;
			si.mFogEnd		=(float)FogEnd.Value;
			si.mChunkRange	=(int)ChunkRange.Value;

			si.mFogColor	=Misc.SystemColorToDXColor(FogColor.BackColor);
			si.mSkyColor0	=Misc.SystemColorToDXColor(SkyColor0.BackColor);
			si.mSkyColor1	=Misc.SystemColorToDXColor(SkyColor1.BackColor);

			Misc.SafeInvoke(eApply, si);
		}
	}
}
