using System;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class DeltaMovementSensor : VEntity, ISensor
    {
        public double Signal { get; private set; }

        private double _x1,_y1;

        public void StartSignalCalulationStep()
        {
            _x1 = X;
            _y1 = Y;
        }

        public void FinishSignalCalculation()
        {
            Signal = Math.Sqrt((X - _x1) * (X - _x1) + (Y - _y1) * (Y - _y1));
        }
    }
}