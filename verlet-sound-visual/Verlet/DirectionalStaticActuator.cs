using System.Collections.Generic;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class DirectionalStaticActuator : VEntity, IActuator
    {
        [DataMember] public double ActuatorDirectionX { get; set; } = 1;

        [DataMember] public double ActuatorDirectionY { get; set; } = 0;

        [DataMember] public double Amplitude { get; set; } = 10;

        public double OriginX;
        public double OriginY;

        public override void Initialize(Dictionary<int, VEntity> entities)
        {
            OriginX = X;
            OriginY = Y;

            base.Initialize(entities);
        }

        public void ApplyInput(double input)
        {
            X = OriginX + input * ActuatorDirectionX * Amplitude;
            Y = OriginY + input * ActuatorDirectionY * Amplitude;
        }
    }
}