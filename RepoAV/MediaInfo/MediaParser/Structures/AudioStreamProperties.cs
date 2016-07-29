using System;
using PSNC.Multimedia.Tools;

namespace PSNC.Multimedia
{
    public struct SimpleAudioProperties
    {
        public int _index;
        public int _bitrate;
        public int _samplesPerSec;
        public int _bitsPerSample;
        public WaveFormatTag _waveFormat;
        public MPEG_LAYER _mpegLayer;
        public bool _isVbr;
        public string _codec;
        public ushort _codecId;
        public byte[] _codecInfo;
    }

    public class AudioStreamProperties
    {
        XmlTools.AudioCoding.Values coding = XmlTools.AudioCoding.Values.Unknown;
        private String _codecName = String.Empty;
        private int _index = 0;
        SimpleAudioProperties sap;

        public AudioStreamProperties()
        {
        }


        public AudioStreamProperties(SimpleAudioProperties sap)
        {
            this.sap = sap;
        }

        public AudioStreamProperties(
                int index,
                int bitrate,
                int samplesPerSec,
                int bitsPerSample,
                WaveFormatTag waveFormat,
                MPEG_LAYER mpegLayer,
                bool isVbr,
                string codec
            )
        {
            _index = index;
            sap._index = index;
            sap._bitrate = bitrate;
            sap._samplesPerSec = samplesPerSec;
            sap._bitsPerSample = bitsPerSample;
            sap._waveFormat = waveFormat;
            sap._mpegLayer = mpegLayer;
            sap._isVbr = isVbr;
            sap._codec = codec;
        }

        public XmlTools.AudioCoding.Values Coding
        {
            get { return coding; }
            set { coding = value; }
        }


        public int Bitrate
        {
            get
            {
                return sap._bitrate;
            }
            set
            {
                sap._bitrate = value;
            }
        }

        public int SamplesPerSec
        {
            get { return sap._samplesPerSec; }
        }

        public int BitsPerSample
        {
            get { return sap._bitsPerSample; }
        }

        public WaveFormatTag WaveFormat
        {
            get { return sap._waveFormat; }
        }

        public MPEG_LAYER Layer
        {
            get { return sap._mpegLayer; }
        }

        public bool IsVbr
        {
            get { return sap._isVbr; }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public string Codec
        {
            get
            {
                if (!String.IsNullOrEmpty(CodecName))
                    return CodecName;
                else
                    return sap._codec;
            }
        }

        public String CodecName
        {
            get { return _codecName; }
            set { _codecName = value; }
        }


        public ushort CodecId
        {
            get { return sap._codecId; }
            set { sap._codecId = value; }
        }

        public byte[] CodecInfo
        {
            get { return sap._codecInfo; }
            set { sap._codecInfo = value; }
        }

    }
}
