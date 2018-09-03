namespace FlagMover.Services
{
  public interface IMoverOperations
  {
    void BackupMovies();
    BackupSeriesResult BackupSeries();
    void RestoreWatchedMovies();
    void RestoreWatchedSeries();
  }
}