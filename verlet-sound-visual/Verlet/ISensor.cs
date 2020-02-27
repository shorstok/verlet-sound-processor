namespace verlet_sound_visual.Verlet
{
    public interface ISensor
    {
        double Signal { get; }
        void StartSignalCalulationStep();
        void FinishSignalCalculation();
    }
}