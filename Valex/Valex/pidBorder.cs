using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class pidBorder : UserControl, IComponentConnector
{
	private string pid;

	public pidBorder(string pid)
	{
		InitializeComponent();
		this.pid = pid;
		pidLabel.Content = "PID: " + pid;
		if (WebSocketServer.SelectedPids.Contains(pid))
		{
			check.IsChecked = true;
		}
	}

	private void CheckBox_Checked(object sender, RoutedEventArgs e)
	{
		if (Globals.webSocketServer.GetConnectedPids().Contains(pid))
		{
			WebSocketServer.SelectedPids.Add(pid);
			return;
		}
		Globals.webSocketServer.Inject(pid);
		WebSocketServer.SelectedPids.Add(pid);
	}

	private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
	{
		if (Globals.webSocketServer.GetConnectedPids().Contains(pid))
		{
			WebSocketServer.SelectedPids.Remove(pid);
		}
	}
}
