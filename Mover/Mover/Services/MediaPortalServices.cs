using System;
using System.IO;
using System.Threading.Tasks;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.PathManager;
using MediaPortal.Common.Threading;
using MediaPortal.UiComponents.Media.MediaItemActions;
using MediaPortal.UI.ServerCommunication;
using MediaPortal.UI.Services.UserManagement;

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

    public bool MarkAsWatched(MediaItem mediaItem)
    {
      bool result = false;
      SetWatched setWatchedAction = new SetWatched();
      if (setWatchedAction.IsAvailable(mediaItem))
      {
        try
        {
          ContentDirectoryMessaging.MediaItemChangeType changeType;
          if (setWatchedAction.Process(mediaItem, out changeType) && changeType != ContentDirectoryMessaging.MediaItemChangeType.None)
          {
            ContentDirectoryMessaging.SendMediaItemChangedMessage(mediaItem, changeType);
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