using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Kinect;
using UtilityLib;
using MeshLib;


namespace ColladaConvert
{
	public partial class KinectForm : Form
	{
		public class StupidGridRows
		{
			public string	BoneName	{ get; set; }

			public StupidGridRows(string sz)
			{
				BoneName	=sz;
			}
		}

		//file dialog
		OpenFileDialog	mOFD	=new OpenFileDialog();
		SaveFileDialog	mSFD	=new SaveFileDialog();

		MeshLib.AnimLib	mAnimLib;

		BindingList<KinectMap>		mKinectBoneData	=new BindingList<KinectMap>();
		BindingList<StupidGridRows>	mCharBoneData	=new BindingList<StupidGridRows>();

		public event EventHandler	eToggleRecord;
		public event EventHandler	eConvertToAnim;
		public event EventHandler	eSaveRawData;
		public event EventHandler	eLoadRawData;
		public event EventHandler	eTrimStart;
		public event EventHandler	eTrimEnd;


		public KinectForm(MeshLib.AnimLib anLib)
		{
			InitializeComponent();

			mAnimLib	=anLib;

			mKinectBoneData.Add(new KinectMap(JointType.AnkleLeft));
			mKinectBoneData.Add(new KinectMap(JointType.AnkleRight));
			mKinectBoneData.Add(new KinectMap(JointType.ElbowLeft));
			mKinectBoneData.Add(new KinectMap(JointType.ElbowRight));
			mKinectBoneData.Add(new KinectMap(JointType.FootLeft));
			mKinectBoneData.Add(new KinectMap(JointType.FootRight));
			mKinectBoneData.Add(new KinectMap(JointType.HandLeft));
			mKinectBoneData.Add(new KinectMap(JointType.HandRight));
			mKinectBoneData.Add(new KinectMap(JointType.Head));
			mKinectBoneData.Add(new KinectMap(JointType.HipCenter));
			mKinectBoneData.Add(new KinectMap(JointType.HipLeft));
			mKinectBoneData.Add(new KinectMap(JointType.HipRight));
			mKinectBoneData.Add(new KinectMap(JointType.KneeLeft));
			mKinectBoneData.Add(new KinectMap(JointType.KneeRight));
			mKinectBoneData.Add(new KinectMap(JointType.ShoulderCenter));
			mKinectBoneData.Add(new KinectMap(JointType.ShoulderLeft));
			mKinectBoneData.Add(new KinectMap(JointType.ShoulderRight));
			mKinectBoneData.Add(new KinectMap(JointType.Spine));
			mKinectBoneData.Add(new KinectMap(JointType.WristLeft));
			mKinectBoneData.Add(new KinectMap(JointType.WristRight));

			KinectBones.DataSource	=mKinectBoneData;
			CharBones.DataSource	=mCharBoneData;

			//adjust the widths after it has figured out the columns
			KinectBones.DataBindingComplete	+=OnDataBindingComplete;

			ColladaConvert.eAnimsUpdated	+=OnAnimsUpdated;
		}


		internal void UpdateCapturedDataStats(int numFrames, float totalTime)
		{
			NumFrames.Text	="" + numFrames;

			TotalTime.Text	=Convert.ToDecimal(totalTime).ToString(
				System.Globalization.CultureInfo.InvariantCulture);
		}


		internal void FillSkeleton(MeshLib.Skeleton skel)
		{
			List<string>	names	=new List<string>();

			skel.GetBoneNames(names);

			foreach(string name in names)
			{
				mCharBoneData.Add(new StupidGridRows(name));
			}
		}


		void OnAnimsUpdated(object sender, EventArgs e)
		{
			MeshLib.Skeleton	skel	=mAnimLib.GetSkeleton();

			if(skel != null)
			{
				FillSkeleton(skel);
			}
		}


		void OnDataBindingComplete(object sender, EventArgs ea)
		{
			KinectBones.Columns[0].Width	=80;
			KinectBones.Columns[1].Width	=34;
			KinectBones.Columns[2].Width	=34;
			KinectBones.Columns[3].Width	=34;
		}


		void OnAssignBone(object sender, EventArgs e)
		{
			if(CharBones.SelectedRows.Count <= 0)
			{
				return;
			}

			if(KinectBones.SelectedRows.Count <= 0)
			{
				return;
			}

			KinectBones.SelectedRows[0].Cells[4].Value	=
				CharBones.SelectedRows[0].Cells[0].Value;
		}


		void OnSaveMap(object sender, EventArgs e)
		{
			mSFD.DefaultExt		="*.kinmap";
			mSFD.Filter			="Kinect Map files (*.kinmap)|*.kinmap|All files (*.*)|*.*";
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			SaveMapData(mSFD.FileName);
		}


		void OnLoadMap(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.kinmap";
			mOFD.Filter			="Kinect Map files (*.kinmap)|*.kinmap|All files (*.*)|*.*";
			mOFD.Multiselect	=false;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			LoadMapData(mOFD.FileName);
		}


		void SaveMapData(string filePath)
		{
			FileStream	fs	=new FileStream(filePath, FileMode.Create, FileAccess.Write);
			if(fs == null)
			{
				return;
			}

			BinaryWriter	bw	=new BinaryWriter(fs);
			if(bw == null)
			{
				fs.Close();
				return;
			}

			//write an identifier
			UInt32	magic	=0xC1BEC700;
			bw.Write(magic);

			bw.Write(mKinectBoneData.Count);

			foreach(KinectMap km in mKinectBoneData)
			{
				km.Write(bw);
			}
		}


		void LoadMapData(string filePath)
		{
			FileStream	fs	=new FileStream(filePath, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				return;
			}

			BinaryReader	br	=new BinaryReader(fs);
			if(br == null)
			{
				fs.Close();
				return;
			}

			UInt32	magic	=br.ReadUInt32();
			if(magic != 0xC1BEC700)
			{
				br.Close();
				fs.Close();
				return;
			}

			mKinectBoneData.Clear();

			int	count	=br.ReadInt32();
			for(int i=0;i < count;i++)
			{
				KinectMap	km	=new KinectMap(br);

				mKinectBoneData.Add(km);
			}

			br.Close();
			fs.Close();
		}


		void OnToggleRecording(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eToggleRecord, null);
		}


		void OnSaveData(object sender, EventArgs e)
		{
			mSFD.DefaultExt		="*.kinmap";
			mSFD.Filter			="Kinect Data files (*.kindata)|*.kindata|All files (*.*)|*.*";
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Misc.SafeInvoke(eSaveRawData, mSFD.FileName);
		}

		
		void OnLoadData(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.kindata";
			mOFD.Filter			="Kinect Data files (*.kindata)|*.kindata|All files (*.*)|*.*";
			mOFD.Multiselect	=false;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Misc.SafeInvoke(eLoadRawData, mOFD.FileName);
		}

		
		void OnConvertToAnim(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eConvertToAnim, mKinectBoneData);
		}

		
		void OnTrimStart(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eTrimStart, new Nullable<Int32>((Int32)TrimAmount.Value));
		}

		
		void OnTrimEnd(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eTrimEnd, new Nullable<Int32>((Int32)TrimAmount.Value));
		}
	}
}
