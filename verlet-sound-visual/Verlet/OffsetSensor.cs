using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class OffsetSensor : VEntity, ISensor
    {
        public double Signal { get; private set; }

        [DataMember] public double SensorNormalX { get; set; } = 0;

        [DataMember] public double SensorNormalY { get; set; } = 1;

        private double _x1,_y1;
        private double dirX,dirY;

        public void StartSignalCalulationStep()
        {
            
        }

        public override void Initialize(Dictionary<int, VEntity> entities)
        {
            _x1 = X;
            _y1 = Y;

            (dirX, dirY) = (-SensorNormalY, SensorNormalX);

            base.Initialize(entities);
        }

        public void FinishSignalCalculation()
        {
            Signal = (X-_x1) * dirX + (Y - _y1)*dirY;
        }
    }
}