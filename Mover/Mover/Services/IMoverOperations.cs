namespace FlagMover.Services
{
  public interface IMoverOperations
  {
    BackupMoviesResult BackupMovies(string path);
    BackupSeriesResult BackupSeries(string path);
    void RestoreWatchedMovies(string path);
    void RestoreWatchedSeries(string path);
  }
}