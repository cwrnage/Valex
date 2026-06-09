using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class FileHandleWindow : Window, IComponentConnector
{
	public FileHandleWindow()
	{
		InitializeComponent();
		if (FindName("GithubSave") is Button button)
		{
			button.IsEnabled = Globals.githubValidated;
		}
	}

	private void ButtonHandler(object sender, RoutedEventArgs e)
	{
		switch (((Button)sender).Name)
		{
		case "Cancel":
			Hide();
			break;
		case "GithubSave":
		{
			if (!Globals.githubValidated)
			{
				MessageBox.Show("You must be logged in to GitHub to save. Please log in first.", "GitHub Login Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				break;
			}
			string text2 = TextBox.Text;
			if (string.IsNullOrEmpty(text2))
			{
				MessageBox.Show("Name cannot be empty!");
				break;
			}
			if (File.Exists("./scripts/" + text2 + ".lua"))
			{
				switch (MessageBox.Show("The file " + text2 + ".lua already exists. Do you want to replace it with this new save?", "File Already Exists", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
				{
				case MessageBoxResult.No:
					Hide();
					return;
				case MessageBoxResult.Yes:
					File.WriteAllText("./scripts/" + text2 + ".lua", "");
					Globals.tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, text2 + ".lua", Globals.tb.Text));
					Globals.gitHubAPI.UploadFile(Globals.startupPath + "scripts/" + text2 + ".lua", text2 + ".lua");
					Hide();
					return;
				}
			}
			File.WriteAllText("./scripts/" + text2 + ".lua", "");
			Globals.tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, text2 + ".lua", Globals.tb.Text));
			Globals.gitHubAPI.UploadFile(Globals.startupPath + "scripts/" + text2 + ".lua", text2 + ".lua");
			Hide();
			break;
		}
		case "Save":
		{
			string text = TextBox.Text;
			if (string.IsNullOrEmpty(text))
			{
				MessageBox.Show("Name cannot be empty!");
				break;
			}
			if (File.Exists("./scripts/" + text + ".lua"))
			{
				switch (MessageBox.Show("The file " + text + ".lua already exists. Do you want to replace it with this new save?", "File Alredy Exists", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
				{
				case MessageBoxResult.No:
					Hide();
					return;
				case MessageBoxResult.Yes:
					File.WriteAllText("./scripts/" + text + ".lua", "");
					Globals.tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, text + ".lua", Globals.tb.Text));
					Hide();
					return;
				}
			}
			File.WriteAllText("./scripts/" + text + ".lua", "");
			Globals.tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, text + ".lua", Globals.tb.Text));
			Hide();
			break;
		}
		}
	}
}
