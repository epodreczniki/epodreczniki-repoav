using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;
namespace MediaInfoWrapper
{
    public class MediaInfoBase
    {

        public virtual string Get(StreamKind kind, int number, string parameter, InfoKind infoKind1, InfoKind infoKind2)
        {
            throw new Exception("This method Get from class MediaInfoBase was never meant to launch directly!");
        }

        public T Get<T>(Enum Param, int number = 0)
        {
            Type TargetType = typeof(T);
            StreamKind kind = StreamKind.General;
            String parameter = Param.ToString();
            Type ParamType = Param.GetType();
            if (ParamType == typeof(Generalinfo))
                kind = StreamKind.General;
            else
                if (ParamType == typeof(Generalinfo))
                    kind = StreamKind.General;
                else
                    if (ParamType == typeof(Videoinfo))
                        kind = StreamKind.Video;
                    else
                        if (ParamType == typeof(Audioinfo))
                            kind = StreamKind.Audio;
                        else
                            if (ParamType == typeof(Textinfo))
                                kind = StreamKind.Text;
                            else
                                if (ParamType == typeof(Chaptersinfo))
                                    kind = StreamKind.Chapters;
                                else
                                    if (ParamType == typeof(Imageinfo))
                                        kind = StreamKind.Image;
                                    else throw new Exception("Unknown Enum " + ParamType);

            var val = this.Get(kind, number, parameter, InfoKind.Text, InfoKind.Name);
            if (TargetType == typeof(byte) || TargetType == typeof(sbyte) || TargetType == typeof(short) || TargetType == typeof(ushort) || TargetType == typeof(int) || TargetType == typeof(uint) || TargetType == typeof(long) || TargetType == typeof(ulong) || TargetType == typeof(decimal) || TargetType == typeof(double) || TargetType == typeof(float))
                val = val.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            var result = default(T);
            try { result = (T)Convert.ChangeType(val, TargetType); }
            catch { };

            return result;
        }

        public T Get<T>(Enum Param)
        {
            return Get<T>(Param, 0);
        }

    }

    public class MediaInfo32 : MediaInfoBase, IDisposable, IMediaInfo
    {
        const string DllName = "MediaInfo32.dll";
        private IntPtr Handle;
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_New();
        [DllImport(DllName)]
        public static extern void MediaInfo_Delete(IntPtr Handle);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);
        [DllImport(DllName)]
        public static extern void MediaInfo_Close(IntPtr Handle);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_State_Get(IntPtr Handle);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);
        public MediaInfo32()
        {
            this.Handle = MediaInfo32.MediaInfo_New();
        }
        ~MediaInfo32()
        {
            Dispose();
        }
        public int Open(string FileName)
        {
            int result;
            try
            {
                result = (int)MediaInfo32.MediaInfo_Open(this.Handle, FileName);
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        public void Close()
        {
            MediaInfo32.MediaInfo_Close(this.Handle);
        }
        public string Inform()
        {
            return Marshal.PtrToStringUni(MediaInfo32.MediaInfo_Inform(this.Handle, (IntPtr)0));
        }
        public override string Get(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo, InfoKind KindOfSearch)
        {
            return Marshal.PtrToStringUni(MediaInfo32.MediaInfo_Get(this.Handle, (IntPtr)((long)StreamKind), (IntPtr)StreamNumber, Parameter, (IntPtr)((long)KindOfInfo), (IntPtr)((long)KindOfSearch)));
        }
        public string Get(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo)
        {
            return Marshal.PtrToStringUni(MediaInfo32.MediaInfo_GetI(this.Handle, (IntPtr)((long)StreamKind), (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)((long)KindOfInfo)));
        }
        public string Option(string Option, string Value)
        {
            return Marshal.PtrToStringUni(MediaInfo32.MediaInfo_Option(this.Handle, Option, Value));
        }
        public int State_Get()
        {
            return (int)MediaInfo32.MediaInfo_State_Get(this.Handle);
        }
        public int Count_Get(StreamKind StreamKind, int StreamNumber)
        {
            return (int)MediaInfo32.MediaInfo_Count_Get(this.Handle, (IntPtr)((long)StreamKind), (IntPtr)StreamNumber);
        }
        public string Get(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo)
        {
            return this.Get(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name);
        }
        public string Get(StreamKind StreamKind, int StreamNumber, string Parameter)
        {
            return this.Get(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name);
        }
        public string Get(StreamKind StreamKind, int StreamNumber, int Parameter)
        {
            return this.Get(StreamKind, StreamNumber, Parameter, InfoKind.Text);
        }

        public string Option(string Option_)
        {
            return this.Option(Option_, "");
        }
        public int Count_Get(StreamKind StreamKind)
        {
            return this.Count_Get(StreamKind, -1);
        }

        public void Dispose()
        {
            if (this.Handle != IntPtr.Zero)
            {
                MediaInfo32.MediaInfo_Delete(this.Handle);
            }

        }
    }

    public class MediaInfo64 : MediaInfoBase, IDisposable, IMediaInfo
    {
        const string DllName = "MediaInfo64.dll";
        private IntPtr Handle;
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_New();
        [DllImport(DllName)]
        public static extern void MediaInfo_Delete(IntPtr Handle);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);
        [DllImport(DllName)]
        public static extern void MediaInfo_Close(IntPtr Handle);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_State_Get(IntPtr Handle);
        [DllImport(DllName)]
        public static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);
        public MediaInfo64()
        {
            this.Handle = MediaInfo64.MediaInfo_New();
        }
        ~MediaInfo64()
        {
            Dispose();
        }
        public int Open(string FileName)
        {
            int result;
            try
            {
                result = (int)MediaInfo64.MediaInfo_Open(this.Handle, FileName);
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        public void Close()
        {
            MediaInfo64.MediaInfo_Close(this.Handle);
        }
        public string Inform()
        {
            return Marshal.PtrToStringUni(MediaInfo64.MediaInfo_Inform(this.Handle, (IntPtr)0));
        }
        public override string Get(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo, InfoKind KindOfSearch)
        {
            return Marshal.PtrToStringUni(MediaInfo64.MediaInfo_Get(this.Handle, (IntPtr)((long)StreamKind), (IntPtr)StreamNumber, Parameter, (IntPtr)((long)KindOfInfo), (IntPtr)((long)KindOfSearch)));
        }
        public string Get(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo)
        {
            return Marshal.PtrToStringUni(MediaInfo64.MediaInfo_GetI(this.Handle, (IntPtr)((long)StreamKind), (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)((long)KindOfInfo)));
        }
        public string Option(string Option, string Value)
        {
            return Marshal.PtrToStringUni(MediaInfo64.MediaInfo_Option(this.Handle, Option, Value));
        }
        public int State_Get()
        {
            return (int)MediaInfo64.MediaInfo_State_Get(this.Handle);
        }
        public int Count_Get(StreamKind StreamKind, int StreamNumber)
        {
            return (int)MediaInfo64.MediaInfo_Count_Get(this.Handle, (IntPtr)((long)StreamKind), (IntPtr)StreamNumber);
        }
        public string Get(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo)
        {
            return this.Get(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name);
        }
        public string Get(StreamKind StreamKind, int StreamNumber, string Parameter)
        {
            return this.Get(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name);
        }
        public string Get(StreamKind StreamKind, int StreamNumber, int Parameter)
        {
            return this.Get(StreamKind, StreamNumber, Parameter, InfoKind.Text);
        }

        public string Option(string Option_)
        {
            return this.Option(Option_, "");
        }
        public int Count_Get(StreamKind StreamKind)
        {
            return this.Count_Get(StreamKind, -1);
        }

        public void Dispose()
        {
            if (this.Handle != IntPtr.Zero)
            {
                MediaInfo64.MediaInfo_Delete(this.Handle);
            }

        }
    }

    public static class MediaInfoFactory
    {

        public static IMediaInfo GetInstance()
        {
            IMediaInfo result = null;
            if (IntPtr.Size == 4)
                result = new MediaInfo32();
            else
                result = new MediaInfo64();
            return result;
        }
    }
}
