﻿using System;
using System.IO;
using System.Threading.Tasks;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.PathManager;
using MediaPortal.Common.Threading;
using MediaPortal.Common.UserManagement;
using MediaPortal.UiComponents.Media.MediaItemActions;
using MediaPortal.UI.ServerCommunication;

namespace Mover.Services
{
  public class MediaPortalServices : IMediaPortalServices
  {
    public IThreadPool GetThreadPool()
    {
      return ServiceRegistration.Get<IThreadPool>();
    }

    public ILogger GetLogger()
    {
      return ServiceRegistration.Get<ILogger>();
    }

    public IServerConnectionManager GetServerConnectionManager()
    {
      return ServiceRegistration.Get<IServerConnectionManager>();
    }

    public IUserManagement GetUserManagement()
    {
      return ServiceRegistration.Get<IUserManagement>();
    }

    public string GetMoverUserHomePath()
    {
      string rootPath = ServiceRegistration.Get<IPathManager>().GetPath(@"<DATA>\Mover\");
      string userProfileId = GetUserManagement().CurrentUser.ProfileId.ToString();

      return Path.Combine(rootPath, userProfileId);
    }

    public async Task<bool> MarkAsWatched(MediaItem mediaItem)
    {
      bool result = false;
      SetWatched setWatchedAction = new SetWatched();
      if (await setWatchedAction.IsAvailableAsync(mediaItem))
      {
        try
        {
          var processResult = await setWatchedAction.ProcessAsync(mediaItem);
          if (processResult.Success && processResult.Result != ContentDirectoryMessaging.MediaItemChangeType.None)
          {
            ContentDirectoryMessaging.SendMediaItemChangedMessage(mediaItem, processResult.Result);
            GetLogger().Info("Marking media item '{0}' as watched", mediaItem.GetType());
            result = true;
          }
        }
        catch (Exception ex)
        {
          GetLogger().Error("Marking media item '{0}' as watched failed:", mediaItem.GetType(), ex);
          result = false;
        }
      }

      return result;
    }
  }
}