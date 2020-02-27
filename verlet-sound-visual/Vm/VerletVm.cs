using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NAudio.Wave;
using Newtonsoft.Json;
using verlet_sound_visual.Annotations;
using verlet_sound_visual.Config;
using verlet_sound_visual.Verlet;

namespace verlet_sound_visual.Vm
{
    public class VerletVm : INotifyPropertyChanged
    {
        private VerletModel _currentModel;
        private string _modelFilename;

        public ICommand LocateSourceCommand { get; }
        public ICommand PlayProcessedSound { get; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public string SourceFilePath
        {
            get => UiSettings.Current.SourceFileName;
            set
            {
                if (value == UiSettings.Current.SourceFileName) return;
                UiSettings.Current.SourceFileName = value;
                UiSettings.Current.Save();
                OnPropertyChanged();
            }
        }

        public VerletModel CurrentModel
        {
            get => _currentModel;
            set
            {
                if (Equals(value, _currentModel)) return;
                _currentModel = value;
                OnPropertyChanged();
            }
        }

        public void SaveModel()
        {
            return;

            if(_modelFilename == null || CurrentModel == null)
                return;

            File.WriteAllText(_modelFilename,
                JsonConvert.SerializeObject(CurrentModel,
                    JsonSerializers.IndentedAutotypeIgnoreNull));
        }


        public VerletVm()
        {
            _modelFilename = Path.Combine(GlobalSettings.GetOrCreateSettingsPath(), "model.json");

            CurrentModel = !File.Exists(_modelFilename)
                ? VerletModel.CreateSpringLink()
                : JsonConvert.DeserializeObject<VerletModel>(File.ReadAllText(_modelFilename),
                    JsonSerializers.IndentedAutotypeIgnoreNull);

            PlayProcessedSound = new DelegateCommand(t=>true, PlaySoundAsync);
        }

        private void PlaySoundAsync(object obj)
        {
            var waveOut = new WaveOutEvent
            {                
                Volume = 0.6f,
            };

            var backup = CurrentModel;

            CurrentModel = CurrentModel.Clone();
            
            var waveProvider = new VerletWaveProvider(SourceFilePath, CurrentModel);
            
            waveOut.Init(waveProvider);

            waveOut.Play();

            waveOut.PlaybackStopped+= delegate { CurrentModel = backup.Clone(); };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
