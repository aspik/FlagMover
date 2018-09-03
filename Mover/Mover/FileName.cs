namespace FlagMover
{
  public class FileName
  {
    private FileName(string value)
    {
      Value = value;
    }

    public string Value { get; set; }

    public static FileName WatchedMovies { get { return new FileName("WatchedMovies.json"); } }

    public static FileName WatchedEpisodes { get { return new FileName("WatchedEpisodes.json"); } }
  }
}