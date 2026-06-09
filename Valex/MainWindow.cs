using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Microsoft.Win32;
using Valex.Assets.Classes;

namespace Valex;

public partial class MainWindow : Window, IComponentConnector, IStyleConnector
{
    private const uint WDA_NONE = 0u;

    private const uint WDA_EXCLUDEFROMCAPTURE = 17u;

    private int notification_counter = 0;

    private readonly ScriptBloxAPI scriptBloxAPI;

    private readonly DispatcherTimer dispatcherTimer;

    private AI ai = null;

    private bool initialised = false;

    private NotifyIcon ni = new NotifyIcon();

    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);

    public void PopulateScriptBox(string Search = "")
    {
        scriptsbox.Items.Clear();
        string[] files = Directory.GetFiles("scripts");
        foreach (string file in files)
        {
            string filename = Path.GetFileName(file);
            if (string.IsNullOrEmpty(Search) || filename.Contains(Search))
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = filename;
                listBoxItem.Padding = new Thickness(5.0, 5.0, 4.0, 5.0);
                listBoxItem.Style = TryFindResource("ListBoxItemStyle") as Style;
                ListBoxItem listBoxItem2 = listBoxItem;
                listBoxItem2.MouseLeftButtonUp += delegate
                {
                    tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, filename, System.IO.File.ReadAllText(file)));
                };
                listBoxItem2.MouseRightButtonUp += delegate
                {
                    ScriptListHandleWindow scriptListHandleWindow = new ScriptListHandleWindow(file);
                    scriptListHandleWindow.Left = System.Windows.Forms.Cursor.Position.X;
                    scriptListHandleWindow.Top = System.Windows.Forms.Cursor.Position.Y;
                    scriptListHandleWindow.Show();
                };
                scriptsbox.Items.Add(listBoxItem2);
            }
        }
    }

    public async void PopulateCloudScriptBox(string Search = "")
    {
        scriptspanel.Children.Clear();
        await scriptBloxAPI.SearchScript(Search);
        textBlock.Text = ScriptBloxAPI.Deserialized.result.scripts.Count + " scripts found";
        foreach (Valex.Assets.Classes.Script script in ScriptBloxAPI.Deserialized.result.scripts)
        {
            ScriptItem scriptItem = new ScriptItem(script.title, script.game.name, script.views.ToString(), script.likeCount.ToString(), script.script, Globals.webSocketServer)
            {
                Margin = new Thickness(5.0)
            };
            scriptspanel.Children.Add(scriptItem);
        }
    }

    public async void ReceiveOutput()
    {
        await Task.Run(async delegate
        {
            while (true)
            {
                using NamedPipeServerStream pipeServer = new NamedPipeServerStream("frenchtips921", PipeDirection.In);
                await pipeServer.WaitForConnectionAsync();
                using StreamReader reader = new StreamReader(pipeServer, Encoding.Default);
                string message = await reader.ReadToEndAsync();
                base.Dispatcher.Invoke(delegate
                {
                    UpdateConsole(message);
                });
            }
        });
    }

    public void BeginAutoInject()
    {
    }

    public void UpdateConsole(string Log)
    {
        notification_counter++;
        System.Windows.Controls.TextBox textBox = terminal;
        textBox.Text = textBox.Text + "[" + DateTime.Now.ToLongTimeString() + "]: " + Log + "\n";
        counter.Text = notification_counter.ToString();
        terminal.ScrollToEnd();
    }

    public void ClearConsole()
    {
        notification_counter = 0;
        counter.Text = notification_counter.ToString();
        terminal.Clear();
    }

    public void DefenderExclusion()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "Add-MpPreference -ExclusionPath \"" + Globals.startupPath + "\\Valex.exe\"",
            Verb = "runas",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        using Process process = new Process();
        process.StartInfo = startInfo;
        try
        {
            process.Start();
            string text = process.StandardOutput.ReadToEnd();
            string log = process.StandardError.ReadToEnd();
            process.WaitForExit();
            System.Windows.MessageBox.Show("Operation completed, check output.");
            if (string.IsNullOrEmpty(text))
            {
                UpdateConsole(log);
            }
            else
            {
                UpdateConsole(text);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
        }
    }

    public async void LoadChangelogs()
    {
        List<NezurUpdate> changelogs = await Valex.Assets.Classes.Changelogs.GetNezurUpdatesAsync();
        changelogs.Reverse();
        foreach (NezurUpdate changelog in changelogs)
        {
            string version = changelog.Version;
            string date = changelog.Date;
            string id = changelog.Id;
            string added = string.Join(Environment.NewLine, changelog.Added);
            ChangelogBorder changelogItem = new ChangelogBorder(version, added, date, "https://valex.io/?id=" + id)
            {
                Margin = new Thickness(0.0, 10.0, 0.0, 10.0)
            };
            changelogspanel.Children.Add(changelogItem);
            await Task.Delay(100);
        }
        Valex.Assets.Classes.Changelogs.Dispose();
    }

    public MainWindow()
    {
        InitializeComponent();
        Globals.webSocketServer = new WebSocketServer();
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher("scripts");
        scriptBloxAPI = new ScriptBloxAPI();
        dispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0),
            IsEnabled = true
        };
        Globals.tabControl = tabControl;
        Globals.tb = defaultb;
        dispatcherTimer.Tick += delegate
        {
            injlbl.Text = "Clients Connected: " + Globals.webSocketServer.GetConnectedPids().Length;
        };
        tabControl.SelectionChanged += delegate
        {
            if (tabControl.SelectedItem != null)
            {
                editorlabel.Content = ((TabItem)tabControl.SelectedItem).Header;
            }
        };
        searchdirbox.TextChanged += delegate
        {
            PopulateScriptBox(searchdirbox.Text);
        };
        searchscriptblox.KeyDown += delegate (object x, System.Windows.Input.KeyEventArgs y)
        {
            if (y.Key == Key.Return)
            {
                PopulateCloudScriptBox(searchscriptblox.Text);
            }
        };
        ConsoleCheckBox.Checked += delegate
        {
            Thickness margin = tabControl.Margin;
            border4.Height -= 68.0;
            tabControl.Margin = new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom - 68.0);
        };
        ConsoleCheckBox.Unchecked += delegate
        {
            Thickness margin = tabControl.Margin;
            border4.Height += 68.0;
            tabControl.Margin = new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom + 68.0);
        };
        fileSystemWatcher.Created += delegate
        {
            base.Dispatcher.Invoke(delegate
            {
                PopulateScriptBox(searchdirbox.Text);
            });
        };
        fileSystemWatcher.Deleted += delegate
        {
            base.Dispatcher.Invoke(delegate
            {
                PopulateScriptBox(searchdirbox.Text);
            });
        };
        fileSystemWatcher.Renamed += delegate
        {
            base.Dispatcher.Invoke(delegate
            {
                PopulateScriptBox(searchdirbox.Text);
            });
        };
        fileSystemWatcher.EnableRaisingEvents = true;
        UpdateConsole("Valex has been initalised.");
        base.Loaded += async delegate
        {
            days.Text = Globals.KeyAuthApp.expirydaysleft("days", 0) + "d";
            initialised = true;
            Globals.tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, "Untitled.lua"));
            await Task.Delay(1000);
            PopulateScriptBox();
            PopulateCloudScriptBox();
            ReceiveOutput();
            BeginAutoInject();
            LoadChangelogs();
            await Globals.webSocketServer.StartAsync();
            Globals.tb = defaultb;
            string read = System.IO.File.ReadAllText("bin//log");
            if (read.Split('%')[0] == "true")
            {
                preservelogs.IsChecked = true;
                terminal.Text = System.IO.File.ReadAllText(read.Split('%')[1]);
                notification_counter = terminal.Text.Split('\n').Length - 1;
                counter.Text = notification_counter.ToString();
            }
        };


        ni.DoubleClick += delegate
        {
            Show();
            base.WindowState = WindowState.Normal;
        };
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (base.WindowState == WindowState.Minimized && minimisetotray.IsChecked == true)
        {
            ni.Visible = true;
            Hide();
        }
        else
        {
            ni.Visible = false;
        }
        base.OnStateChanged(e);
    }

    private async void ButtonHandler(object sender, RoutedEventArgs e)
    {
        WebViewAPI monaco = null;
        WebViewAPI monaco2 = null;
        string name = ((FrameworkElement)sender).Name;
        if (initialised)
        {
            object selectedContent = tabControl.SelectedContent;
            Grid grid = selectedContent as Grid;
            if (grid != null && grid.Children.Count > 1)
            {
                monaco = grid.Children[0] as WebViewAPI;
                monaco2 = grid.Children[1] as WebViewAPI;
            }
            else
            {
                selectedContent = tabControl.SelectedContent;
                Grid grid2 = selectedContent as Grid;
                if (grid2 != null && grid2.Children.Count == 1)
                {
                    monaco = grid2.Children[0] as WebViewAPI;
                }
            }
        }
        if (name == "addtabbutton" || name == "nnew")
        {
            FileHandleWindow filemenu = new FileHandleWindow();
            filemenu.ShowDialog();
        }
        else if (monaco == null && initialised)
        {
            System.Windows.MessageBox.Show("No tab is selected or the editor is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            return;
        }
        switch (name)
        {
            case "exit":
                if ((!unsavedcontent.IsChecked).Value)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to exit? All unsaved content will be lost.", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (result != MessageBoxResult.Yes)
                    {
                        break;
                    }
                }
                try
                {
                    string x = Globals.logsPath + "./" + DateTime.Now.ToShortDateString().Replace("/", "") + ".txt";
                    if (preservelogs.IsChecked)
                    {
                        System.IO.File.WriteAllText("bin//log", "true%" + x);
                    }
                    else
                    {
                        System.IO.File.WriteAllText("bin//log", "false%" + x);
                    }
                    System.IO.File.WriteAllText(x, terminal.Text);
                    foreach (TabItem Item in (IEnumerable)tabControl.Items)
                    {
                        WebViewAPI itemMonaco = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                        if (itemMonaco != null)
                        {
                            string path = "./scripts/" + Item.Header;
                            System.IO.File.WriteAllText(path, await itemMonaco.GetText());
                        }
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("Failed to save scripts!", "Valex", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                Environment.Exit(0);
                break;
            case "exit_Copy":
                clientsTotal.Opacity = 0.0;
                clientsTotal.Visibility = Visibility.Collapsed;
                tabControl.Visibility = Visibility.Visible;
                break;
            case "minimise":
                base.WindowState = WindowState.Minimized;
                break;
            case "inject":
                {
                    tabControl.Visibility = Visibility.Collapsed;
                    clientsTotal.Opacity = 1.0;
                    clientsTotal.Visibility = Visibility.Visible;
                    clientpanel.Children.Clear();
                    string[] allPids = Globals.webSocketServer.GetAllPids();
                    foreach (string client in allPids)
                    {
                        pidBorder newclientitem = new pidBorder(client)
                        {
                            Margin = new Thickness(0.0, 0.0, 0.0, 13.0)
                        };
                        clientpanel.Children.Add(newclientitem);
                    }
                    break;
                }
            case "execute":
                {
                    WebSocketServer webSocketServer = Globals.webSocketServer;
                    webSocketServer.Execute(await monaco.GetText());
                    if (monaco2 != null)
                    {
                        WebSocketServer webSocketServer2 = Globals.webSocketServer;
                        webSocketServer2.Execute(await monaco2.GetText());
                    }
                    break;
                }
            case "clear":
            case "cclear":
                monaco.SetText("");
                monaco2?.SetText("");
                break;
            case "open":
            case "oopen":
                {
                    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                    if (dialog.ShowDialog() == true)
                    {
                        monaco.SetText(System.IO.File.ReadAllText(dialog.FileName));
                    }
                    break;
                }
            case "savetogit":
                {
                    if (!Globals.githubValidated)
                    {
                        System.Windows.MessageBox.Show("You must be logged in to GitHub to save. Please log in first.", "GitHub Login Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    }
                    string path2 = "./scripts/" + ((TabItem)tabControl.SelectedItem).Header;
                    System.IO.File.WriteAllText(path2, await monaco.GetText());
                    await Globals.gitHubAPI.UploadFile(Globals.startupPath + "scripts/" + ((TabItem)tabControl.SelectedItem).Header, ((TabItem)tabControl.SelectedItem).Header.ToString());
                    LoadGithubFiles();
                    break;
                }
            case "save":
            case "ssave":
                {
                    Microsoft.Win32.SaveFileDialog sdialog = new Microsoft.Win32.SaveFileDialog();
                    if (sdialog.ShowDialog() == true)
                    {
                        string fileName = sdialog.FileName;
                        System.IO.File.WriteAllText(fileName, FormatLuaCode(await monaco.GetText()));
                    }
                    break;
                }
            case "closetabbutton":
                {
                    System.Windows.Controls.Button closeButton = sender as System.Windows.Controls.Button;
                    TabItem tabItem = TabsAPI.FindParent<TabItem>(closeButton);
                    if (tabItem != null)
                    {
                        if (tabItem.Header == "Split View")
                        {
                            string header1 = tabItem.Name.Split('_')[0] + ".lua";
                            string header2 = tabItem.Name.Split('_')[1] + ".lua";
                            ItemCollection items = tabControl.Items;
                            Style tabStyle = TryFindResource("TabItemStyle") as Style;
                            string title = header1;
                            items.Add(TabsAPI.CreateNewTab(tabStyle, title, await monaco.GetText()));
                            ItemCollection items2 = tabControl.Items;
                            Style tabStyle2 = TryFindResource("TabItemStyle") as Style;
                            string title2 = header2;
                            items2.Add(TabsAPI.CreateNewTab(tabStyle2, title2, await monaco2.GetText()));
                            tabControl.Items.Remove(tabItem);
                        }
                        else
                        {
                            System.IO.File.WriteAllText(contents: (await monaco.GetText()) ?? "", path: "./scripts/" + tabItem.Header.ToString());
                            tabControl.Items.Remove(tabItem);
                        }
                    }
                    break;
                }
            case "openLink":
                keySystem.Visibility = Visibility.Collapsed;
                keySystem.Opacity = 0.0;
                Files.IsSelected = true;
                tabControl.Visibility = Visibility.Visible;
                break;
            case "enterKey":
                try
                {
                    ai = new AI(keyInput.Text);
                    Storyboard sb2 = (Storyboard)TryFindResource("KeysystemExit");
                    sb2.Begin();
                    await Task.Delay(1000);
                    tabControl.Visibility = Visibility.Visible;
                }
                catch
                {
                    System.Windows.MessageBox.Show("Invalid API Key!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    ai = null;
                    Storyboard sb3 = (Storyboard)TryFindResource("KeysystemExit");
                    sb3.Begin();
                    await Task.Delay(1000);
                    tabControl.Visibility = Visibility.Visible;
                    Files.IsSelected = true;
                }
                break;
            case "clearconsole":
                ClearConsole();
                break;
            case "saveconsole":
                System.IO.File.WriteAllText(Globals.logsPath + "./" + DateTime.Now.ToShortDateString().Replace("/", "") + ".txt", terminal.Text);
                System.Windows.MessageBox.Show("Saved to logs folder.");
                break;
            case "killrblx":
                {
                    Process[] processesByName = Process.GetProcessesByName("RobloxPlayerBeta");
                    foreach (Process Process in processesByName)
                    {
                        Process.Kill();
                    }
                    break;
                }
            case "excludedef":
                DefenderExclusion();
                break;
        }
    }

    private void CheckedHandler(object sender, RoutedEventArgs e)
    {
        switch (((System.Windows.Controls.CheckBox)sender).Name)
        {
            case "verified":
                ScriptBloxAPI.Sort["verified"] = "1";
                break;
            case "universal":
                ScriptBloxAPI.Sort["universal"] = "1";
                break;
            case "free":
                ScriptBloxAPI.Sort["free"] = "free";
                break;
            case "keysystem":
                ScriptBloxAPI.Sort["keysystem"] = "1";
                break;
        }
    }

    private void UncheckedHandler(object sender, RoutedEventArgs e)
    {
        switch (((System.Windows.Controls.CheckBox)sender).Name)
        {
            case "verified":
                ScriptBloxAPI.Sort["verified"] = "0";
                break;
            case "universal":
                ScriptBloxAPI.Sort["universal"] = "0";
                break;
            case "free":
                ScriptBloxAPI.Sort["free"] = "paid";
                break;
            case "keysystem":
                ScriptBloxAPI.Sort["keysystem"] = "0";
                break;
        }
    }

    private void MenuCheckHandler(object sender, RoutedEventArgs e)
    {
        switch (((System.Windows.Controls.RadioButton)sender).Name)
        {
            case "File":
                FileCM.IsOpen = true;
                File.IsChecked = false;
                break;
            case "Edit":
                EditCM.IsOpen = true;
                Edit.IsChecked = false;
                break;
            case "View":
                break;
            case "Consolee":
                ConsoleCM.IsOpen = true;
                Consolee.IsChecked = false;
                break;
        }
    }

    private void SettingsHandler(object sender, RoutedEventArgs e)
    {
        General.IsSelected = false;
        Module.IsSelected = false;
        Interface.IsSelected = false;
        switch (((System.Windows.Controls.RadioButton)sender).Name)
        {
            case "radioButton4":
                General.IsSelected = true;
                break;
            case "radioButton5":
                Interface.IsSelected = true;
                break;
            case "radioButton6":
                Module.IsSelected = true;
                break;
        }
    }

    private async void GeneralCheckedHandler(object sender, RoutedEventArgs e)
    {
        switch (((FrameworkElement)sender).Name)
        {
            case "autoinj":
                Globals.autoinj = true;
                break;
            case "autoexe":
                Globals.autoexec = true;
                break;
            case "hidevalex":
                {
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowDisplayAffinity(hwnd, 17u);
                    break;
                }
            case "topmost":
                base.Topmost = true;
                break;
            case "spoofdisks":
                Spoofer.SpoofDisks();
                UpdateConsole("Disks have been spoofed.");
                System.Windows.MessageBox.Show("Operation completed, check output.");
                break;
            case "spoofguids":
                Spoofer.SpoofGUIDs();
                UpdateConsole("GUIDs have been spoofed.");
                System.Windows.MessageBox.Show("Operation completed, check output.");
                break;
            case "spoofproductid":
                Spoofer.SpoofProductId();
                UpdateConsole("Windows ProductId spoofed.");
                System.Windows.MessageBox.Show("Operation completed, check output.");
                break;
            case "spoofmacaddress":
                if (Spoofer.SpoofMAC())
                {
                    UpdateConsole("MAC address have been spoofed.");
                }
                else
                {
                    UpdateConsole("Failed to spoof MAC addresses!");
                }
                System.Windows.MessageBox.Show("Operation completed, check output.");
                break;
            case "spoofmacaddress1":
                Spoofer.SpoofOwner();
                UpdateConsole("Windows RegisteredOwner spoofed.");
                System.Windows.MessageBox.Show("Operation completed, check output.");
                break;
            case "switchminimap":
                Globals.minimap = true;
                foreach (TabItem item5 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco8 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco8.ToggleMinimap(Globals.minimap);
                    if (item5.Name == "Split View")
                    {
                        WebViewAPI monaco9 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco9.ToggleMinimap(Globals.minimap);
                    }
                }
                break;
            case "switchminimapside":
                foreach (TabItem item4 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco6 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco6.ToggleMinimap(Globals.minimap, "left");
                    if (item4.Name == "Split View")
                    {
                        WebViewAPI monaco7 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco7.ToggleMinimap(Globals.minimap, "left");
                    }
                }
                break;
            case "smoothscroll":
                foreach (TabItem item3 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco4 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco4.ToggleSmoothScroll(enabled: true);
                    if (item3.Name == "Split View")
                    {
                        WebViewAPI monaco5 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco5.ToggleSmoothScroll(enabled: true);
                    }
                }
                break;
            case "switchwordwrap":
                foreach (TabItem item2 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco2 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco2.SwitchWordWrap("on");
                    if (item2.Name == "Split View")
                    {
                        WebViewAPI monaco3 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco3.SwitchWordWrap("on");
                    }
                }
                break;
            case "resize":
                base.ResizeMode = ResizeMode.NoResize;
                base.Height = 540.0;
                base.Width = 935.0;
                break;
            case "lingatureswitch":
                foreach (TabItem item in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco.SetFontLigatures(enabled: true);
                    if (item.Name == "Split View")
                    {
                        _ = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco.SetFontLigatures(enabled: true);
                    }
                }
                break;
            case "changelayout":
                sidepanel.Visibility = Visibility.Collapsed;
                toppanel.Visibility = Visibility.Visible;
                radioButton2_Copy_2.IsChecked = true;
                break;
            case "hidesidebar":
                toppanel.Opacity = 0.0;
                sidepanel.Opacity = 0.0;
                break;
            case "dealign":
                sidepanel.Margin = new Thickness(0.0, 333.0, 0.0, 0.0);
                break;
        }
    }

    private async void UncheckedGeneralHandler(object sender, RoutedEventArgs e)
    {
        switch (((System.Windows.Controls.CheckBox)sender).Name)
        {
            case "dealign":
                sidepanel.Margin = new Thickness(0.0, 54.0, 0.0, 0.0);
                break;
            case "changelayout":
                sidepanel.Visibility = Visibility.Visible;
                toppanel.Visibility = Visibility.Collapsed;
                break;
            case "hidesidebar":
                toppanel.Opacity = 1.0;
                sidepanel.Opacity = 1.0;
                break;
            case "autoinj":
                Globals.autoinj = false;
                break;
            case "autoexe":
                Globals.autoexec = false;
                break;
            case "hidevalex":
                {
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowDisplayAffinity(hwnd, 0u);
                    break;
                }
            case "topmost":
                base.Topmost = false;
                break;
            case "switchminimap":
                Globals.minimap = false;
                foreach (TabItem item5 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco8 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco8.ToggleMinimap(Globals.minimap);
                    if (item5.Name == "Split View")
                    {
                        WebViewAPI monaco9 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco9.ToggleMinimap(Globals.minimap);
                    }
                }
                break;
            case "switchwordwrap":
                foreach (TabItem item4 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco6 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco6.SwitchWordWrap("off");
                    if (item4.Name == "Split View")
                    {
                        WebViewAPI monaco7 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco7.SwitchWordWrap("off");
                    }
                }
                break;
            case "switchminimapside":
                foreach (TabItem item3 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco4 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco4.ToggleMinimap(Globals.minimap);
                    if (item3.Name == "Split View")
                    {
                        WebViewAPI monaco5 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco5.ToggleMinimap(Globals.minimap);
                    }
                }
                break;
            case "smoothscroll":
                foreach (TabItem item2 in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco2 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco2.ToggleSmoothScroll(enabled: false);
                    if (item2.Name == "Split View")
                    {
                        WebViewAPI monaco3 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco3.ToggleSmoothScroll(enabled: false);
                    }
                }
                break;
            case "resize":
                base.ResizeMode = ResizeMode.CanResize;
                break;
            case "lingatureswitch":
                foreach (TabItem item in (IEnumerable)tabControl.Items)
                {
                    WebViewAPI monaco = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
                    await monaco.SetFontLigatures(enabled: false);
                    if (item.Name == "Split View")
                    {
                        _ = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                        await monaco.SetFontLigatures(enabled: false);
                    }
                }
                break;
        }
    }

    private void radioButton_Click(object sender, RoutedEventArgs e)
    {
        Files.IsSelected = true;
    }

    private async void LoadGithubFiles()
    {
        githubbox.Items.Clear();
        foreach (string file in await Globals.gitHubAPI.GetRepositoryFileNames())
        {
            ListBoxItem newlistitem = new ListBoxItem
            {
                Content = file,
                Padding = new Thickness(5.0, 5.0, 4.0, 5.0),
                Style = (TryFindResource("ListBoxItemStyle") as Style)
            };
            newlistitem.MouseLeftButtonUp += async delegate
            {
                string result = await Globals.gitHubAPI.GetFileContent(file);
                tabControl.Items.Add(TabsAPI.CreateNewTab(TryFindResource("TabItemStyle") as Style, file, result));
            };
            githubbox.Items.Add(newlistitem);
        }
    }

    private void GithubHandler(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Globals.githubPAT))
        {
            new GithubWindow().ShowDialog();
            if (Globals.githubValidated)
            {
                LoadGithubFiles();
                Github.IsSelected = true;
            }
            else
            {
                System.Windows.MessageBox.Show("GitHub API validation failed. Please check your credentials.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                Files.IsSelected = true;
            }
        }
        else
        {
            LoadGithubFiles();
        }
    }

    private void OpacitySlider(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        base.Opacity = e.NewValue / 10.0;
    }

    public string FormatLuaCode(string luaCode)
    {
        SyntaxTree syntaxTree = LuaSyntaxTree.ParseText(luaCode);
        SyntaxNode syntaxNode = syntaxTree.GetRoot().NormalizeWhitespace();
        return syntaxNode.ToFullString();
    }

    private async void TabSizeSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        tabsizelabel.Content = "Tab Size: " + e.NewValue;
        foreach (TabItem item in (IEnumerable)tabControl.Items)
        {
            WebViewAPI monaco = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
            await monaco.SwitchTabSize((int)e.NewValue);
            if (item.Name == "Split View")
            {
                _ = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                await monaco.SwitchTabSize((int)e.NewValue);
            }
        }
    }

    private void ChatAIButton(object sender, RoutedEventArgs e)
    {
        if (ai == null)
        {
            keySystem.Visibility = Visibility.Visible;
            keySystem.Opacity = 1.0;
            tabControl.Visibility = Visibility.Collapsed;
        }
        else
        {
            ChatAI.IsSelected = true;
        }
    }

    public static List<(string Text, bool IsCode)> ParseMessage(string message)
    {
        List<(string, bool)> list = new List<(string, bool)>();
        Regex regex = new Regex("```(?:lua)?\\s*(.*?)\\s*```", RegexOptions.Singleline);
        int num = 0;
        foreach (Match item in regex.Matches(message))
        {
            if (item.Index > num)
            {
                string text = message.Substring(num, item.Index - num);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    list.Add((text.Trim(), false));
                }
            }
            string value = item.Groups[1].Value;
            list.Add((value, true));
            num = item.Index + item.Length;
        }
        if (num < message.Length)
        {
            string text2 = message.Substring(num);
            if (!string.IsNullOrWhiteSpace(text2))
            {
                list.Add((text2.Trim(), false));
            }
        }
        return list;
    }

    public void AppendMessage(string speaker, List<(string Text, bool IsCode)> segments)
    {
        aioutput.ScrollToEnd();
        Paragraph paragraph = new Paragraph
        {
            Margin = new Thickness(0.0)
        };
        Run item = new Run(speaker + ": ")
        {
            Foreground = ((speaker == "You") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(181, 141, 141)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 111, 123))),
            FontWeight = FontWeights.Bold
        };
        paragraph.Inlines.Add(item);
        foreach (var segment in segments)
        {
            Run run = new Run(segment.Text)
            {
                Foreground = (segment.IsCode ? System.Windows.Media.Brushes.LightGreen : System.Windows.Media.Brushes.White),
                FontFamily = (segment.IsCode ? new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Jetbrains Mono") : new System.Windows.Media.FontFamily("Segoe UI"))
            };
            if (segment.IsCode)
            {
                run.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
            }
            paragraph.Inlines.Add(run);
        }
        aioutput.Document.Blocks.Add(paragraph);
    }

    private async void inputbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Return)
        {
            return;
        }
        e.Handled = true;
        string prompt = inputbox.Text.Trim();
        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }
        string codeFromMonaco = "";
        Grid grid = default(Grid);
        int num;
        if (initialised)
        {
            object selectedContent = tabControl.SelectedContent;
            grid = selectedContent as Grid;
            num = ((grid != null) ? 1 : 0);
        }
        else
        {
            num = 0;
        }
        if (num != 0)
        {
            WebViewAPI monaco = default(WebViewAPI);
            int num2;
            if (grid.Children.Count > 0)
            {
                UIElement uIElement = grid.Children[0];
                monaco = uIElement as WebViewAPI;
                num2 = ((monaco != null) ? 1 : 0);
            }
            else
            {
                num2 = 0;
            }
            if (num2 != 0)
            {
                codeFromMonaco = await monaco.GetText();
            }
        }
        string combinedPrompt = "User Prompt: " + prompt + "\n\nCurrent Code:\n" + codeFromMonaco;
        AppendMessage("You", new List<(string, bool)> { (prompt, false) });
        inputbox.Clear();
        List<(string Text, bool IsCode)> segments = ParseMessage(await ai.AskAsync("Respond as a Roblox Lua assistant", combinedPrompt));
        AppendMessage("Valex", segments);
        AppendMessage("", new List<(string, bool)> { ("", false) });
    }

    private async void defaultb1_TextChanged(object sender, TextChangedEventArgs e)
    {
        foreach (TabItem item in (IEnumerable)tabControl.Items)
        {
            System.Windows.MessageBox.Show("asxc");
            WebViewAPI monaco = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
            monaco.Tokenize(((System.Windows.Controls.TextBox)sender).Text);
            if (item.Name == "Split View")
            {
                WebViewAPI monaco2 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                monaco2.Tokenize(((System.Windows.Controls.TextBox)sender).Text);
            }
        }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        using FontDialog fontDialog = new FontDialog();
        fontDialog.ShowColor = true;
        fontDialog.ShowEffects = false;
        if (fontDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }
        string fontFamily = fontDialog.Font.Name;
        int fontSize = (int)fontDialog.Font.Size;
        ColorTranslator.ToHtml(fontDialog.Color);
        foreach (TabItem item in (IEnumerable)tabControl.Items)
        {
            WebViewAPI monaco = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[0];
            await monaco.SetEditorStyle(fontSize, fontFamily);
            if (item.Name == "Split View")
            {
                WebViewAPI monaco2 = (WebViewAPI)((Grid)tabControl.SelectedContent).Children[1];
                await monaco2.SetEditorStyle(fontSize, fontFamily);
            }
        }
    }

    private void defaultb_TextChanged(object sender, TextChangedEventArgs e)
    {
    }

    private void radioButton1_2_Click(object sender, RoutedEventArgs e)
    {
    }

    private void panelenter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (hidesidebar.IsChecked.Value)
        {
            sidepanel.Opacity = 1.0;
            toppanel.Opacity = 1.0;
        }
    }

    private void panelleave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (hidesidebar.IsChecked.Value)
        {
            sidepanel.Opacity = 0.0;
            toppanel.Opacity = 0.0;
        }
    }

    private void sidepanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
    }
}
