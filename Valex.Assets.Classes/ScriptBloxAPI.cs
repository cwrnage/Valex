using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Valex.Assets.Classes;

public class ScriptBloxAPI
{
	public static Root Deserialized;

	private readonly HttpClient _httpClient;

	private readonly string baseUrl = "https://scriptblox.com/api/script/";

	public static Dictionary<string, string> Sort = new Dictionary<string, string>
	{
		{ "verified", "0" },
		{ "universal", "0" },
		{ "free", "paid" },
		{ "keysystem", "0" },
		{ "views", "0" },
		{ "likes", "0" },
		{ "recent", "0" }
	};

	public ScriptBloxAPI()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		_httpClient = new HttpClient();
	}

	public async Task SearchScript(string Search = "", int Page = 1)
	{
		string Query = ((!string.IsNullOrEmpty(Search)) ? (baseUrl + string.Format("search?q={0}&page={1}&verified={2}&universal={3}&mode={4}&patched=0&key={5}&max=18", Search, Page, Sort["verified"], Sort["universal"], Sort["free"], Sort["keysystem"])) : (baseUrl + "fetch?page=1&max=18"));
		Deserialized = JsonConvert.DeserializeObject<Root>(await (await _httpClient.GetAsync(Query)).Content.ReadAsStringAsync());
	}
}
