using System.Text;

namespace Mover.Services
{
  public interface IFileOperations
  {
    void FileWriteAllText(string path, string contents, Encoding encoding);
  }
}