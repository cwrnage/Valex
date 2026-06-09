using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Valex.Assets.Classes;

public class WebSocketServer
{
	private readonly HttpListener _httpListener;

	private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();

	public static List<string> SelectedPids = new List<string>();

	public event Action<string> ClientConnected;

	public event Action<string> ClientDisconnected;

	public event Action<string, string> MessageReceived;

	public WebSocketServer(string prefix = "http://127.0.0.1:6969/")
	{
		_httpListener = new HttpListener();
		_httpListener.Prefixes.Add(prefix);
	}

	public void Inject(string pid)
	{
		if (GetAllPids().Length < 1)
		{
			MessageBox.Show("Roblox is not running! Please start Roblox before injecting Valex.", "Roblox not found");
		}
		else if (!GetConnectedPids().Contains(pid))
		{
			Process.Start(new ProcessStartInfo(Globals.startupPath + "\\bin\\valex.exe")
			{
				WorkingDirectory = Globals.startupPath + "./bin",
				CreateNoWindow = true,
				Arguments = pid
			});
		}
		else
		{
			MessageBox.Show("Valex is already injected to Roblox!");
		}
	}

	public async Task StartAsync(CancellationToken token = default(CancellationToken))
	{
		_httpListener.Start();
		Console.WriteLine("WebSocket Server running on ws://127.0.0.1:6969");
		while (!token.IsCancellationRequested)
		{
			HttpListenerContext context = await _httpListener.GetContextAsync();
			if (!context.Request.IsWebSocketRequest)
			{
				context.Response.StatusCode = 400;
			}
			else
			{
				HandleClientAsync(context);
			}
		}
	}

	private async Task HandleClientAsync(HttpListenerContext context)
	{
		string clientPid = context.Request.Headers["Client-PID"] ?? "Unknown";
		WebSocket socket = (await context.AcceptWebSocketAsync(null)).WebSocket;
		_clients[clientPid] = socket;
		this.ClientConnected?.Invoke(clientPid);
		MessageBox.Show("Client connected with PID: " + clientPid);
		try
		{
			byte[] buffer = new byte[1024];
			while (socket.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				if (result.MessageType != WebSocketMessageType.Close)
				{
					string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
					this.MessageReceived?.Invoke(clientPid, message);
					Console.WriteLine("Received from " + clientPid + ": " + message);
					continue;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			Console.WriteLine("Error with client " + clientPid + ": " + ex2.Message);
		}
		finally
		{
			_clients.TryRemove(clientPid, out var _);
			this.ClientDisconnected?.Invoke(clientPid);
			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
			Console.WriteLine("Client " + clientPid + " disconnected");
		}
	}

	private async Task SendToClientAsync(string pid, string message)
	{
		if (_clients.TryGetValue(pid, out var socket) && socket.State == WebSocketState.Open)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
			Console.WriteLine("Sent to " + pid + ": " + message);
		}
		else
		{
			Console.WriteLine("Client with PID " + pid + " not connected or socket closed.");
		}
	}

	public async void Execute(string script)
	{
		foreach (string pid in SelectedPids)
		{
			await SendToClientAsync(pid, script);
		}
	}

	public void Stop()
	{
		_httpListener.Stop();
		foreach (KeyValuePair<string, WebSocket> client in _clients)
		{
			client.Value.Abort();
		}
		_clients.Clear();
	}

	public string[] GetConnectedPids()
	{
		return _clients.Keys.ToArray();
	}

	public string[] GetAllPids()
	{
		return (from p in Process.GetProcessesByName("RobloxPlayerBeta")
			select p.Id.ToString()).ToArray();
	}
}
