using System;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.Common.Threading;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;

namespace FlagMover.Models
{
  public class RestoreModel : BaseModel, IWorkflowModel
  {
    private static readonly Guid RESTORE_MODEL_ID = new Guid("E3010B93-7AD4-41BA-8F13-31760E9D65DE");

    public void RestoreLibrary()
    {
      try
      {
        IThreadPool threadPool = _mediaPortalServices.GetThreadPool();
        threadPool.Add(() =>
        {
          Status = "[Restore.Movies]";
          _moverOperations.RestoreWatchedMovies(_selectedPath);
          Status = "[Restore.Series]";
          _moverOperations.RestoreWatchedSeries(_selectedPath);
          Status = "[Restore.Finished]";
        }, ThreadPriority.BelowNormal);
      }
      catch (Exception ex)
      {
        Status = "[Restore.Failed]";
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
      get { return RESTORE_MODEL_ID; }
    }
  }
}