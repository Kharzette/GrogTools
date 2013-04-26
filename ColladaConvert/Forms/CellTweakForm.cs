using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ColladaConvert
{
	public partial class CellTweakForm : Form
	{
		internal class CellThreshLevel
		{
			float	mThreshold;
			float	mLevel;

			public float Threshold
			{
				get { return mThreshold; }
				set { mThreshold = value; }
			}

			public float Level
			{
				get { return mLevel; }
				set { mLevel = value; }
			}
		}

		BindingList<CellThreshLevel>	mCellValues	=new BindingList<CellThreshLevel>();

		MaterialLib.MaterialLib	mMats;
		GraphicsDevice			mGD;


		public CellTweakForm(GraphicsDevice gd, MaterialLib.MaterialLib mats)
		{
			InitializeComponent();

			mGD		=gd;
			mMats	=mats;

			CellTweakGrid.DataSource	=mCellValues;
		}


		void OnApplyShading(object sender, EventArgs e)
		{
			int	numLevels	=mCellValues.Count;

			float	[]thresholds	=new float[numLevels - 1];
			float	[]levels		=new float[numLevels];

			for(int i=0;i < (numLevels - 1);i++)
			{
				thresholds[i]	=mCellValues[i].Threshold;
				levels[i]		=mCellValues[i].Level;
			}
			levels[numLevels - 1]	=mCellValues[numLevels - 1].Level;

			mMats.GenerateCellTexture(mGD, 0, 16, thresholds, levels);
			mMats.SetCellTexture(0);
		}
	}
}
