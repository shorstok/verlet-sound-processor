using System;
using NAudio.Wave;

namespace verlet_sound_visual.Verlet
{
    internal class VerletWaveProvider : WaveProvider32
    {
        private readonly string _filename;
        private readonly VerletModel _model;
        private readonly AudioFileReader _reader;

        public VerletWaveProvider(string filename, VerletModel model)
        {
            _filename = filename;
            _model = model;

            _reader = new AudioFileReader(filename);

            SetWaveFormat(_reader.WaveFormat.SampleRate,_reader.WaveFormat.Channels);
        }

        float[] _input = new float[0];

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            if(_input.Length!=buffer.Length)
                _input = new float[buffer.Length];

            var nread = _reader.Read(_input, offset, sampleCount);

            if (nread == 0)
                return 0;

            return Process(_input, buffer, offset, sampleCount, nread);
        }

        

        private int Process(float[] input, float[] output, int offset, int sampleCount, int nread)
        {
            float max = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                output[i + offset] = (float) _model.Step(input[i]);

                if (output[i + offset] > max)
                    max = output[i + offset];
            }

            return sampleCount;
        }
    }
}