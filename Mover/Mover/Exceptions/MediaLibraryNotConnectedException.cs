using System;

namespace Mover.Exceptions
{
  public class MediaLibraryNotConnectedException : Exception
  {
    public MediaLibraryNotConnectedException(string message) : base(message)
    {

    }
  }
}