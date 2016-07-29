using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSNC.Multimedia.Tools
{
    public class DictionaryEx<TKey, TValue> : Dictionary<TKey, TValue>
    {

        public DictionaryEx()
        {
        }

        public DictionaryEx(Dictionary<TKey, TValue> parent)
        {

            foreach (var pair in parent)
            {

                this.Add(pair.Key, pair.Value);

            }

        }

        public new void Add(TKey key, TValue val)
        {
            if (!this.ContainsKey(key))
                base.Add(key, val);
            else
                this[key] = val;
        }

        public new TValue this[TKey key]
        {
            get
            {
                TValue local;
                if (base.TryGetValue(key, out local))
                {
                    return local;
                }
                return default(TValue);
            }
            set
            {
                base[key] = value;
            }
        }
    }
}

