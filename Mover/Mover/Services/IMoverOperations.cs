namespace Mover.Services
{
  public interface IMoverOperations
  {
    void BackupMovies();
    void BackupSeries();
    void RestoreWatchedMovies();
    void RestoreWatchedSeries();
  }
}