using System;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.Common.Threading;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;

namespace FlagMover.Models
{
  public class BackupModel : BaseModel, IWorkflowModel
  {
    private static readonly Guid BACKUP_MODEL_ID = new Guid("8621AA8C-509A-45FD-B47D-1AE9DDDB46DD");

    public void BackupLibrary()
    {
      try
      {
        IThreadPool threadPool = _mediaPortalServices.GetThreadPool();
        threadPool.Add(() =>
        {
          Status = "[Backup.Movies]";
          _moverOperations.BackupMovies(_selectedPath);
          Status = "[Backup.Series]";
          _moverOperations.BackupSeries(_selectedPath);
          Status = "[Backup.Finished]";
        }, ThreadPriority.BelowNormal);
      }
      catch (Exception ex)
      {
        Status = "[Backup.Failed]";
        _mediaPortalServices.GetLogger().Error(ex.Message);
      }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      Status = "[Mover.Ready]";
      _directoryTree = new ItemsList();
      RefreshDirectoryTree(_directoryTree, ".");
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      _directoryTree = null;
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {

    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {

    }

    public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
    {

    }

    public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
    {

    }

    public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
    {
      return ScreenUpdateMode.AutoWorkflowManager;
    }

    public Guid ModelId
    {
      get { return BACKUP_MODEL_ID; }
    }
  }
} 