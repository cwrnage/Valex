using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class TabItemHandleWindow : Window, IComponentConnector
{
	private TabItem Tab;

	public TabItemHandleWindow(TabItem Item)
	{
		InitializeComponent();
		Tab = Item;
		if (Globals.tabControl.SelectedItem == Tab)
		{
			SplitB.IsEnabled = false;
			SplitB.Visibility = Visibility.Collapsed;
		}
		base.Deactivated += delegate
		{
			Hide();
		};
	}

	private async void ButtonHandler(object sender, RoutedEventArgs e)
	{
		string text = await ((WebViewAPI)((Grid)Tab.Content).Children[0]).GetText();
		switch (((Button)sender).Name)
		{
		case "SplitB":
			TabsAPI.AddSplitViewTab(Globals.tabControl.SelectedItem as TabItem, Tab);
			break;
		case "CloseB":
			Globals.tabControl.Items.Remove(Tab);
			break;
		case "DuplicateB":
		{
			string name = Tab.Header.ToString().Replace(".lua", "") + "_dup.lua";
			TabItem DuplicatedTab = TabsAPI.CreateNewTab(Tab.Style, name, text);
			File.WriteAllText("./scripts/" + name, text);
			Globals.tabControl.Items.Add(DuplicatedTab);
			break;
		}
		case "ExecuteB":
			Globals.webSocketServer.Execute(text);
			break;
		}
		Hide();
	}
}
