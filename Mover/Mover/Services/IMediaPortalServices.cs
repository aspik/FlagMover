using System.Threading.Tasks;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.Threading;
using MediaPortal.Common.UserManagement;
using MediaPortal.UI.ServerCommunication;

namespace FlagMover.Services
{
  public interface IMediaPortalServices
  {
    IThreadPool GetThreadPool();
    ILogger GetLogger();
    IServerConnectionManager GetServerConnectionManager();
    IUserManagement GetUserManagement();
    string GetMoverUserHomePath();
    Task<bool> MarkAsWatched(MediaItem mediaItem);
  }
}