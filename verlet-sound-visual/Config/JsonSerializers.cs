using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace verlet_sound_visual.Config
{
    public static class JsonSerializers
    {
        public static readonly JsonSerializerSettings IndentedAutotypeIgnoreNull = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };
    }
}
