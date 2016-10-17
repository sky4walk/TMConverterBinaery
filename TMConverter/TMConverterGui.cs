// André Betz
// http://www.andrebetz.de
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TM2Train;

namespace TMConverter
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(8, 8);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(200, 72);
			this.button1.TabIndex = 0;
			this.button1.Text = "Convert";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(218, 95);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "TMConverter www.AndreBetz.de";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog opfd = new OpenFileDialog();
			opfd.Filter = "Turing Machine Description (*.tm)|*.tm" ;
			opfd.FilterIndex = 1 ;
			opfd.InitialDirectory = Application.StartupPath;
			opfd.RestoreDirectory = false ;
			opfd.Title = "Load Turing Machine";
			DialogResult res = opfd.ShowDialog(this);
			if(res==DialogResult.OK)
			{
				string TMName = opfd.FileName;
				string TMNameNew = Path.GetFileNameWithoutExtension(TMName) + "_(1Bit).tm";
				TMConvert1Bit conv = new TMConvert1Bit(TMName);
				string TMNew = conv.Convert();
				if(TMNew!=null)
				{
					Speicher(TMNew,TMNameNew);
				}
			}
		}

		private bool Speicher(string Datei,string DateiName)
		{
			BinaryWriter br = null;
			try
			{
				if(File.Exists(DateiName))
				{
					File.Delete(DateiName);
				}
				FileStream fs = new FileStream(DateiName,FileMode.CreateNew,FileAccess.Write);					
				br = new BinaryWriter(fs);
				br.Write(Datei.ToCharArray());
				br.Flush();
			}
			catch(Exception)
			{
				return false;
			}
			finally
			{
				if(br!=null)
				{
					br.Close();
				}
			}
			return true;
		}
	}
}
