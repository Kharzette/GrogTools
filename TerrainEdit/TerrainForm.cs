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
	internal partial class TerrainForm : Form
	{
		internal event EventHandler	eBuild;


		internal TerrainForm()
		{
			InitializeComponent();

			foreach(object obj in Controls)
			{
				GroupBox	gb	=obj as GroupBox;
				if(gb == null)
				{
					continue;
				}

				foreach(object gbObj in gb.Controls)
				{
					NumericUpDown	num	=gbObj as NumericUpDown;
					if(num == null)
					{
						continue;
					}
					BindToSettings(num);
				}
			}
		}


		internal void GetBuildData(out int gridSize, out int chunkSize,
			out float medianHeight, out float variance, out int polySize,
			out int tilingIterations, out float borderSize,
			out int smoothPasses, out int seed,
			out int erosionIterations, out float rainFall,
			out float solubility, out float evaporation,
			out int streamingThreads)
		{
			gridSize			=(int)GridSize.Value;
			chunkSize			=(int)ChunkSize.Value;
			medianHeight		=(float)MedianHeight.Value;
			variance			=(float)Variance.Value;
			polySize			=(int)PolySize.Value;
			tilingIterations	=(int)TileIterations.Value;
			borderSize			=(float)BorderSize.Value;
			erosionIterations	=(int)ErosionIterations.Value;
			rainFall			=(float)RainFall.Value;
			solubility			=(float)Solubility.Value;
			evaporation			=(float)Evaporation.Value;
			seed				=(int)Seed.Value;
			smoothPasses		=(int)SmoothPasses.Value;
			streamingThreads	=(int)StreamingThreads.Value;
		}


		void OnBuild(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eBuild, null);
		}


		void BindToSettings(NumericUpDown numThing)
		{
			numThing.DataBindings.Add(new Binding("Value",
				Settings.Default, numThing.Name, true,
				DataSourceUpdateMode.OnPropertyChanged));
		}
	}
}
