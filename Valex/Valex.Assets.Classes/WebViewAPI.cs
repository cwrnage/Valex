using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;

namespace Valex.Assets.Classes;

public class WebViewAPI : WebView2
{
	public delegate void TextUpdateHandler(string Type);

	private string ToSetText;

	private string LatestRecievedText;

	public bool isDOMLoaded { get; set; } = false;

	public event TextUpdateHandler OnTextUpdate;

	public event EventHandler EditorReady;

	public WebViewAPI(string Text = "")
	{
		base.Source = new Uri(Directory.GetCurrentDirectory() + "\\bin\\Monaco\\Monaco.html");
		base.CoreWebView2InitializationCompleted += WebViewAPI_CoreWebView2InitializationCompleted;
		ToSetText = Text;
	}

	protected virtual async void OnEditorReady()
	{
		this.EditorReady?.Invoke(this, new EventArgs());
		if (Globals.minimap)
		{
			await ToggleMinimap(enabled: true);
		}
	}

	private void WebViewAPI_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
	{
		base.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
		base.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
		base.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
		base.CoreWebView2.Settings.AreDevToolsEnabled = false;
	}

	private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
	{
		LatestRecievedText = e.TryGetWebMessageAsString();
		if (LatestRecievedText.Contains("TextUpdate"))
		{
			this.OnTextUpdate(JsonConvert.DeserializeObject<TextChangedObject>(LatestRecievedText).Data);
		}
	}

	private async void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
	{
		while (base.CoreWebView2 == null)
		{
			await Task.Delay(1000);
		}
		isDOMLoaded = true;
		OnEditorReady();
		await Task.Delay(1000);
		SetText(ToSetText);
		await ExecuteScriptAsync("\r\n            let InitialText = GetText();\r\n            let PreviousText = InitialText;\r\n            \r\n            setInterval(() => {\r\n                const CurrentText = GetText();\r\n                if (CurrentText !== PreviousText) {\r\n                    PreviousText = CurrentText;\r\n                    if (CurrentText !== InitialText) {\r\n                        //window.chrome.webview.postMessage('TextChanged');\r\n                        window.chrome.webview.postMessage(JSON.stringify({ Type: 'TextUpdate', Data: 'Change' }));\r\n                    } else {\r\n                        //window.chrome.webview.postMessage('TextReverted');\r\n                        window.chrome.webview.postMessage(JSON.stringify({ Type: 'TextUpdate', Data: 'Revert' }));\r\n                    }\r\n                }\r\n            }, 50);");
	}

	public async Task<string> GetText()
	{
		string Output = await base.CoreWebView2.ExecuteScriptAsync("GetText();");
		if (Output.Length > 0 && Output[0] == '"')
		{
			Output = Output.Substring(1, Output.Length - 2);
		}
		return Regex.Unescape(Output);
	}

	public async void SetText(string text)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SetText(\"" + HttpUtility.JavaScriptStringEncode(text) + "\")");
		}
	}

	public void AddIntellisense(string label, Types type, string description, string insert)
	{
		if (isDOMLoaded)
		{
			string text = type.ToString();
			if (type == Types.None)
			{
				text = "";
			}
			ExecuteScriptAsync("AddIntellisense(" + label + ", " + text + ", " + description + ", " + insert + ");");
		}
	}

	public void Refresh()
	{
		if (isDOMLoaded)
		{
			ExecuteScriptAsync("Refresh();");
		}
	}

	public async void Tokenize(string x)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("MaxTokenize(" + x + ")");
		}
	}

	public async void Lingatures(bool x)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("MaxTokenize(" + x + ")");
		}
	}

	public async Task ToggleMinimap(bool enabled, string side = "right")
	{
		if (isDOMLoaded)
		{
			Globals.minimap = enabled;
			if (side != "left" && side != "right")
			{
				side = "right";
			}
			await base.CoreWebView2.ExecuteScriptAsync("SwitchMinimap(" + enabled.ToString().ToLower() + ", '" + side + "')");
		}
	}

	public async Task ToggleSmoothScroll(bool enabled)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchSmoothScrolling(" + enabled.ToString().ToLower() + ")");
		}
	}

	public async Task SwitchWordWrap(string enabled)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchWordWrap('" + enabled + "')");
		}
	}

	public async Task SwitchTabSize(int amount)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync($"SwitchTabSize({amount})");
		}
	}

	public async Task SetReadOnly(bool readOnly)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchReadonly(" + readOnly.ToString().ToLower() + ")");
		}
	}

	public async Task SetRenderWhitespace(string mode)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchRenderWhitespace('" + mode + "')");
		}
	}

	public async Task SetLinks(bool enabled)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchLinks(" + enabled.ToString().ToLower() + ")");
		}
	}

	public async Task SetLineHeight(int lineHeight)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync($"SwitchLineHeight({lineHeight})");
		}
	}

	public async Task SetFontSize(int fontSize)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync($"SwitchFontSize({fontSize})");
		}
	}

	public async Task SetFolding(bool enabled)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchFolding(" + enabled.ToString().ToLower() + ")");
		}
	}

	public async Task SetAutoIndent(string mode)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchAutoIndent('" + mode + "')");
		}
	}

	public async Task SetFontFamily(string fontFamily)
	{
		if (isDOMLoaded)
		{
			string encodedFont = HttpUtility.JavaScriptStringEncode(fontFamily);
			await base.CoreWebView2.ExecuteScriptAsync("SwitchFontFamily('" + encodedFont + "')");
		}
	}

	public async Task SetFontLigatures(bool enabled)
	{
		if (isDOMLoaded)
		{
			await base.CoreWebView2.ExecuteScriptAsync("SwitchFontLigatures(" + enabled.ToString().ToLower() + ")");
		}
	}

	public async Task SetEditorStyle(int fontSize, string fontFamily)
	{
		await SetFontSize(fontSize);
		await SetFontFamily(fontFamily);
	}

	public async Task ConfigureEditor(bool readOnly = false, bool showMinimap = false, bool enableFolding = true)
	{
		await SetReadOnly(readOnly);
		await ToggleMinimap(showMinimap);
		await SetFolding(enableFolding);
	}
}
