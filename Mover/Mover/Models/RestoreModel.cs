using System;
using System.Collections.Generic;
using System.Threading;
using FlagMover.Services;
using MediaPortal.Common.General;
using MediaPortal.Common.Threading;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;

namespace FlagMover.Models
{
  public class RestoreModel : IWorkflowModel
  {
    private static readonly Guid RESTORE_MODEL_ID = new Guid("E3010B93-7AD4-41BA-8F13-31760E9D65DE");

    private readonly IMediaPortalServices _mediaPortalServices;
    private readonly IMoverOperations _moverOperations;

    private readonly AbstractProperty _statusProperty = new WProperty(typeof(string), string.Empty);

    public RestoreModel()
    {
      _mediaPortalServices = new MediaPortalServices();
      _moverOperations = new MoverOperations(_mediaPortalServices, new FileOperations());
    }

    public RestoreModel(IMediaPortalServices mediaPortalServices, IMoverOperations moverOperations)
    {
      _mediaPortalServices = mediaPortalServices;
      _moverOperations = moverOperations;
    }

    public AbstractProperty StatusProperty
    {
      get { return _statusProperty; }
    }

    public string Status
    {
      get { return (string)_statusProperty.GetValue(); }
      set { _statusProperty.SetValue(value); }
    }

    public void RestoreLibrary()
    {
      try
      {
        IThreadPool threadPool = _mediaPortalServices.GetThreadPool();
        threadPool.Add(() =>
        {
          Status = "[Restore.Movies]";
          _moverOperations.RestoreWatchedMovies();
          Status = "[Restore.Series]";
          _moverOperations.RestoreWatchedSeries();
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
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {

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