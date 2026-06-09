using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class ScriptListHandleWindow : Window, IComponentConnector
{
	private string Path;

	public ScriptListHandleWindow(string Item)
	{
		InitializeComponent();
		Path = Item;
		base.Deactivated += delegate
		{
			Hide();
		};
	}

	private void ButtonHandler(object sender, RoutedEventArgs e)
	{
		string name = ((Button)sender).Name;
		string text = name;
		if (!(text == "ExecuteB"))
		{
			if (text == "DeleteB")
			{
				File.Delete(Path);
			}
		}
		else
		{
			Globals.webSocketServer.Execute(File.ReadAllText(Path));
		}
		Hide();
	}
}
