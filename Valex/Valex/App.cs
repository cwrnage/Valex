using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using Microsoft.Win32;
using Valex.Assets.Classes;

namespace Valex;

public partial class App : Application
{
	private static readonly string[] redistPaths = new string[2] { "SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\X64", "Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\X64" };

	public static bool VCRedistInstalled()
	{
		string[] array = redistPaths;
		foreach (string name in array)
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
			if (registryKey != null)
			{
				object value = registryKey.GetValue("Installed");
				return value != null && value.ToString() == "1";
			}
		}
		return false;
	}

	public static async void Update()
	{
		HttpClientHandler handler = new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => true
		};
		HttpClient httpClient = new HttpClient((HttpMessageHandler)(object)handler);
		try
		{
			string version = await (await httpClient.GetAsync("https://raw.githubusercontent.com/1Softworks/assets/refs/heads/main/intver.txt")).Content.ReadAsStringAsync();
			Globals.registry.SetValue("Version", version);
			HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/1Softworks/assets/refs/heads/main/valex.exe", (HttpCompletionOption)1);
			try
			{
				response.EnsureSuccessStatusCode();
				using Stream contentStream = await response.Content.ReadAsStreamAsync();
				FileStream fileStream = new FileStream(Globals.startupPath + "./bin/valex.exe", FileMode.Create, FileAccess.Write, FileShare.None);
				await contentStream.CopyToAsync(fileStream);
			}
			finally
			{
				((IDisposable)response)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)httpClient)?.Dispose();
		}
	}

	public App()
	{
        InitializeComponent();
        if (!VCRedistInstalled())
		{
			MessageBox.Show("VCRedistributables can't be found, please make sure to download them otherwise Valex won't work properly!", "Valex");
		}
		Globals.KeyAuthApp = new api("Valex", "2UWe8CI1m7", "1.0");
		Globals.startupPath = AppDomain.CurrentDomain.BaseDirectory;
		Globals.workspacePath = Path.Combine(Globals.startupPath, "workspace");
		Globals.autoexecPath = Path.Combine(Globals.startupPath, "autoexec");
		Globals.logsPath = Path.Combine(Globals.startupPath, "logs");
		Directory.CreateDirectory(Globals.workspacePath);
		Directory.CreateDirectory(Globals.autoexecPath);
		Directory.CreateDirectory(Globals.logsPath);
		if (File.Exists("./bin/github"))
		{
			string[] array = File.ReadAllText("./bin/github").Split('@');
			Globals.githubUsername = array[0];
			Globals.githubPAT = array[1];
			Globals.gitHubAPI = new GitHubAPI(Globals.githubPAT);
		}
		string value = Globals.workspacePath.Replace("\\", "\\\\");
		Globals.registry.SetValue("WorkspaceFolder", value);
		string value2 = Globals.autoexecPath.Replace("\\", "\\\\");
		Globals.registry.SetValue("AutoExecuteFolder", value2);
		object value3 = Globals.registry.GetValue("License");
		Globals.KeyAuthApp.init();
		base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
		/*
		if (value3 != null)
		{
			Globals.KeyAuthApp.license(value3.ToString());
			if (Globals.KeyAuthApp.response.success)
			{
				base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
				Globals.registry.SetValue("License", value3.ToString());
				Globals.license = value3.ToString();
			}
			else
			{
				base.StartupUri = new Uri("LoadWindow.xaml", UriKind.Relative);
			}
		}
		else
		{
			base.StartupUri = new Uri("LoadWindow.xaml", UriKind.Relative);
		}
		*/
	}
}
