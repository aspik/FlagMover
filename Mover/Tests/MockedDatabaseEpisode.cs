using System;
using System.Collections.Generic;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.UserProfileDataManagement;

namespace Tests
{
  public class MockedDatabaseEpisode
  {
    public MediaItem Episode { get; }

    public MockedDatabaseEpisode(string tvDbId, int seasonIndex, List<int> episodeIndex, int playPercentage)
    {
      IDictionary<Guid, IList<MediaItemAspect>> episodeAspects = new Dictionary<Guid, IList<MediaItemAspect>>();
      MultipleMediaItemAspect resourceAspect = new MultipleMediaItemAspect(ProviderResourceAspect.Metadata);
      resourceAspect.SetAttribute(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH, "c:\\" + tvDbId + ".mkv");
      MediaItemAspect.AddOrUpdateAspect(episodeAspects, resourceAspect);
      MediaItemAspect.AddOrUpdateExternalIdentifier(episodeAspects, ExternalIdentifierAspect.SOURCE_TVDB, ExternalIdentifierAspect.TYPE_SERIES, tvDbId);
      MediaItemAspect.SetAttribute(episodeAspects, EpisodeAspect.ATTR_SEASON, seasonIndex);
      MediaItemAspect.SetCollectionAttribute(episodeAspects, EpisodeAspect.ATTR_EPISODE, episodeIndex);
      SingleMediaItemAspect smia = new SingleMediaItemAspect(MediaAspect.Metadata);
      MediaItemAspect.SetAspect(episodeAspects, smia);
      IDictionary<string, string> userData = new Dictionary<string, string>();
      userData.Add(UserDataKeysKnown.KEY_PLAY_PERCENTAGE, playPercentage.ToString());

      Episode = new MediaItem(Guid.NewGuid(), episodeAspects, userData);
    }
  }
}