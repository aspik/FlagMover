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
  }
}