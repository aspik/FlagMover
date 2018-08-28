using System;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.Common.General;
using MediaPortal.Common.Threading;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;
using Mover.Services;

namespace Mover.Models
{
  public class BackupModel : IWorkflowModel
  {
    private static readonly Guid BACKUP_MODEL_ID = new Guid("8621AA8C-509A-45FD-B47D-1AE9DDDB46DD");

    private readonly IMediaPortalServices _mediaPortalServices;
    private readonly IMoverOperations _moverOperations;

    private readonly AbstractProperty _statusProperty = new WProperty(typeof(string), string.Empty);

    public BackupModel()
    {
      _mediaPortalServices = new MediaPortalServices();
      _moverOperations = new MoverOperations(_mediaPortalServices, new FileOperations());
    }

    public BackupModel(IMediaPortalServices mediaPortalServices, IMoverOperations moverOperations)
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

    public void BackupLibrary()
    {
      try
      {
        IThreadPool threadPool = _mediaPortalServices.GetThreadPool();
        threadPool.Add(() =>
        {
          Status = "[Backup.Movies]";
          _moverOperations.BackupMovies();
          Status = "[Backup.Series]";
          _moverOperations.BackupSeries();
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
      get { return BACKUP_MODEL_ID; }
    }
  }
} 