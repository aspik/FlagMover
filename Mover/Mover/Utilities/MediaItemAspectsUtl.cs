using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.UserProfileDataManagement;

namespace FlagMover.Utilities
{
  public static class MediaItemAspectsUtl
  {
    public static string GetMovieTitle(MediaItem mediaItem)
    {
      string value;
      return MediaItemAspect.TryGetAttribute(mediaItem.Aspects, MovieAspect.ATTR_MOVIE_NAME, out value) ? value : null;
    }

    public static DateTime GetDateAddedToDb(MediaItem mediaItem)
    {
      DateTime addedToDb;
      return MediaItemAspect.TryGetAttribute(mediaItem.Aspects, ImporterAspect.ATTR_DATEADDED, out addedToDb) ? addedToDb.ToUniversalTime() : DateTime.Now;
    }

    public static DateTime GetLastPlayedDate(MediaItem mediaItem)
    {
      DateTime lastplayed;
      return MediaItemAspect.TryGetAttribute(mediaItem.Aspects, MediaAspect.ATTR_LASTPLAYED, out lastplayed) ? lastplayed.ToUniversalTime() : DateTime.Now;
    }

    public static string GetMovieImdbId(MediaItem mediaItem)
    {
      string id;
      return MediaItemAspect.TryGetExternalAttribute(mediaItem.Aspects, ExternalIdentifierAspect.SOURCE_IMDB, ExternalIdentifierAspect.TYPE_MOVIE, out id) ? id : null;
    }

    public static uint? GetMovieTmdbId(MediaItem mediaItem)
    {
      string id;
      int tmdbId;
      return MediaItemAspect.TryGetExternalAttribute(mediaItem.Aspects, ExternalIdentifierAspect.SOURCE_TMDB, ExternalIdentifierAspect.TYPE_MOVIE, out id) && int.TryParse(id, out tmdbId) ? (uint?)tmdbId : null;
    }

    public static int GetMovieYear(MediaItem mediaItem)
    {
      DateTime dtValue;
      return MediaItemAspect.TryGetAttribute(mediaItem.Aspects, MediaAspect.ATTR_RECORDINGTIME, out dtValue) ? dtValue.Year : 0;
    }

    public static bool IsWatched(MediaItem mediaItem)
    {
      int playPercentage = 0;
      if (mediaItem.UserData.ContainsKey(UserDataKeysKnown.KEY_PLAY_PERCENTAGE))
      {
        int.TryParse(mediaItem.UserData[UserDataKeysKnown.KEY_PLAY_PERCENTAGE], out playPercentage);
      }

      return playPercentage == 100;
    }

    public static string GetSeriesTitle(MediaItem mediaItem)
    {
      string value;
      return MediaItemAspect.TryGetAttribute(mediaItem.Aspects, EpisodeAspect.ATTR_SERIES_NAME, out value) ? value : null;
    }

    public static string GetSeriesImdbId(MediaItem mediaItem)
    {
      string id;
      return MediaItemAspect.TryGetExternalAttribute(mediaItem.Aspects, ExternalIdentifierAspect.SOURCE_IMDB, ExternalIdentifierAspect.TYPE_SERIES, out id) ? id : null;
    }

    public static string GetTvdbId(MediaItem mediaItem)
    {
      string id;
      return MediaItemAspect.TryGetExternalAttribute(mediaItem.Aspects, ExternalIdentifierAspect.SOURCE_TVDB, ExternalIdentifierAspect.TYPE_SERIES, out id) ? id : null;
    }

    public static int GetSeasonIndex(MediaItem mediaItem)
    {
      int value;
      return MediaItemAspect.TryGetAttribute(mediaItem.Aspects, EpisodeAspect.ATTR_SEASON, out value) ? value : 0;
    }

    public static List<int> GetEpisodeNumbers(MediaItem mediaItem)
    {
      List<int> episodeNumbers = new List<int>();
      if (MediaItemAspect.TryGetAttribute(mediaItem.Aspects, EpisodeAspect.ATTR_EPISODE, out IEnumerable episodes))
      {
        foreach (int episode in episodes.Cast<int>())
        {
          episodeNumbers.Add(episode);
        }
      }
      return episodeNumbers.Distinct().ToList();
    }
  }
}