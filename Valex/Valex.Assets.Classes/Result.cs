using System.Collections.Generic;

namespace Valex.Assets.Classes;

public class Result
{
	public int totalPages { get; set; }

	public object nextPage { get; set; }

	public List<Script> scripts { get; set; }
}
