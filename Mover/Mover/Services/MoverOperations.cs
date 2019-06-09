using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlagMover.Entities;
using FlagMover.Exceptions;
using FlagMover.Utilities;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.SystemCommunication;
using MediaPortal.Common.UserManagement;
using Newtonsoft.Json;

namespace FlagMover.Services
{
  /// <summary>
  /// 
  /// </summary>
  public class MoverOperations : IMoverOperations
  {
    private readonly IMediaPortalServices _mediaPortalServices;
    private readonly IFileOperations _fileOperations;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaPortalServices"></param>
    /// <param name="fileOperations"></param>
    public MoverOperations(IMediaPortalServices mediaPortalServices, IFileOperations fileOperations)
    {
      _mediaPortalServices = mediaPortalServices;
      _fileOperations = fileOperations;
    }

    /// <inheritdoc />
    public BackupResult BackupMovies(string path)
    {
      BackupResult result = new BackupResult { CollectedCount = 0, WatchedCount = 0 };
      Guid[] types =
      {
        MediaAspect.ASPECT_ID, MovieAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ExternalIdentifierAspect.ASPECT_ID, ProviderResourceAspect.ASPECT_ID, VideoStreamAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = GetContentDirectory();
      Guid? userProfile = GetUserProfileId();

      IList<MediaItem> collectedMovieMediaItems = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;
      result.CollectedCount = collectedMovieMediaItems.Count;
      List<MediaItem> watchedMovieMediaItems = collectedMovieMediaItems.Where(MediaItemAspectsUtl.IsWatched).ToList();
      IList<MediaLibraryMovie> watchedMovies = new List<MediaLibraryMovie>();

      foreach (MediaItem watchedMovieItem in watchedMovieMediaItems)
      {
        watchedMovies.Add(new MediaLibraryMovie
        {
          Title = MediaItemAspectsUtl.GetMovieTitle(watchedMovieItem),
          Imdb = MediaItemAspectsUtl.GetMovieImdbId(watchedMovieItem),
          Year = MediaItemAspectsUtl.GetMovieYear(watchedMovieItem)
        });
      }
      SaveLibraryMovies(path, watchedMovies);
      result.WatchedCount = watchedMovies.Count;

      return result;
    }

    /// <inheritdoc />
    public BackupResult BackupSeries(string path)
    {
      BackupResult result = new BackupResult { WatchedCount = 0, CollectedCount = 0 };
      Guid[] types =
      {
        MediaAspect.ASPECT_ID, EpisodeAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ProviderResourceAspect.ASPECT_ID, ExternalIdentifierAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = GetContentDirectory();
      Guid? userProfile = GetUserProfileId();

      IList<MediaItem> collectedEpisodeMediaItems = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;

      foreach (MediaItem collectedEpisodeMediaItem in collectedEpisodeMediaItems)
      {
        List<int> episodeNumbers = MediaItemAspectsUtl.GetEpisodeNumbers(collectedEpisodeMediaItem);
        result.CollectedCount = result.CollectedCount + episodeNumbers.Count;
      }

      List<MediaItem> watchedEpisodeMediaItems = collectedEpisodeMediaItems.Where(MediaItemAspectsUtl.IsWatched).ToList();
      IList<MediaLibraryEpisode> watchedEpisodes = new List<MediaLibraryEpisode>();

      foreach (MediaItem watchedEpisodeItem in watchedEpisodeMediaItems)
      {
        List<int> episodeNumbers = MediaItemAspectsUtl.GetEpisodeNumbers(watchedEpisodeItem);
        if (episodeNumbers.Any())
        {
          AddEpisodes(watchedEpisodes, watchedEpisodeItem, episodeNumbers);
        }
        else
        {
          _mediaPortalServices.GetLogger().Warn("FlagMover: Skipping series episode without episode number.");
        }
      }

      SaveLibraryEpisodes(path, watchedEpisodes);
      result.WatchedCount = watchedEpisodes.Count;

      return result;
    }

    /// <inheritdoc />
    public RestoreResult RestoreWatchedMovies(string path)
    {
      RestoreResult result = new RestoreResult { SavedWatchedCount = 0, MarkedWatchedCount = 0 };
      IList<MediaLibraryMovie> watchedMovies = ReadSavedWatchedMovies(path);
      result.SavedWatchedCount = watchedMovies.Count;

      Guid[] types =
      {
        MediaAspect.ASPECT_ID, MovieAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ExternalIdentifierAspect.ASPECT_ID, ProviderResourceAspect.ASPECT_ID, VideoStreamAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = GetContentDirectory();
      Guid? userProfile = GetUserProfileId();

      IList<MediaItem> collectedMovies = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;

      foreach (MediaLibraryMovie watchedMovie in watchedMovies)
      {
        MediaItem movie = collectedMovies.FirstOrDefault(m => MovieMatch(m, watchedMovie));
        if (movie == null)
        {
          continue;
        }

        if (_mediaPortalServices.MarkAsWatched(movie).Result)
        {
          _mediaPortalServices.GetLogger().Info(
            "FlagMover: Marking movie as watched in library. Title = '{0}', Year = '{1}', IMDb ID = '{2}', TMDb ID = '{3}'",
            watchedMovie.Title, watchedMovie.Year.HasValue ? watchedMovie.Year.ToString() : "<empty>",
            watchedMovie.Imdb ?? "<empty>", watchedMovie.Tmdb.HasValue ? watchedMovie.Tmdb.ToString() : "<empty>");

          result.MarkedWatchedCount++;
        }
      }
      return result;
    }

    /// <inheritdoc />
    public RestoreResult RestoreWatchedSeries(string path)
    {
      RestoreResult result = new RestoreResult { SavedWatchedCount = 0, MarkedWatchedCount = 0 };

      IList<MediaLibraryEpisode> watchedEpisodes = ReadSavedWatchedEpisodes(path);
      result.SavedWatchedCount = watchedEpisodes.Count;

      Guid[] types =
      {
        MediaAspect.ASPECT_ID, EpisodeAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ProviderResourceAspect.ASPECT_ID, ExternalIdentifierAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = GetContentDirectory();
      Guid? userProfile = GetUserProfileId();

      IList<MediaItem> localEpisodes = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;
      ILookup<string, MediaLibraryEpisode> loadedEpisodes = watchedEpisodes.ToLookup(twe => CreateLookupKey(twe), twe => twe);

      foreach (MediaItem episode in localEpisodes)
      {
        MatchEpisodeAndMarkAsWatched(result, loadedEpisodes, episode);
      }
      return result;
    }

    private void MatchEpisodeAndMarkAsWatched(RestoreResult result, ILookup<string, MediaLibraryEpisode> loadedEpisodes, MediaItem episode)
    {
      string tvdbKey = CreateLookupKey(episode);
      MediaLibraryEpisode watchedEpisode = loadedEpisodes[tvdbKey].FirstOrDefault();

      if (watchedEpisode != null)
      {
        if (_mediaPortalServices.MarkAsWatched(episode).Result)
        {
          _mediaPortalServices.GetLogger().Info(
            "FlagMover: Marking episode as watched in library. Title = '{0}, Season = '{1}', Episode = '{2}', Show TVDb ID = '{3}', Show IMDb ID = '{4}'",
            watchedEpisode.ShowTitle, watchedEpisode.Season,
            watchedEpisode.Number, watchedEpisode.ShowTvdb ?? "<empty>",
            watchedEpisode.ShowImdb ?? "<empty>");

          IncreaseResultCount(result, episode);
        }
      }
    }

    private void IncreaseResultCount(RestoreResult result, MediaItem episode)
    {
      List<int> episodeNumbers = MediaItemAspectsUtl.GetEpisodeNumbers(episode);
      bool isMultiEpisode = episodeNumbers.Count > 1;
      if (isMultiEpisode)
      {
        result.MarkedWatchedCount = result.MarkedWatchedCount + episodeNumbers.Count;
      }
      else
      {
        result.MarkedWatchedCount++;
      }
    }

    private IList<MediaLibraryEpisode> ReadSavedWatchedEpisodes(string path)
    {
      IList<MediaLibraryEpisode> watchedEpisodes;

      string watchedEpisodesPath = Path.Combine(path, FileName.WatchedEpisodes.Value);
      if (_fileOperations.FileExists(watchedEpisodesPath))
      {
        string watchedEpisodesJson = _fileOperations.FileReadAllText(watchedEpisodesPath);
        watchedEpisodes = JsonConvert.DeserializeObject<List<MediaLibraryEpisode>>(watchedEpisodesJson);
      }
      else
      {
        throw new PathNotFoundException("Watched series path not found.");
      }

      return watchedEpisodes;
    }

    private IList<MediaLibraryMovie> ReadSavedWatchedMovies(string path)
    {
      IList<MediaLibraryMovie> watchedMovies;

      string watchedMoviesPath = Path.Combine(path, FileName.WatchedMovies.Value);
      if (_fileOperations.FileExists(watchedMoviesPath))
      {
        string watchedMoviesJson = _fileOperations.FileReadAllText(watchedMoviesPath);
        watchedMovies = JsonConvert.DeserializeObject<List<MediaLibraryMovie>>(watchedMoviesJson);
      }
      else
      {
        throw new PathNotFoundException("Watched movies file not found.");
      }

      return watchedMovies;
    }

    private Guid? GetUserProfileId()
    {
      Guid? userProfile = null;
      IUserManagement userProfileDataManagement = _mediaPortalServices.GetUserManagement();
      if (userProfileDataManagement != null && userProfileDataManagement.IsValidUser)
      {
        userProfile = userProfileDataManagement.CurrentUser.ProfileId;
      }

      return userProfile;
    }

    private IContentDirectory GetContentDirectory()
    {
      IContentDirectory contentDirectory = _mediaPortalServices.GetServerConnectionManager().ContentDirectory;
      if (contentDirectory == null)
      {
        throw new MediaLibraryNotConnectedException("Media library not connected");
      }
      return contentDirectory;
    }

    private void AddEpisodes(IList<MediaLibraryEpisode> watchedEpisodes, MediaItem episodeMediaItem, List<int> episodeNumbers)
    {
      bool isMultiEpisode = episodeNumbers.Count > 1;

      if (isMultiEpisode)
      {
        foreach (int epNumber in episodeNumbers)
        {
          watchedEpisodes.Add(GetLibraryEpisode(episodeMediaItem, epNumber));
        }
      }
      else
      {
        watchedEpisodes.Add(GetLibraryEpisode(episodeMediaItem, episodeNumbers.FirstOrDefault()));
      }
    }

    private MediaLibraryEpisode GetLibraryEpisode(MediaItem episodeMediaItem, int epNumber)
    {
      return new MediaLibraryEpisode
      {
        Season = MediaItemAspectsUtl.GetSeasonIndex(episodeMediaItem),
        ShowImdb = MediaItemAspectsUtl.GetSeriesImdbId(episodeMediaItem),
        ShowTitle = MediaItemAspectsUtl.GetSeriesTitle(episodeMediaItem),
        ShowTvdb = MediaItemAspectsUtl.GetTvdbId(episodeMediaItem),
        Number = epNumber
      };
    }

    private void SaveLibraryMovies(string path, IList<MediaLibraryMovie> watchedMovies)
    {
      string libraryMoviesPath = Path.Combine(path, FileName.WatchedMovies.Value);
      string libraryMoviesJson = JsonConvert.SerializeObject(watchedMovies);
      _fileOperations.FileWriteAllText(libraryMoviesPath, libraryMoviesJson, Encoding.UTF8);
    }

    private void SaveLibraryEpisodes(string path, IList<MediaLibraryEpisode> watchedEpisodes)
    {
      string libraryMoviesPath = Path.Combine(path, FileName.WatchedEpisodes.Value);
      string libraryMoviesJson = JsonConvert.SerializeObject(watchedEpisodes);
      _fileOperations.FileWriteAllText(libraryMoviesPath, libraryMoviesJson, Encoding.UTF8);
    }

    private bool MovieMatch(MediaItem localMovie, MediaLibraryMovie watchedRestoreMovie)
    {
      bool result = false;

      if (!string.IsNullOrEmpty(watchedRestoreMovie.Imdb) && !string.IsNullOrEmpty(MediaItemAspectsUtl.GetMovieImdbId(localMovie)))
      {
        result = String.Compare(MediaItemAspectsUtl.GetMovieImdbId(localMovie), watchedRestoreMovie.Imdb, StringComparison.OrdinalIgnoreCase) == 0;
      }

      else if ((MediaItemAspectsUtl.GetMovieTmdbId(localMovie) != 0) && watchedRestoreMovie.Tmdb.HasValue)
      {
        result = MediaItemAspectsUtl.GetMovieTmdbId(localMovie) == watchedRestoreMovie.Tmdb.Value;
      }

      else if (String.Compare(MediaItemAspectsUtl.GetMovieTitle(localMovie), watchedRestoreMovie.Title, StringComparison.OrdinalIgnoreCase) == 0
               && (MediaItemAspectsUtl.GetMovieYear(localMovie) == watchedRestoreMovie.Year))
      {
        result = true;
      }
      return result;
    }

    private string CreateLookupKey(MediaLibraryEpisode episode)
    {
      string show;

      if (episode.ShowTvdb != null)
      {
        show = episode.ShowTvdb;
      }
      else if (episode.ShowImdb != null)
      {
        show = episode.ShowImdb;
      }
      else
      {
        if (episode.ShowTitle == null)
        {
          return episode.GetHashCode().ToString();
        }
        show = episode.ShowTitle;
      }
      return string.Format("{0}_{1}_{2}", show, episode.Season, episode.Number);
    }

    private string CreateLookupKey(MediaItem episode)
    {
      string tvdbId = MediaItemAspectsUtl.GetTvdbId(episode);
      int seasonIndex = MediaItemAspectsUtl.GetSeasonIndex(episode);
      int episodeIndex = MediaItemAspectsUtl.GetEpisodeNumbers(episode).FirstOrDefault();

      return string.Format("{0}_{1}_{2}", tvdbId, seasonIndex, episodeIndex);
    }
  }
}