using System;
using Newtonsoft.Json;

namespace Mover.Entities
{
  public class MediaLibraryMovie
  {
    [JsonProperty(PropertyName = "imdb")]
    public string Imdb { get; set; }

    [JsonProperty(PropertyName = "tmdb")]
    public uint? Tmdb { get; set; }

    [JsonProperty(PropertyName = "title")]
    public string Title { get; set; }

    [JsonProperty(PropertyName = "year")]
    public int? Year { get; set; }
  }
}