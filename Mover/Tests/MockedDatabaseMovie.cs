using System;
using System.Collections.Generic;
using FlagMover.Entities;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;

namespace Tests
{
  public class MockedDatabaseMovie
  {
    public MediaItem Movie { get; }

    public MockedDatabaseMovie(MediaLibraryMovie movie)
    {
      IDictionary<Guid, IList<MediaItemAspect>> movieAspects = new Dictionary<Guid, IList<MediaItemAspect>>();
      MultipleMediaItemAspect resourceAspect = new MultipleMediaItemAspect(ProviderResourceAspect.Metadata);
      resourceAspect.SetAttribute(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH, "c:\\" + movie.Title + ".mkv");
      MediaItemAspect.AddOrUpdateAspect(movieAspects, resourceAspect);
      MediaItemAspect.AddOrUpdateExternalIdentifier(movieAspects, ExternalIdentifierAspect.SOURCE_IMDB, ExternalIdentifierAspect.TYPE_MOVIE, movie.Imdb);
      MediaItemAspect.SetAttribute(movieAspects, MovieAspect.ATTR_MOVIE_NAME, movie.Title);

      SingleMediaItemAspect mediaItemAspect = new SingleMediaItemAspect(MediaAspect.Metadata);
      mediaItemAspect.SetAttribute(MediaAspect.ATTR_PLAYCOUNT, 1);
      mediaItemAspect.SetAttribute(MediaAspect.ATTR_LASTPLAYED, DateTime.Now);
      MediaItemAspect.SetAspect(movieAspects, mediaItemAspect);

      SingleMediaItemAspect importerAspect = new SingleMediaItemAspect(ImporterAspect.Metadata);
      importerAspect.SetAttribute(ImporterAspect.ATTR_DATEADDED, DateTime.Now);
      MediaItemAspect.SetAspect(movieAspects, importerAspect);

      Movie = new MediaItem(Guid.NewGuid(), movieAspects);
    }
  }
}