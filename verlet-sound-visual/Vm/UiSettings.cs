using System.Runtime.Serialization;
using verlet_sound_visual.Config;

namespace verlet_sound_visual.Vm
{
    [DataContract]
    public class UiSettings : GlobalSettings<UiSettings>
    {
        [DataMember]
        public string SourceFileName { get; set; }
    }
}