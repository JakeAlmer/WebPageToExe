using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ionic.Zip;

namespace Viewer
{
	public partial class Form1 : Form
	{
		private readonly DirectoryInfo _tempDir;

		public Form1()
		{
			InitializeComponent();
			_tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "Viewer_"+Guid.NewGuid().ToString()));
			_tempDir.Create();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
            using (Stream theResource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Viewer.Files.zip"))
            using (ZipFile zip = ZipFile.Read(theResource))
                zip.ExtractAll(_tempDir.FullName);
			webBrowser1.Navigate(Path.Combine(_tempDir.FullName, "index.html"));
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				_tempDir.Delete(true);
			}catch
			{
			}
		}

		private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			try
			{
				this.Text = webBrowser1.Document.Title;
			}
			catch
			{
			}
		}
	}
}
