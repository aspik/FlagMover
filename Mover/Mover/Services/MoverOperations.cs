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
  public class MoverOperations : IMoverOperations
  {
    private readonly IMediaPortalServices _mediaPortalServices;
    private readonly IFileOperations _fileOperations;

    public MoverOperations(IMediaPortalServices mediaPortalServices, IFileOperations fileOperations)
    {
      _mediaPortalServices = mediaPortalServices;
      _fileOperations = fileOperations;
    }

    public BackupMoviesResult BackupMovies(string path)
    {
      BackupMoviesResult result = new BackupMoviesResult {CollectedMoviesCount = 0, WatchedMoviesCount = 0};
      Guid[] types =
      {
        MediaAspect.ASPECT_ID, MovieAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ExternalIdentifierAspect.ASPECT_ID, ProviderResourceAspect.ASPECT_ID, VideoStreamAspect.ASPECT_ID,
        VideoAudioStreamAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = _mediaPortalServices.GetServerConnectionManager().ContentDirectory;
      if (contentDirectory == null)
      {
        throw new MediaLibraryNotConnectedException("ML not connected");
      }

      Guid? userProfile = null;
      IUserManagement userProfileDataManagement = _mediaPortalServices.GetUserManagement();
      if (userProfileDataManagement != null && userProfileDataManagement.IsValidUser)
      {
        userProfile = userProfileDataManagement.CurrentUser.ProfileId;
      }

      IList<MediaItem> collectedMovieMediaItems = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;
      result.CollectedMoviesCount = collectedMovieMediaItems.Count;
      List<MediaItem> watchedMovieMediaItems = collectedMovieMediaItems.Where(MediaItemAspectsUtl.IsWatched).ToList();
      IList<MediaLibraryMovie> watchedMovies = new List<MediaLibraryMovie>();

      foreach (MediaItem movieMediaItem in watchedMovieMediaItems)
      {
        watchedMovies.Add(new MediaLibraryMovie
        {
          Title = MediaItemAspectsUtl.GetMovieTitle(movieMediaItem),
          Imdb = MediaItemAspectsUtl.GetMovieImdbId(movieMediaItem),
          Year = MediaItemAspectsUtl.GetMovieYear(movieMediaItem)
        });
      }
      SaveLibraryMovies(path, watchedMovies);
      result.WatchedMoviesCount = watchedMovies.Count;

      return result;
    }

    public BackupSeriesResult BackupSeries(string path)
    {
      BackupSeriesResult result = new BackupSeriesResult {WatchedEpisodesCount = 0, CollectedEpisodesCount = 0};
      Guid[] types =
      {
        MediaAspect.ASPECT_ID, EpisodeAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ProviderResourceAspect.ASPECT_ID, ExternalIdentifierAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = _mediaPortalServices.GetServerConnectionManager().ContentDirectory;
      if (contentDirectory == null)
      {
        throw new MediaLibraryNotConnectedException("ML not connected");
      }

      Guid? userProfile = null;
      IUserManagement userProfileDataManagement = _mediaPortalServices.GetUserManagement();
      if (userProfileDataManagement != null && userProfileDataManagement.IsValidUser)
      {
        userProfile = userProfileDataManagement.CurrentUser.ProfileId;
      }

      IList<MediaItem> collectedEpisodeMediaItems = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;
      result.CollectedEpisodesCount = collectedEpisodeMediaItems.Count;
      List<MediaItem> watchedEpisodeMediaItems = collectedEpisodeMediaItems.Where(MediaItemAspectsUtl.IsWatched).ToList();
      IList<MediaLibraryEpisode> watchedEpisodes = new List<MediaLibraryEpisode>();

      foreach (MediaItem episodeMediaItem in watchedEpisodeMediaItems)
      {
        List<int> episodeNumbers = MediaItemAspectsUtl.GetEpisodeNumbers(episodeMediaItem);
        if (!episodeNumbers.Any())
        {
          break;
        }

        bool isMultiEpisode = episodeNumbers.Count > 1;

        if (isMultiEpisode)
        {
          foreach (int epNumber in episodeNumbers)
          {
            watchedEpisodes.Add(new MediaLibraryEpisode
            {
              ShowTitle = MediaItemAspectsUtl.GetSeriesTitle(episodeMediaItem),
              ShowImdb = MediaItemAspectsUtl.GetSeriesImdbId(episodeMediaItem),
              ShowTvdb = MediaItemAspectsUtl.GetTvdbId(episodeMediaItem),
              Season = MediaItemAspectsUtl.GetSeasonIndex(episodeMediaItem),
              Number = epNumber
            });
          }
        }
        else
        {
          watchedEpisodes.Add(new MediaLibraryEpisode
          {
            ShowTitle = MediaItemAspectsUtl.GetSeriesTitle(episodeMediaItem),
            ShowImdb = MediaItemAspectsUtl.GetSeriesImdbId(episodeMediaItem),
            ShowTvdb = MediaItemAspectsUtl.GetTvdbId(episodeMediaItem),
            Season = MediaItemAspectsUtl.GetSeasonIndex(episodeMediaItem),
            Number = MediaItemAspectsUtl.GetEpisodeNumbers(episodeMediaItem).FirstOrDefault()
          });
        }
      }
      SaveLibraryEpisodes(path, watchedEpisodes);
      result.WatchedEpisodesCount = watchedEpisodes.Count;

      return result;
    }

    public void RestoreWatchedMovies(string path)
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
        throw new Exception("Wrong movies path");
      }

      Guid[] types =
      {
        MediaAspect.ASPECT_ID, MovieAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ExternalIdentifierAspect.ASPECT_ID, ProviderResourceAspect.ASPECT_ID, VideoStreamAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = _mediaPortalServices.GetServerConnectionManager().ContentDirectory;
      if (contentDirectory == null)
      {
        throw new MediaLibraryNotConnectedException("ML not connected");
      }

      Guid? userProfile = null;
      IUserManagement userProfileDataManagement = _mediaPortalServices.GetUserManagement();
      if (userProfileDataManagement != null && userProfileDataManagement.IsValidUser)
      {
        userProfile = userProfileDataManagement.CurrentUser.ProfileId;
      }

      IList<MediaItem> collectedMovies = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;

      foreach (MediaLibraryMovie watchedMovie in watchedMovies)
      {
        MediaItem localMovie = collectedMovies.FirstOrDefault(m => MovieMatch(m, watchedMovie));
        if (localMovie == null)
        {
          continue;
        }

        if (_mediaPortalServices.MarkAsWatched(localMovie).Result)
        {
          _mediaPortalServices.GetLogger().Info(
            "Marking movie as watched in library. Title = '{0}', Year = '{1}', IMDb ID = '{2}', TMDb ID = '{3}'",
            watchedMovie.Title, watchedMovie.Year.HasValue ? watchedMovie.Year.ToString() : "<empty>",
            watchedMovie.Imdb ?? "<empty>", watchedMovie.Tmdb.HasValue ? watchedMovie.Tmdb.ToString() : "<empty>");
        }
      }
    }

    public void RestoreWatchedSeries(string path)
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
        throw new Exception("Wrong series path");
      }

      Guid[] types =
      {
        MediaAspect.ASPECT_ID, EpisodeAspect.ASPECT_ID, VideoAspect.ASPECT_ID, ImporterAspect.ASPECT_ID,
        ProviderResourceAspect.ASPECT_ID, ExternalIdentifierAspect.ASPECT_ID
      };

      IContentDirectory contentDirectory = _mediaPortalServices.GetServerConnectionManager().ContentDirectory;
      if (contentDirectory == null)
      {
        throw new MediaLibraryNotConnectedException("ML not connected");
      }

      Guid? userProfile = null;
      IUserManagement userProfileDataManagement = _mediaPortalServices.GetUserManagement();
      if (userProfileDataManagement != null && userProfileDataManagement.IsValidUser)
      {
        userProfile = userProfileDataManagement.CurrentUser.ProfileId;
      }

      IList<MediaItem> localEpisodes = contentDirectory.SearchAsync(new MediaItemQuery(types, null, null), true, userProfile, false).Result;
      ILookup<string, MediaLibraryEpisode> onlineEpisodes = watchedEpisodes.ToLookup(twe => CreateLookupKey(twe), twe => twe);
      
      foreach (MediaItem episode in localEpisodes)
      {
        string tvdbKey = CreateLookupKey(episode);

        MediaLibraryEpisode watchedEpisode = onlineEpisodes[tvdbKey].FirstOrDefault();
        if (watchedEpisode != null)
        {
          if (_mediaPortalServices.MarkAsWatched(episode).Result)
          {
            _mediaPortalServices.GetLogger().Info(
              "Marking episode as watched in library. Title = '{0}, Season = '{1}', Episode = '{2}', Show TVDb ID = '{3}', Show IMDb ID = '{4}'",
              watchedEpisode.ShowTitle, watchedEpisode.Season,
              watchedEpisode.Number, watchedEpisode.ShowTvdb.HasValue ? watchedEpisode.ShowTvdb.ToString() : "<empty>",
              watchedEpisode.ShowImdb ?? "<empty>");
          }
        }
      }
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
        show = episode.ShowTvdb.Value.ToString();
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
      uint tvdbId = MediaItemAspectsUtl.GetTvdbId(episode);
      int seasonIndex = MediaItemAspectsUtl.GetSeasonIndex(episode);
      int episodeIndex = MediaItemAspectsUtl.GetEpisodeNumbers(episode).FirstOrDefault();

      return string.Format("{0}_{1}_{2}", tvdbId, seasonIndex, episodeIndex);
    }
  }
}