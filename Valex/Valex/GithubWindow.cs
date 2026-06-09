using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class GithubWindow : Window, IComponentConnector
{
	public GithubWindow()
	{
		InitializeComponent();
	}

	private async void ButtonHandler(object sender, RoutedEventArgs e)
	{
		if (((Button)sender).Name == "Save")
		{
			if (string.IsNullOrWhiteSpace(userbox.Text) || string.IsNullOrWhiteSpace(patbox.Text))
			{
				MessageBox.Show("Username or PAT cannot be empty!", "Incorrect details!");
				return;
			}
			Globals.gitHubAPI = new GitHubAPI(patbox.Text);
			try
			{
				MessageBox.Show("New repository created: " + await Globals.gitHubAPI.CreateRepository("Created by Valex via GitHubAPI"));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Incorrect details!");
				Close();
				return;
			}
			Globals.githubUsername = userbox.Text;
			Globals.githubPAT = patbox.Text;
			File.WriteAllText("./bin/github", userbox.Text + "@" + patbox.Text);
		}
		Close();
	}
}
