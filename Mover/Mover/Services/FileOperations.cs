using System.IO;
using System.Text;

namespace Mover.Services
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

    public bool DirectoryExists(string path)
    {
      return Directory.Exists(path);
    }

    public DirectoryInfo CreateDirectory(string path)
    {
      return Directory.CreateDirectory(path);
    }
  }
}