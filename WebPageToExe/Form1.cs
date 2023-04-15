using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Build.BuildEngine;

namespace WebPageToExe
{
	public partial class Form1 : Form
	{
		private const string ViewerProjectResourceLocation = "WebPageToExe.ViewerProject.";

		public Form1()
		{
			InitializeComponent();
			source.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WebSite");
			output.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WebSite.exe");
		}

		private void BrowseToFile(TextBox textBox)
		{
			OpenFileDialog fb = new OpenFileDialog();
			fb.FileName = textBox.Text;
			fb.InitialDirectory = new FileInfo(textBox.Text).DirectoryName;
			if (fb.ShowDialog() == DialogResult.OK)
			{
				textBox.Text = fb.FileName;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			BrowseToDirectory(source);
		}

		private void BrowseToDirectory(TextBox textBox)
		{
			FolderBrowserDialog fb = new FolderBrowserDialog();
			fb.SelectedPath = textBox.Text;
		    if (fb.ShowDialog() == DialogResult.OK)
		        textBox.Text = fb.SelectedPath;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			BrowseToFile(output);
		}

		private void generateButton_Click(object sender, EventArgs e)
		{
			if(new DirectoryInfo(source.Text).GetFiles("index.html").Length==0)
			{
				MessageBox.Show("Make sure there's a file named index.html in your source directory.");
				return;;
			}

			var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WebPageToExe_" + Guid.NewGuid().ToString()));
			tempDir.Create();

		    generateButton.Enabled = false;
		    generateButton.Text = "Working...";
			ExtractViewerSource(tempDir);
			UpdateViewerFilesZip(tempDir);
			BuildViewerProject(tempDir);
            generateButton.Enabled = true;
            generateButton.Text = "Generate Executable";

			try
			{
				tempDir.Delete(true);
			}catch{}
		}

		private void BuildViewerProject(DirectoryInfo tempDir)
		{
		    Environment.SetEnvironmentVariable("MSBuildToolsPath", System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
			Environment.SetEnvironmentVariable("Platform", "AnyCPU");
            Engine engine = new Engine();
            engine.BinPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

            // Instantiate a new FileLogger to generate build log
            FileLogger logger = new FileLogger();

            // Set the logfile parameter to indicate the log destination
            string logFile = Path.Combine(Path.GetTempPath(), "build.log");
		    logger.Parameters = @"logfile=" + logFile;

            // Register the logger with the engine
            engine.RegisterLogger(logger);

            // Build a project file
			bool success = engine.BuildProjectFile(Path.Combine(tempDir.FullName, "Viewer.csproj"));

            //Unregister all loggers to close the log file
            engine.UnregisterAllLoggers();
            engine.UnloadAllProjects();

            if (!success)
            {
                try
                {
                    MessageBox.Show(string.Format(@"Log file: 
{0}

Error:
{1}", logFile, File.ReadAllText(logFile)));
                }
                catch { }
            }
            else
            {
                File.Copy(Path.Combine(tempDir.FullName, "bin/debug/viewer.exe"), output.Text);
                MessageBox.Show("Boom! It's done.");
            }
		}

		private void UpdateViewerFilesZip(DirectoryInfo tempDir)
		{
		    if (File.Exists("Files.zip")) File.Delete("Files.zip");



		    using (var zipFile = new ZipFile(Path.Combine(tempDir.FullName, "Files.zip")))
		    {
		        var sourceDir = new DirectoryInfo(source.Text);
		        foreach (FileInfo file in sourceDir.GetFiles())
		            zipFile.AddFile(file.FullName, "");
		        foreach (DirectoryInfo directory in sourceDir.GetDirectories())
		            zipFile.AddDirectory(directory.FullName, directory.Name);
		        zipFile.Save();
		    }
		}

	    private void ExtractViewerSource(DirectoryInfo tempDir)
	    {
	        using (Stream theResource = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebPageToExe.Viewer.zip"))
	        using (ZipFile zip = ZipFile.Read(theResource))
	            zip.ExtractAll(tempDir.FullName);
	    }
	}
}
