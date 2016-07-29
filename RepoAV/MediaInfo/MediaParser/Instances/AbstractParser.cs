using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaInfoWrapper;

namespace PSNC.Multimedia.Instances
{
    public class AbstractParser
    {

        private IMediaInfo _mediaInfo;

        public IMediaInfo MediaInfo
        {
            get
            {
                return _mediaInfo;
            }
            set
            {
                _mediaInfo = value;
            }
        }

    }
}
