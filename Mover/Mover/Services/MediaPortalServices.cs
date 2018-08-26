using System.IO;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.PathManager;
using MediaPortal.Common.Threading;
using MediaPortal.Common.UserManagement;
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
  }
}