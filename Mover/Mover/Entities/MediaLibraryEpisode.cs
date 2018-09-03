using Newtonsoft.Json;

namespace FlagMover.Entities
{
  public class MediaLibraryEpisode
  {
    [JsonProperty(PropertyName = "show_imdb")]
    public string ShowImdb { get; set; }

    [JsonProperty(PropertyName = "show_tvdb")]
    public uint? ShowTvdb { get; set; }

    [JsonProperty(PropertyName = "show_title")]
    public string ShowTitle { get; set; }

    [JsonProperty(PropertyName = "season")]
    public int? Season { get; set; }

    [JsonProperty(PropertyName = "number")]
    public int? Number { get; set; }
  }
}