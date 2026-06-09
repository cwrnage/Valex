using System;
using System.Collections.Generic;

namespace Valex.Assets.Classes;

public class Script
{
	public string _id { get; set; }

	public string title { get; set; }

	public Game game { get; set; }

	public string slug { get; set; }

	public bool verified { get; set; }

	public int views { get; set; }

	public string scriptType { get; set; }

	public bool isUniversal { get; set; }

	public bool isPatched { get; set; }

	public DateTime createdAt { get; set; }

	public bool key { get; set; }

	public int dislikeCount { get; set; }

	public int likeCount { get; set; }

	public string image { get; set; }

	public DateTime lastBump { get; set; }

	public string script { get; set; }

	public List<string> matched { get; set; }
}
