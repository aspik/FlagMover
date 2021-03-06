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

namespace FlagMover.Services
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
            result = true;
          }
          else
          {
            GetLogger().Error("FlagMover: Marking media item '{0}' as watched failed:", mediaItem.GetType());
          }
        }
        catch (Exception ex)
        {
          GetLogger().Error("FlagMover: Exception occurred while marking media item '{0}' as watched. Message: {1}", mediaItem.GetType(), ex.Message);
        }
      }
      else
      {
        GetLogger().Warn("FlagMover: The SetWatched action is not available for '{0}'. Already marked as watched?", mediaItem.GetType());
      }

      return result;
    }
  }
}