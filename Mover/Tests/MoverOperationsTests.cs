using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlagMover;
using FlagMover.Entities;
using FlagMover.Exceptions;
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
      IList<MediaItem> databaseMediaItems = new List<MediaItem>
      {
        new MockedDatabaseEpisode("272127", 1, new List<int> { 7, 8 }, 100).Episode,
        new MockedDatabaseEpisode("275278", 2, new List<int> { 1 }, 100).Episode,
        new MockedDatabaseEpisode("275278", 4, new List<int> { 10 }, 100).Episode,
        new MockedDatabaseEpisode("275271", 2, new List<int> { 11, 12 }, 90).Episode
      };
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
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IContentDirectory contentDirectory = Substitute.For<IContentDirectory>();
      IList<MediaItem> databaseMediaItems = new List<MediaItem>
      {
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt0268380", Title = "Title_1", Tmdb = 12345, Year = 2017}, 100).Movie,
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt0034492", Title = "Title_2", Tmdb = 12111, Year = 2016}, 100).Movie,
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt1355630", Title = "Title_3", Tmdb = 12388, Year = 2013}, 100).Movie,
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt1599348", Title = "Title_4", Tmdb = 12100, Year = 2011}, 10).Movie,
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt0457939", Title = "Title_5", Tmdb = 12144, Year = 2010}, 0).Movie

      };
      contentDirectory.SearchAsync(Arg.Any<MediaItemQuery>(), true, null, false).Returns(databaseMediaItems);
      mediaPortalServices.GetServerConnectionManager().ContentDirectory.Returns(contentDirectory);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();

      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act
      BackupResult result = operations.BackupMovies(FakePath);

      // Assert
      Assert.Equal(3, result.WatchedCount);
      Assert.Equal(5, result.CollectedCount);
    }

    [Fact]
    public void Should_MarkFourEpisodesAsWatched_When_FourEpisodesRestored()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IContentDirectory contentDirectory = Substitute.For<IContentDirectory>();
      IList<MediaItem> databaseMediaItems = new List<MediaItem>
      {
        new MockedDatabaseEpisode("272127", 1, new List<int> { 7, 8 }, 0).Episode,
        new MockedDatabaseEpisode("317653", 1, new List<int> { 1 }, 0).Episode,
        new MockedDatabaseEpisode("317653", 1, new List<int> { 2 }, 0).Episode,
        new MockedDatabaseEpisode("275271", 2, new List<int> { 11, 12 }, 0).Episode
      };
      contentDirectory.SearchAsync(Arg.Any<MediaItemQuery>(), true, null, false).Returns(databaseMediaItems);
      mediaPortalServices.GetServerConnectionManager().ContentDirectory.Returns(contentDirectory);
      mediaPortalServices.MarkAsWatched(Arg.Any<MediaItem>()).Returns(true);

      string savedEpisodesPath = Path.Combine(FakePath, FileName.WatchedEpisodes.Value);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();
      fileOperations.FileExists(savedEpisodesPath).Returns(true);
      string watchedEpisodesJson =
        "[{\"show_imdb\":\"tt6682754\",\"show_tvdb\":317653,\"show_title\":\"Je-an-Claude Van Johnson\",\"season\":1,\"number\":1}," +
        "{\"show_imdb\":\"tt3155320\",\"show_tvdb\":272127,\"show_title\":\"Extant\",\"season\":1,\"number\":7}," +
        "{\"show_imdb\":\"tt3155320\",\"show_tvdb\":272127,\"show_title\":\"Extant\",\"season\":1,\"number\":8}," +
        "{\"show_imdb\":\"tt6682754\",\"show_tvdb\":317653,\"show_title\":\"Jean-Claude Van Johnson\",\"season\":1,\"number\":2}]";

      fileOperations.FileReadAllText(savedEpisodesPath).Returns(watchedEpisodesJson);

      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act
      RestoreResult result = operations.RestoreWatchedSeries(FakePath);

      // Assert
      Assert.Equal(4, result.MarkedWatchedCount);
      Assert.Equal(4, result.SavedWatchedCount);
    }

    [Fact]
    public void Should_MarkOneMovieAsWatched_When_OneMovieRestored()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IContentDirectory contentDirectory = Substitute.For<IContentDirectory>();
      IList<MediaItem> databaseMediaItems = new List<MediaItem>
      {
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt0268380", Title = "Ice Age", Tmdb = null, Year = 2002}, 0).Movie,
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt0457939", Title = "Title_2", Tmdb = null, Year = 2016}, 0).Movie,
        new MockedDatabaseMovie(new MediaLibraryMovie {Imdb = "tt1355630", Title = "Title_3", Tmdb = null, Year = 2013}, 0).Movie

      };
      contentDirectory.SearchAsync(Arg.Any<MediaItemQuery>(), true, null, false).Returns(databaseMediaItems);
      mediaPortalServices.GetServerConnectionManager().ContentDirectory.Returns(contentDirectory);
      mediaPortalServices.MarkAsWatched(Arg.Any<MediaItem>()).Returns(true);

      string savedMoviesPath = Path.Combine(FakePath, FileName.WatchedMovies.Value);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();
      fileOperations.FileExists(savedMoviesPath).Returns(true);
      string watchedMoviesJson =
        "[{\"imdb\":\"tt0268380\",\"tmdb\":null,\"title\":\"Ice Age\",\"year\":2002}]";
      fileOperations.FileReadAllText(savedMoviesPath).Returns(watchedMoviesJson);

      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act
      RestoreResult result = operations.RestoreWatchedMovies(FakePath);

      // Assert
      Assert.Equal(1, result.MarkedWatchedCount);
      Assert.Equal(1, result.SavedWatchedCount);
    }

    [Fact]
    public void Should_ThrowMediaLibraryNotConnectedException_When_ContentDirectoryIsNull()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IFileOperations fileOperations = Substitute.For<IFileOperations>();
      mediaPortalServices.GetServerConnectionManager().ContentDirectory.Returns(x => null);

      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act & Assert
      Assert.Throws<MediaLibraryNotConnectedException>(() => operations.BackupMovies(FakePath));
    }

    [Fact]
    public void Should_ThrowPathNotFoundException_When_WatchedMoviesFileNotFound()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();

      string savedMoviesPath = Path.Combine(FakePath, FileName.WatchedMovies.Value);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();
      fileOperations.FileExists(savedMoviesPath).Returns(false);
      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act & Assert
      Assert.Throws<PathNotFoundException>(() => operations.RestoreWatchedMovies(FakePath));
    }

    [Fact]
    public void Should_ThrowPathNotFoundException_When_WatchedEpisodesFileNotFound()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();

      string savedEpisodesPath = Path.Combine(FakePath, FileName.WatchedEpisodes.Value);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();
      fileOperations.FileExists(savedEpisodesPath).Returns(false);
      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act & Assert
      Assert.Throws<PathNotFoundException>(() => operations.RestoreWatchedSeries(FakePath));
    }
  }
}
