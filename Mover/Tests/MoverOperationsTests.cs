using System;
using System.Collections.Generic;
using System.Linq;
using FlagMover;
using FlagMover.Entities;
using FlagMover.Models;
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
    [Fact]
    public void BackupSeries()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IContentDirectory contentDirectory = Substitute.For<IContentDirectory>();
      IList<MediaItem> databaseMediaItems = new List<MediaItem>();
      databaseMediaItems.Add(new MockedDatabaseEpisode("272127", 1, new List<int> {7,8}, 100).Episode);
      contentDirectory.SearchAsync(Arg.Any<MediaItemQuery>(), true, null, false).Returns(databaseMediaItems);
      mediaPortalServices.GetServerConnectionManager().ContentDirectory.Returns(contentDirectory);
      IFileOperations fileOperations = Substitute.For<IFileOperations>();

      IMoverOperations operations = new MoverOperations(mediaPortalServices, fileOperations);

      // Act
      BackupSeriesResult result = operations.BackupSeries("");

      // Assert
      Assert.Equal(2, result.WatchedEpisodesCount);
    }

    [Fact]
    public void EnteringModelFillsItemsTreeWithDrivers()
    {
      // Arrange
      IMediaPortalServices mediaPortalServices = Substitute.For<IMediaPortalServices>();
      IFileOperations fileOperations = new FileOperations();
      IMoverOperations operations = Substitute.For<IMoverOperations>();
      BackupModel backupModel = null;//new BackupModel(mediaPortalServices, operations, fileOperations);
      
      // Act
      backupModel.EnterModelContext(null, null);

      // Assert
      Assert.NotNull(backupModel.DirectoryTree);
    }
  }
}
