using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Valex.Assets.Classes;

public static class TabsAPI
{
	public static TabItem CreateNewTab(Style TabStyle, string Title = "", string Content = "")
	{
		Grid grid = new Grid
		{
			Background = Brushes.Transparent
		};
		WebViewAPI webViewAPI = CreateNewEditor(Content);
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(1.0, GridUnitType.Star)
		});
		Grid.SetColumn(webViewAPI, 0);
		grid.Children.Add(webViewAPI);
		TabItem tab = new TabItem
		{
			Header = Title.Replace(" ", "_"),
			Content = grid,
			IsSelected = true
		};
		webViewAPI.OnTextUpdate += delegate(string Type)
		{
			_ = ((string)tab.Header) ?? "";
			if (Type == "Change")
			{
				if (!Title.EndsWith("*"))
				{
					tab.Header = Title + "*";
				}
			}
			else if (Type == "Revert" && Title.EndsWith("*"))
			{
				tab.Header = Title.TrimEnd('*');
			}
		};
		tab.MouseDown += delegate(object sender, MouseButtonEventArgs e)
		{
			if (e.RightButton == MouseButtonState.Pressed)
			{
				TabItemHandleWindow tabItemHandleWindow = new TabItemHandleWindow(tab);
				Mouse.GetPosition(System.Windows.Application.Current.MainWindow);
				tabItemHandleWindow.Left = System.Windows.Forms.Cursor.Position.X;
				tabItemHandleWindow.Top = System.Windows.Forms.Cursor.Position.Y;
				tabItemHandleWindow.Show();
			}
		};
		return tab;
	}

	public static void AddSplitViewTab(TabItem tab1, TabItem tab2)
	{
		Grid grid = tab1.Content as Grid;
		Grid grid2 = tab2.Content as Grid;
		WebViewAPI element = grid2.Children[0] as WebViewAPI;
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(1.0, GridUnitType.Star)
		});
		Grid.SetColumn(element, 1);
		grid2.Children.Remove(element);
		grid.Children.Add(element);
		TabItem newItem = new TabItem
		{
			Name = tab1.Header.ToString().Replace(".lua", "") + "_" + tab2.Header.ToString().Replace(".lua", ""),
			Header = "Split View",
			Content = grid,
			IsSelected = true
		};
		Globals.tabControl.Items.Add(newItem);
		Globals.tabControl.Items.Remove(tab1);
		Globals.tabControl.Items.Remove(tab2);
	}

	public static WebViewAPI CreateNewEditor(string Content)
	{
		return new WebViewAPI(Content);
	}

	public static T FindParent<T>(DependencyObject child) where T : DependencyObject
	{
		DependencyObject parent = VisualTreeHelper.GetParent(child);
		if (parent == null)
		{
			return null;
		}
		if (!(parent is T result))
		{
			return FindParent<T>(parent);
		}
		return result;
	}
}
