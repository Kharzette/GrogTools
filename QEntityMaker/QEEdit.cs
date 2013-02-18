using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;


namespace QEntityMaker
{
	public partial class QEEdit : Form
	{
		//editable key value pair
		class EntityKVP
		{
			public string	Key		{ get; set; }
			public string	Value	{ get; set; }
		}

		OpenFileDialog	mOFD	=new OpenFileDialog();

		const string	EntityFolder	="GrogLibs Entities.qtxfolder";

		BindingList<EntityKVP>	mSelectedEntityFields
			=new BindingList<EntityKVP>();


		public QEEdit()
		{
			InitializeComponent();

			//add data bindings for positions of forms
			DataBindings.Add(new Binding("Location",
				global::QEntityMaker.Properties.Settings.Default,
				"QEEditFormPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			//text fields
			QuarkEntityFile.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "QuarkEntityFile", true,
				DataSourceUpdateMode.OnPropertyChanged));

			QuarkEntityFile.TextChanged	+=OnQEFTextChanged;
		}


		void OnBrowseForEntityFile(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.qrk";
			mOFD.Filter			="QuArK entity files (*.qrk)|*.qrk|All files (*.*)|*.*";
			mOFD.Multiselect	=false;

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			QuarkEntityFile.Text	=mOFD.FileName;
		}


		void OnQEFTextChanged(object sender, EventArgs ea)
		{
			RefreshTree();
		}


		string PreviousLine(string file, int pos)
		{
			int	braceNewLinePos	=file.LastIndexOf('\n', pos);
			int	titleNewLinePos	=file.LastIndexOf('\n', braceNewLinePos - 1);

			string	ret	=file.Substring(titleNewLinePos + 1, braceNewLinePos - titleNewLinePos - 1);

			ret	=ret.Trim();

			return	ret;
		}


		void ParseGuts(string file, TreeNode node)
		{
			//check for { }
			int	open	=file.IndexOf('{');
			int	close	=file.IndexOf('}');

			file	=file.Trim();

			while(true)
			{
				int	nextNewLine	=file.IndexOf('\n');

				string	gut	=file.Substring(0, nextNewLine);

				int	eqPos	=gut.IndexOf('=');

				if(eqPos == -1 || eqPos == (nextNewLine - 1))
				{
					return;
				}

				string	key	=gut.Substring(0, eqPos - 1);

				key	=key.Trim();

				string	value	=gut.Substring(eqPos + 1);

				value	=value.Trim();

				//trim quotes
				value	=value.Substring(1, value.Length - 2);

				TreeNode	kid	=new TreeNode();

				kid.Text		=key;
				kid.Tag			=value;
				kid.ToolTipText	=value;

				node.Nodes.Add(kid);

				file	=file.Substring(nextNewLine + 1);
			}
		}


		void ParseTreeRecursive(ref string file, TreeNode parent, int depth)
		{
			while(true)
			{
				//check for { }
				int	open	=file.IndexOf('{');
				int	close	=file.IndexOf('}');

				if(open == -1)
				{
					return;
				}

				if(open < close)
				{
					TreeNode	tn	=new TreeNode();

					tn.Text	=PreviousLine(file, open);

					file	=file.Substring(open + 1);

					ParseGuts(file, tn);

					parent.Nodes.Add(tn);

					ParseTreeRecursive(ref file, tn, depth + 1);
				}
				else
				{
					//skip past }
					file	=file.Substring(close + 1);
					return;
				}
			}
		}


		void ParseTree(string file)
		{
			//check for { }
			int	open	=file.IndexOf('{');
			int	close	=file.IndexOf('}');

			if(open == -1)
			{
				return;
			}

			if(open < close)
			{
				TreeNode	tn	=new TreeNode();

				tn.Text	=PreviousLine(file, open);

				file	=file.Substring(open + 1);

				EntityTree.Nodes.Add(tn);

				ParseTreeRecursive(ref file, tn, 1);
			}
			else
			{
				return;
			}
		}


		void RefreshTree()
		{
			if(QuarkEntityFile.Text == null || QuarkEntityFile.Text == "")
			{
				return;
			}

			EntityTree.Nodes.Clear();

			FileStream	fs	=new FileStream(QuarkEntityFile.Text, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				EntityTree.Nodes.Add("Can't open " + QuarkEntityFile.Text);
				return;
			}

			StreamReader	sr	=new StreamReader(fs);

			string	fileContents	=sr.ReadToEnd();

			sr.Close();
			fs.Close();

			ParseTree(fileContents);
		}


		bool IsInEntityFolder(TreeNode tn)
		{
			if(tn == null)
			{
				return	false;
			}

			if(tn.Text.StartsWith(EntityFolder))
			{
				return	true;
			}

			if(tn.Parent == null)
			{
				return	false;
			}
			return	IsInEntityFolder(tn.Parent);
		}


		void PopulateFieldGrid(TreeNode tn)
		{
			//start over
//			EntityFields.Rows.Clear();
//			EntityFields.Columns.Clear();

			EntityFields.DataSource	=null;

			mSelectedEntityFields.Clear();

			//fill in data
			foreach(TreeNode kid in tn.Nodes)
			{
				//skip tree nodes
				if(kid.Nodes.Count > 0)
				{
					continue;
				}

				EntityKVP	ekvp	=new EntityKVP();

				ekvp.Key	=kid.Text;
				ekvp.Value	=(string)kid.Tag;

				mSelectedEntityFields.Add(ekvp);
			}

			EntityFields.DataSource	=mSelectedEntityFields;
		}


		void OnAfterSelect(object sender, TreeViewEventArgs e)
		{
			//see if this is an entity
			if(!IsInEntityFolder(e.Node.Parent))
			{
				return;
			}

			//make sure not a foldery thing
			if(e.Node.Text.Contains("*"))
			{
				return;
			}

			//make sure not an entity subfield
			if(!e.Node.Text.Contains(":e"))
			{
				return;
			}

			PopulateFieldGrid(e.Node);
		}
	}
}
