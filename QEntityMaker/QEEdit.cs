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
	}
}