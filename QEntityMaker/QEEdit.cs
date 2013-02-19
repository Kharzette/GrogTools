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

			internal bool	mbUsesSingleQuotes;
		}

		OpenFileDialog	mOFD	=new OpenFileDialog();

		string	mRenameEnd	="";

		const string	EntityFolder	="GrogLibs Entities.qtxfolder";
		const string	FormsFolder		="Entity forms.fctx";

		//hints
		const string	AngleHint		="Rotation about Y in degrees";
		const string	AnglesHint		="Specifies a facing direction in 3 dimensions, defined by pitch, yaw, and roll. (Default=0 0 0).";
		const string	DelayHint		="Specifies a delay in seconds before firing";
		const string	TargetHint		="Targetname of the entity to be triggered";
		const string	TargetNameHint	="Name of this entity for targeting by others";
		const string	MessageHint		="A string message to display";
		const string	MeshNameHint	="Name of the mesh to render, usually something.Static";


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


		BindingList<EntityKVP> ParseGuts(string file, TreeNode node)
		{
			//check for { }
			int	open	=file.IndexOf('{');
			int	close	=file.IndexOf('}');

			file	=file.Trim();

			BindingList<EntityKVP>	kvps	=new BindingList<EntityKVP>();

			while(true)
			{
				int	nextNewLine	=file.IndexOf('\n');

				string	gut	=file.Substring(0, nextNewLine);

				int	eqPosPreTrim	=gut.IndexOf('=');

				gut	=gut.Trim();

				int	eqPos	=gut.IndexOf('=');

				if(eqPos == -1 || eqPosPreTrim == (nextNewLine - 1) || gut.StartsWith("\""))
				{
					//watch for double lines
					if(gut.StartsWith("\""))
					{
						//trim crap and quotes
						if(gut.EndsWith("\""))
						{
							gut	=gut.Substring(1, gut.Length - 2);
						}
						else
						{
							gut	=gut.Substring(1, gut.Length - 1);
							gut	+="\"";
						}

						//tack onto end of previous value
						kvps[kvps.Count - 1].Value	+=gut;

						//advance
						file	=file.Substring(nextNewLine + 1);
						continue;
					}
					return	kvps;
				}

				string	key	=gut.Substring(0, eqPos - 1);

				key	=key.Trim();

				string	value	=gut.Substring(eqPos + 1);

				value	=value.Trim();

				EntityKVP	ekvp	=new EntityKVP();

				if(value.StartsWith("'") && value.EndsWith("'"))
				{
					ekvp.mbUsesSingleQuotes	=true;
				}

				//trim quotes
				value	=value.Substring(1, value.Length - 2);

				ekvp.Key	=key;
				ekvp.Value	=value;

				kvps.Add(ekvp);

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

					tn.Tag	=ParseGuts(file, tn);

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

				tn.Tag	=ParseGuts(file, tn);

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

			EntityTree.ExpandAll();

			EntityTree.TopNode	=EntityTree.Nodes[0];
		}


		bool IsInFolder(TreeNode tn, string folderName)
		{
			if(tn == null)
			{
				return	false;
			}

			if(tn.Text.StartsWith(folderName))
			{
				return	true;
			}

			if(tn.Parent == null)
			{
				return	false;
			}
			return	IsInFolder(tn.Parent, folderName);
		}


		void PopulateFieldGrid(TreeNode tn)
		{
			EntityFields.DataSource	=tn.Tag;
		}


		void OnAfterSelect(object sender, TreeViewEventArgs e)
		{
			AddGroupBox.Enabled		=false;
			DeleteEntity.Enabled	=false;
			RenameGroupBox.Enabled	=false;

			//see if this is an entity
			if(!IsInFolder(e.Node.Parent, EntityFolder))
			{
				//see if in forms folder
				if(!IsInFolder(e.Node.Parent, FormsFolder))
				{
					return;
				}
			}

			//make sure not a foldery thing
			if(e.Node.Text.Contains("*"))
			{
				AddGroupBox.Enabled		=true;
				return;
			}

			//check for entity subfield
			if(e.Node.Text.Contains(":e")
				|| e.Node.Text.Contains(":b"))
			{
				PopulateFieldGrid(e.Node);
				DeleteEntity.Enabled	=true;
				RenameGroupBox.Enabled	=true;
			}
			else if(e.Node.Text.Contains(":form")
				|| e.Node.Parent.Text.Contains(":form"))
			{
				//for editing of forms
				PopulateFieldGrid(e.Node);
			}
		}


		int SpaceOver(StreamWriter sw, int depth)
		{
			int	ret	=0;
			for(int i=0;i < depth;i++)
			{
				sw.Write("  ");
				ret	+=2;
			}
			return	ret;
		}


		//split long strings into chunks with the hex codes pulled out
		List<string>	SplitUpValue(string value)
		{
			List<string>	ret	=new List<string>();

			while(true)
			{
				int	hexPos	=value.IndexOf("\"$0D\"");
				if(hexPos == -1)
				{
					ret.Add(value);
					break;
				}

				string	part	=value.Substring(0, hexPos);

				ret.Add(part);

				string	hex	=value.Substring(hexPos, 5);

				ret.Add(hex);

				if(value.Length < (hexPos + 5))
				{
					break;
				}

				value	=value.Substring(hexPos + 5);
			}

			return	ret;
		}


		void WriteRecursive(StreamWriter sw, TreeNode tn, int depth)
		{
			SpaceOver(sw, depth - 1);

			sw.Write(tn.Text + "\n");

			SpaceOver(sw, depth - 1);

			sw.Write("{\n");

			if(tn.Tag != null)
			{
				BindingList<EntityKVP>	tagged	=tn.Tag as BindingList<EntityKVP>;

				foreach(EntityKVP ekvp in tagged)
				{
					int	width	=SpaceOver(sw, depth);

					//measure width, quark has problems with wide strings
					int	keyWidth	=width + ekvp.Key.Length + 4;

					if((keyWidth + ekvp.Value.Length) < 80)
					{
						if(!ekvp.mbUsesSingleQuotes)
						{
							sw.Write(ekvp.Key + " = \"" + ekvp.Value + "\"\n");
						}
						else
						{
							sw.Write(ekvp.Key + " = '" + ekvp.Value + "'\n");
						}
					}
					else
					{
						//write the key
						sw.Write(ekvp.Key + " = \"");

						//hex codes in the strings are a problem
						//quark likes 80 wide stuff, but if a hex code
						//occurs, it doesn't split them
						List<string>	chunks	=SplitUpValue(ekvp.Value);

						int	totalWidth	=keyWidth + (ekvp.Value.Length + 1);
						int	endPoint	=78 - keyWidth;

						//write chunks out to a width of 80
						for(int i=0;i < chunks.Count;i++)
						{
							string	chunk	=chunks[i];

							if((chunk.Length + keyWidth) < 80)
							{
								sw.Write(chunk);
								keyWidth	+=chunk.Length;
							}
							else
							{
								//break over onto another line
								//but if this chunk is a hex code,
								//don't split it
								if(chunk == "\"$0D\"")
								{
									//move to next line
									sw.Write("\"\n");

									int	beg	=SpaceOver(sw, depth);

									//additional space and quote
									sw.Write(" \"");
									beg	+=2;

									keyWidth	=beg;

									//back up one and iterate
									i--;
								}
								else
								{
									while(true)
									{
										if((chunk.Length + keyWidth) < 80)
										{
											sw.Write(chunk);
											keyWidth	+=chunk.Length;
											break;
										}
										string	chopped	=chunk.Substring(0, 78 - keyWidth);

										//write a split chunk
										sw.Write(chopped + "\"\n");

										int	beg	=SpaceOver(sw, depth);

										//additional space and quote
										sw.Write(" \"");
										beg	+=2;

										if(chunk.Length <= (78 - keyWidth))
										{
											break;
										}

										chunk	=chunk.Substring(78 - keyWidth);

										keyWidth	=beg;
									}
								}
							}
						}

						//write "\n
						sw.Write("\"\n");
					}
				}
			}

			foreach(TreeNode kid in tn.Nodes)
			{
				WriteRecursive(sw, kid, depth + 1);
			}

			SpaceOver(sw, depth - 1);

			sw.Write("}\n");

			return;
		}


		void OnSave(object sender, EventArgs e)
		{
			if(QuarkEntityFile.Text == null || QuarkEntityFile.Text == "")
			{
				return;
			}

			FileStream	fs	=new FileStream(QuarkEntityFile.Text, FileMode.Create, FileAccess.Write);
			if(fs == null)
			{
				EntityTree.Nodes.Add("Can't open " + QuarkEntityFile.Text);
				return;
			}

			EntityTree.Enabled		=false;
			EntityFields.Enabled	=false;

			StreamWriter	sw	=new StreamWriter(fs);

			WriteRecursive(sw, EntityTree.Nodes[0], 1);

			sw.Close();
			fs.Close();

			EntityTree.Enabled		=true;
			EntityFields.Enabled	=true;
		}


		void OnAddEntity(object sender, EventArgs e)
		{
			TreeNode	tn	=new TreeNode();

			TreeNode	mom	=EntityTree.SelectedNode;

			//grab the category text
			string	cat	=mom.Text;

			cat	=cat.ToLower();

			int	catIndex	=cat.IndexOf("_");

			cat	=cat.Substring(0, catIndex);
			cat	+="_new";

			tn.Text	=cat + ":e =";

			BindingList<EntityKVP>	kvps	=new BindingList<EntityKVP>();

			if(AutoAddOrigin.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		="origin";
				kvp.Value	="0 0 0";

				kvps.Add(kvp);
			}

			if(AutoAddMeshName.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		="meshname";
				kvp.Value	="Default.Static";

				kvps.Add(kvp);
			}

			if(AutoAddDesc.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		=";desc";
				kvp.Value	="An automatically generated description";

				kvps.Add(kvp);
			}

			tn.Tag	=kvps;

			if(AutoAddForm.Checked)
			{
				AddFormForEntity(tn, cat);
			}

			mom.Nodes.Add(tn);
		}


		TreeNode	FindNode(TreeNode tree, string name)
		{
			if(tree.Text.StartsWith(name))
			{
				return	tree;
			}

			foreach(TreeNode kid in tree.Nodes)
			{
				TreeNode	ret	=FindNode(kid, name);
				if(ret != null)
				{
					return	ret;
				}
			}
			return	null;
		}


		TreeNode CreateBasicFormNode(string title, string hint)
		{
			TreeNode	ret	=new TreeNode();

			ret.Text	=title + ": =";

			BindingList<EntityKVP>	stuff	=new BindingList<EntityKVP>();

			EntityKVP	usual	=new EntityKVP();

			usual.Key	="Txt";
			usual.Value	="&";
			stuff.Add(usual);

			usual	=new EntityKVP();

			usual.Key	="Hint";
			usual.Value	=hint;
			stuff.Add(usual);

			ret.Tag	=stuff;

			return	ret;
		}


		void AddFormForEntity(TreeNode tn, string entName)
		{
			TreeNode	forms	=FindNode(EntityTree.Nodes[0], FormsFolder);

			TreeNode	form	=new TreeNode();

			form.Text	=entName + ":form =";

			BindingList<EntityKVP>	formStuff	=new BindingList<EntityKVP>();

			//add the usual help and bbox
			EntityKVP	usual	=new EntityKVP();

			usual.Key	="Help";
			usual.Value	="A newly created Entity type.";
			formStuff.Add(usual);

			usual	=new EntityKVP();

			usual.mbUsesSingleQuotes	=true;
			usual.Key					="bbox";
			usual.Value					="-16 -16 -16 16 16 16";
			formStuff.Add(usual);

			form.Tag	=formStuff;

			foreach(EntityKVP kvp in tn.Tag as BindingList<EntityKVP>)
			{
				if(kvp.Key == "origin")
				{
					continue;
				}

				if(kvp.Key == "angle")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, AngleHint));
				}
				else if(kvp.Key == "angles")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, AnglesHint));
				}
				else if(kvp.Key == "delay")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, DelayHint));
				}
				else if(kvp.Key == "target")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, TargetHint));
				}
				else if(kvp.Key == "targetname")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, TargetNameHint));
				}
				else if(kvp.Key == "message")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, MessageHint));
				}
				else if(kvp.Key == "meshname")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, MeshNameHint));
				}
				else if(kvp.Key == ";desc")
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, kvp.Value));
				}
				else
				{
					form.Nodes.Add(CreateBasicFormNode(kvp.Key, "An unknown field of some sort"));
				}
			}

			forms.Nodes.Add(form);
		}


		void DeleteFormForEntity(TreeNode toNuke)
		{
			string	formName	=toNuke.Text;

			int	colonPos	=formName.IndexOf(':');

			formName	=formName.Substring(0, colonPos);
			formName	+=":form =";

			TreeNode	formNode	=FindNode(EntityTree.Nodes[0], formName);

			if(formNode != null)
			{
				formNode.Remove();
			}
		}


		void RenameFormForEntity(TreeNode toRename, string newName)
		{
			string	formName	=toRename.Text;

			int	colonPos	=formName.IndexOf(':');

			formName	=formName.Substring(0, colonPos);
			formName	+=":form =";

			TreeNode	formNode	=FindNode(EntityTree.Nodes[0], formName);

			if(formNode != null)
			{
				formNode.Text	=newName + ":form =";
			}
		}


		void OnDeleteEntity(object sender, EventArgs e)
		{
			TreeNode	toNuke	=EntityTree.SelectedNode;

			//nuke the form if it is there
			DeleteFormForEntity(toNuke);

			//remove from tree
			toNuke.Remove();
		}


		void OnRenameEntity(object sender, EventArgs e)
		{
			string	name	=EntityTree.SelectedNode.Text;

			//chop off the end stuff
			int	colonPos	=name.IndexOf(':');

			mRenameEnd	=name.Substring(colonPos);

			RenameBox.Text		=name.Substring(0, colonPos);
			RenameBox.Enabled	=true;

			RenameBox.Leave	+=OnRenameDone;

			RenameBox.Focus();
		}


		void OnRenameDone(object sender, EventArgs ea)
		{
			RenameDone(RenameBox.Text);
		}


		void RenameDone(string name)
		{
			RenameBox.Enabled	=false;

			RenameFormForEntity(EntityTree.SelectedNode, name);
			
			EntityTree.SelectedNode.Text	=name + mRenameEnd;

			RenameBox.Text	="";

			RenameBox.Leave	-=OnRenameDone;
		}


		void OnRenameBoxKey(object sender, KeyPressEventArgs e)
		{
			char	pressed	=e.KeyChar;

			if(pressed == '\r')
			{
				RenameDone(RenameBox.Text);
				e.Handled	=true;
			}
		}


		void OnTreeKeyUp(object sender, KeyEventArgs e)
		{
			if(!EntityTree.Focused)
			{
				return;
			}

			if(e.KeyCode == Keys.Delete)
			{
				OnDeleteEntity(null, null);
				e.Handled	=true;
			}
			else if(e.KeyCode == Keys.F2)
			{
				OnRenameEntity(null, null);
			}
		}
	}
}
