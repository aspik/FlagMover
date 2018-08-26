using MediaPortal.Common.Logging;
using MediaPortal.Common.Threading;
using MediaPortal.Common.UserManagement;
using MediaPortal.UI.ServerCommunication;

namespace Mover.Services
{
  public interface IMediaPortalServices
  {
    IThreadPool GetThreadPool();
    ILogger GetLogger();
    IServerConnectionManager GetServerConnectionManager();
    IUserManagement GetUserManagement();
    string GetMoverUserHomePath();
  }
}