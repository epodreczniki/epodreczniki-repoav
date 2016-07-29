using System;
namespace MediaInfoWrapper
{
    public interface IMediaInfo:IDisposable
    {
        void Close();
        T Get<T>(Enum Param);
        T Get<T>(Enum Param, int number);
        string Inform();
        int Open(string FileName);
    }
}
