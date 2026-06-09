using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace Valex.Assets.Classes;

public class AI
{
	public enum PresetAction
	{
		FixCode,
		BeautifyCode,
		OptimizeCode,
		ExplainCode
	}

	private readonly ChatClient _client;

	private readonly string _model;

	private readonly Dictionary<PresetAction, string> _presetPrompts = new Dictionary<PresetAction, string>
	{
		{
			PresetAction.FixCode,
			"You are a Roblox Lua developer. Fix the following Lua code so it works correctly."
		},
		{
			PresetAction.BeautifyCode,
			"You are a Roblox Lua developer. Beautify and format this Lua code properly."
		},
		{
			PresetAction.OptimizeCode,
			"You are a Roblox Lua developer. Optimize this Lua code for performance without changing functionality."
		},
		{
			PresetAction.ExplainCode,
			"You are a Roblox Lua developer. Explain what the following Lua code does, step by step."
		}
	};

	public AI(string apiKey, string model = "gpt-4")
	{
		_model = model;
		_client = new ChatClient(model, apiKey);
	}

	public async Task<string> RunPresetAsync(PresetAction action, string code)
	{
		if (!_presetPrompts.ContainsKey(action))
		{
			throw new ArgumentException("Invalid preset action.");
		}
		string prompt = _presetPrompts[action] + "\n\nCode:\n" + code;
		try
		{
			ClientResult<ChatCompletion> completion = await _client.CompleteChatAsync(prompt);
			string raw = ((completion.Value.Content.Count > 0) ? completion.Value.Content[0].Text : "(no response)");
			return StripMarkdownCodeFences(raw);
		}
		catch (Exception ex)
		{
			return "Error: " + ex.Message;
		}
	}

	public async Task<string> AskAsync(string instruction, string code)
	{
		if (string.IsNullOrWhiteSpace(instruction))
		{
			throw new ArgumentException("Instruction cannot be empty.");
		}
		try
		{
			ClientResult<ChatCompletion> completion = await _client.CompleteChatAsync(instruction + Environment.NewLine + code);
			string raw = ((completion.Value.Content.Count > 0) ? completion.Value.Content[0].Text : "(no response)");
			return StripMarkdownCodeFences(raw);
		}
		catch (Exception ex)
		{
			return "Error: " + ex.Message;
		}
	}

	private string StripMarkdownCodeFences(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		return text.Trim();
	}
}
