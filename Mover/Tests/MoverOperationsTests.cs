using System;
using System.Collections.Generic;
using System.Linq;
using FlagMover;
using FlagMover.Services;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.SystemCommunication;
using NSubstitute;
using Xunit;

namespace Tests
{
  public class MoverOperationsTests
  {
    private const string FakePath = "D:\\FlagMover\\";

    [Fact]
    public void Should_BackupFourEpisodes_When_SixEpisodesCollectedAndFourWatched()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IContentDirectory contentDirectory = Substitute.For<IContentDirectory>();
      IList<MediaItem> databaseMediaItems = new List<MediaItem>();
      databaseMediaItems.Add(new MockedDatabaseEpisode("272127", 1, new List<int> {7,8}, 100).Episode);
      databaseMediaItems.Add(new MockedDatabaseEpisode("275278", 2, new List<int> {1}, 100).Episode);
      databaseMediaItems.Add(new MockedDatabaseEpisode("275278", 4, new List<int> {10}, 100).Episode);
      databaseMediaItems.Add(new MockedDatabaseEpisode("275271", 2, new List<int> {11,12}, 90).Episode);
      contentDirectory.SearchAsync(Arg.Any<MediaItemQuery>(), true, null, false).Returns(databaseMediaItems);
      mediaPortalServices.GetServerConnectionManager().ContentDirectory.Returns(contentDirectory);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();

      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act
      BackupResult result = operations.BackupSeries(FakePath);

      // Assert
      Assert.Equal(4, result.WatchedCount);
      Assert.Equal(6, result.CollectedCount);
    }

    [Fact]
    public void Should_BackupThreeMovies_When_FiveMoviesCollectedAndThreeWatched()
    {
      // Arrange

      // Act
      BackupResult result = null;

      // Assert
      Assert.Equal(3, result.WatchedCount);
      Assert.Equal(5, result.CollectedCount);
    }

    [Fact]
    public void Should_MarkTwoEpisodesAsWatched_When_TwoEpisodesRestored()
    {
      // Arrange

      // Act
      RestoreResult result = null;

      // Assert
      Assert.Equal(2, result.MarkedWatchedCount);
      Assert.Equal(2, result.SavedWatchedCount);
    }

    [Fact]
    public void Should_MarkOneMovieAsWatched_When_OneMovieRestored()
    {
      // Arrange

      // Act
      RestoreResult result = null;

      // Assert
      Assert.Equal(1, result.MarkedWatchedCount);
      Assert.Equal(1, result.SavedWatchedCount);
    }
  }
}