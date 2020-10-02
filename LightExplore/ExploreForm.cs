using System;
using System.Windows.Forms;
using BSPCore;
using UtilityLib;


namespace LightExplore
{
	public partial class ExploreForm : Form
	{
		OpenFileDialog	mOFD	=new OpenFileDialog();
		
		public event EventHandler	eOpenGBSP;


		public ExploreForm()
		{
			InitializeComponent();
		}
		
		
		void OnOpenGBSP(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			mOFD.Filter		="GBSP files (*.gbsp)|*.gbsp|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			CoreEvents.Print("Exploring gbsp " + mOFD.FileName + "\n");

			Misc.SafeInvoke(eOpenGBSP, mOFD.FileName);

			GBSPFileName.Text	=FileUtil.StripPath(mOFD.FileName);
		}
	}
}
