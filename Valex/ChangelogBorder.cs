using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Valex;

public partial class ChangelogBorder : UserControl, IComponentConnector
{
	public ChangelogBorder(string Title, string Changelogs, string Date, string ShareLink)
	{
		InitializeComponent();
		title.Content = Title;
		changelogstb.Text = Changelogs;
		date.Content = Date;
		changelogsbutton.Click += delegate
		{
			try
			{
				Clipboard.SetText(ShareLink);
				MessageBox.Show("Link copied to clipboard!", "Valex", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to copy link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		};
	}
}
