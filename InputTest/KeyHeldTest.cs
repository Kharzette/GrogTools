using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InputTest
{
	public partial class Form1 : Form
	{
		InputLib.Input	mInput	=new InputLib.Input();


		public Form1()
		{
			InitializeComponent();
		}

		void OnUpdate(object sender, EventArgs e)
		{
			mInput.Update();

			Dictionary<int, InputLib.Input.HeldKeyInfo>	stuff	=
				mInput.GetHeldKeyInfo();

			string	toPrint	="";
			foreach(KeyValuePair<int, InputLib.Input.HeldKeyInfo> key in stuff)
			{
				toPrint	+=key.Value.mKey + " : " + key.Value.mTimeHeld + "\r\n";
			}
			Action<TextBox>	ta	=con => con.AppendText(toPrint);
			FormExtensions.Invoke(InfoConsole, ta);
		}
	}

	public static class FormExtensions
	{
		public static void Invoke<T>(this T c, Action<T> doStuff)
			where T:System.Windows.Forms.Control
		{
			if(c.InvokeRequired)
			{
				c.Invoke((EventHandler) delegate { doStuff(c); } );
			}
			else
			{
				doStuff(c);
			}
		}
	}
}
