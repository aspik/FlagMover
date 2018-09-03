using System;

namespace FlagMover.Exceptions
{
  public class MediaLibraryNotConnectedException : Exception
  {
    public MediaLibraryNotConnectedException(string message) : base(message)
    {

    }
  }
}