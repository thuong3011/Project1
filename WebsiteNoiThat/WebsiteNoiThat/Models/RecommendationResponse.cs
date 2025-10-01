using Newtonsoft.Json;
using System.Collections.Generic;

namespace Models
{
	public class RecommendationResponse
	{
		[JsonProperty("recommendations")]
		public List<string> Recommendations { get; set; }
	}
}
