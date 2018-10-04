using System;
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
            GetLogger().Info("FlagMover: marking media item '{0}' as watched", mediaItem.GetType());
            result = true;
          }
        }
        catch (Exception ex)
        {
          GetLogger().Error("FlagMover:: marking media item '{0}' as watched failed:", mediaItem.GetType(), ex);
          result = false;
        }
      }

      return result;
    }
  }
}