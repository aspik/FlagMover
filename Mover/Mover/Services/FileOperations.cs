using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlagMover.Services
{
  public class FileOperations : IFileOperations
  {
    public void FileWriteAllText(string path, string contents, Encoding encoding)
    {
      File.WriteAllText(path, contents, encoding);
    }

    public bool FileExists(string path)
    {
      return File.Exists(path);
    }

    public string FileReadAllText(string path)
    {
      return File.ReadAllText(path);
    }

    public IDictionary<string, string> GetLogicalDrivers()
    {
      IDictionary<string, string> drivers = new Dictionary<string, string>();
      foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
      {
        if ((driveInfo.DriveType == DriveType.Fixed || driveInfo.DriveType == DriveType.Removable) && driveInfo.IsReady)
        {
          drivers.Add("[" + driveInfo.Name + "]"+ " " +driveInfo.VolumeLabel, driveInfo.Name);
        }
      }

      return drivers;
    }

    public IDictionary<string, string> GetDirectories(string path)
    {
      IDictionary<string, string> directories = new Dictionary<string, string>();
      DirectoryInfo directoryInfo = new DirectoryInfo(path);
      foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
      {
        if (directory.Attributes == FileAttributes.Directory)
        {
          directories.Add(directory.Name, directory.FullName);
        }
      }

      return directories;
    }
  }
}