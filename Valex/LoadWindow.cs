using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class LoadWindow : Window, IComponentConnector
{
    public LoadWindow()
    {
        InitializeComponent();
    }

    private async void ButtonHandler(object sender, RoutedEventArgs e)
    {
        if (!(sender is Button { Name: var name }))
        {
            return;
        }
        switch (name)
        {
            case "exit":
                Environment.Exit(0);
                break;
            case "ContinueButton":

                MainWindow window = new MainWindow();
                window.Show();
                Hide();
                break;
            case "GetKeyButton":
                Process.Start("https://key.valex.io");
                break;
            case "discord":
                Process.Start("https://discord.gg/xv74bHYVb9");
                break;
        }
    }

    private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
