using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.SystemCommunication;
using MediaPortal.Common.UserManagement;
using Mover.Entities;
using Mover.Exceptions;
using Mover.Utilities;
using Newtonsoft.Json;

namespace Mover.Services
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

    public void BackupMovies()
    {
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
      SaveLibraryMovies(watchedMovies);
    }

    public void BackupSeries()
    {
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
      List<MediaItem> watchedEpisodeMediaItems = collectedEpisodeMediaItems.Where(MediaItemAspectsUtl.IsWatched).ToList();
      IList<MediaLibraryEpisode> watchedEpisodes = new List<MediaLibraryEpisode>();

      foreach (MediaItem episodeMediaItem in watchedEpisodeMediaItems)
      {
        watchedEpisodes.Add(new MediaLibraryEpisode
        {
          ShowTitle = MediaItemAspectsUtl.GetSeriesTitle(episodeMediaItem),
          ShowImdb = MediaItemAspectsUtl.GetSeriesImdbId(episodeMediaItem),
          ShowTvdb = MediaItemAspectsUtl.GetTvdbId(episodeMediaItem),
          Season = MediaItemAspectsUtl.GetSeasonIndex(episodeMediaItem),
          Number = MediaItemAspectsUtl.GetEpisodeIndex(episodeMediaItem)
        });
      }
      SaveLibraryEpisodes(watchedEpisodes);
    }

    private void SaveLibraryMovies(IList<MediaLibraryMovie> watchedMovies)
    {
      string libraryMoviesPath = Path.Combine(_mediaPortalServices.GetMoverUserHomePath(), FileName.WatchedMovies.Value);
      string libraryMoviesJson = JsonConvert.SerializeObject(watchedMovies);
      _fileOperations.FileWriteAllText(libraryMoviesPath, libraryMoviesJson, Encoding.UTF8);
    }

    private void SaveLibraryEpisodes(IList<MediaLibraryEpisode> watchedEpisodes)
    {
      string libraryMoviesPath = Path.Combine(_mediaPortalServices.GetMoverUserHomePath(), FileName.WatchedEpisodes.Value);
      string libraryMoviesJson = JsonConvert.SerializeObject(watchedEpisodes);
      _fileOperations.FileWriteAllText(libraryMoviesPath, libraryMoviesJson, Encoding.UTF8);
    }
  }
}