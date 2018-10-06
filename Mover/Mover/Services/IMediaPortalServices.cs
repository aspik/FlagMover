using System.Threading.Tasks;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.Threading;
using MediaPortal.UI.ServerCommunication;
using MediaPortal.UI.Services.UserManagement;

namespace FlagMover.Services
{
  public interface IMediaPortalServices
  {
    IThreadPool GetThreadPool();
    ILogger GetLogger();
    IServerConnectionManager GetServerConnectionManager();
    IUserManagement GetUserManagement();
    bool MarkAsWatched(MediaItem mediaItem);
  }
}