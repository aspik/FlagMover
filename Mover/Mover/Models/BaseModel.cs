using System;
using System.Collections.Generic;
using FlagMover.Services;
using MediaPortal.Common.General;
using MediaPortal.UI.Presentation.DataObjects;

namespace FlagMover.Models
{
  public abstract class BaseModel
  {
    protected readonly IMediaPortalServices _mediaPortalServices;
    protected readonly IMoverOperations _moverOperations;
    protected readonly IFileOperations _fileOperations;

    private readonly AbstractProperty _statusProperty = new WProperty(typeof(string), String.Empty);

    protected ItemsList _directoryTree;
    protected string _selectedPath;

    protected BaseModel()
    {
      _mediaPortalServices = new MediaPortalServices();
      _moverOperations = new MoverOperations(_mediaPortalServices, new FileOperations());
      _fileOperations = new FileOperations();
    }

    protected BaseModel(IMediaPortalServices mediaPortalServices, IMoverOperations moverOperations, IFileOperations fileOperations)
    {
      _mediaPortalServices = mediaPortalServices;
      _moverOperations = moverOperations;
      _fileOperations = fileOperations;
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

    public ItemsList DirectoryTree
    {
      get { return _directoryTree; }
    }

    protected void RefreshDirectoryTree(ItemsList items, string path)
    {
      items.Clear();
      bool isRoot = path.Equals(".", StringComparison.InvariantCulture);

      IDictionary<string, string> directories = isRoot ? _fileOperations.GetLogicalDrivers() : _fileOperations.GetDirectories(path);

      foreach (KeyValuePair<string, string> directory in directories)
      {
        TreeItem item = new TreeItem("Name", directory.Key);
        item.SetLabel("Path", directory.Value);
        item.SelectedProperty.Attach(OnTreePathSelectionChanged);
        item.AdditionalProperties["Expansion"] = new ExpansionHelper(item, this);
        items.Add(item);
      }
      items.FireChange();
    }

    private void OnTreePathSelectionChanged(AbstractProperty property, object oldValue)
    {
      _selectedPath = FindSelectedPath(_directoryTree);
    }

    private string FindSelectedPath(ItemsList items)
    {
      foreach (TreeItem treeItem in items)
      {
        if (treeItem.Selected)
        {
          return treeItem.Labels["Path"].Evaluate();
        }

        string path = FindSelectedPath(treeItem.SubItems);
        if (path != null)
        {
          return path;
        }
      }
      return null;
    }

    private void RefreshOrClearSubTreeItems(TreeItem pathItem, bool clearSubItems)
    {
      if (clearSubItems)
      {
        pathItem.SubItems.Clear();
        pathItem.SubItems.FireChange();
      }
      else
      {
        RefreshDirectoryTree(pathItem.SubItems, pathItem["Path"]);
      }
    }

    private class ExpansionHelper
    {
      private AbstractProperty _isExpandedProperty = new WProperty(typeof(bool), false);
      private BaseModel _parent;
      private TreeItem _directoryItem;

      public ExpansionHelper(TreeItem directoryItem, BaseModel parent)
      {
        _parent = parent;
        _directoryItem = directoryItem;
        _isExpandedProperty.Attach(OnExpandedChanged);
      }

      private void OnExpandedChanged(AbstractProperty property, object oldvalue)
      {
        bool expanded = (bool)property.GetValue();
        _parent.RefreshOrClearSubTreeItems(_directoryItem, !expanded);
      }

      public AbstractProperty IsExpandedProperty
      {
        get { return _isExpandedProperty; }
      }

      public bool IsExpanded
      {
        get { return (bool)_isExpandedProperty.GetValue(); }
        set { _isExpandedProperty.SetValue(value); }
      }
    }
  }
}