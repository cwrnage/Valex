using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Valex.Assets.Classes;

public class Changelogs
{
	private static readonly HttpClient httpClient = new HttpClient();

	private const string CHANGELOG_URL = "https://valex.io/logs/data/changelogs.json";

	public static async Task<List<NezurUpdate>> GetNezurUpdatesAsync()
	{
		try
		{
			List<NezurUpdate> allUpdates = JsonConvert.DeserializeObject<List<NezurUpdate>>(await httpClient.GetStringAsync("https://valex.io/logs/data/changelogs.json"));
			return allUpdates.Where((NezurUpdate update) => update.NezurType == "Nezur Executor").ToList();
		}
		catch (HttpRequestException ex)
		{
			HttpRequestException ex2 = ex;
			HttpRequestException ex3 = ex2;
			throw new InvalidOperationException("Failed to retrieve data from https://google.com", (Exception)(object)ex3);
		}
		catch (JsonException ex4)
		{
			JsonException ex5 = ex4;
			throw new InvalidOperationException("Invalid JSON format received from server", ex5);
		}
		catch (Exception ex6)
		{
			Exception ex7 = ex6;
			throw new InvalidOperationException("Error processing Nezur updates", ex7);
		}
	}

	public static void Dispose()
	{
		HttpClient obj = httpClient;
		if (obj != null)
		{
			((HttpMessageInvoker)obj).Dispose();
		}
	}
}
