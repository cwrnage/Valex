using System.Collections.Generic;
using Newtonsoft.Json;

namespace Valex.Assets.Classes;

public class NezurUpdate
{
	[JsonProperty("version")]
	public string Version { get; set; }

	[JsonProperty("date")]
	public string Date { get; set; }

	[JsonProperty("added")]
	public List<string> Added { get; set; }

	[JsonProperty("changed")]
	public List<string> Changed { get; set; }

	[JsonProperty("removed")]
	public List<string> Removed { get; set; }

	[JsonProperty("stream")]
	public string Stream { get; set; }

	[JsonProperty("nezur_type")]
	public string NezurType { get; set; }

	[JsonProperty("id")]
	public string Id { get; set; }
}
