namespace FlagMover.Services
{
  public interface IMoverOperations
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    BackupResult BackupMovies(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    BackupResult BackupSeries(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    RestoreResult RestoreWatchedMovies(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    RestoreResult RestoreWatchedSeries(string path);
  }
}