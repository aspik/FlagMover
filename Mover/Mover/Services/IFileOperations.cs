using System.IO;
using System.Text;

namespace Mover.Services
{
  public interface IFileOperations
  {
    void FileWriteAllText(string path, string contents, Encoding encoding);
    bool FileExists(string path);
    string FileReadAllText(string file);
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
  }
}