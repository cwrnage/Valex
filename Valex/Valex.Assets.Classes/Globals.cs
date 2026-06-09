using System.Windows.Controls;
using Microsoft.Win32;

namespace Valex.Assets.Classes;

public static class Globals
{
	public static string startupPath;

	public static string workspacePath;

	public static string autoexecPath;

	public static string logsPath;

	public static RegistryKey registry = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Valex");

	public static bool autoexec = false;

	public static bool autoinj = false;

	public static bool minimap = false;

	public static api KeyAuthApp;

	public static string license;

	public static TabControl tabControl;

	public static WebSocketServer webSocketServer;

	public static bool githubValidated = false;

	public static string githubUsername;

	public static string githubPAT;

	public static GitHubAPI gitHubAPI;

	public static TextBox tb;

	public static string defaultcontent = "";
}
