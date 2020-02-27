using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using verlet_sound_visual.Annotations;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public abstract class ConstraintBase: INotifyPropertyChanged
    {
        public abstract void Satisfy(VEntity one, VEntity other, ConstraintBase oneConstraint);

        private VEntity _target;
        private VEntity _source;

        public VEntity Target
        {
            get => _target;
            set
            {
                if (Equals(value, _target)) return;
                _target = value;
                OnPropertyChanged();
            }
        }

        public VEntity Source
        {
            get => _source;
            set
            {
                if (Equals(value, _source)) return;
                _source = value;
                OnPropertyChanged();
            }
        }

        public void RestoreEntityReferences(VEntity container, Dictionary<int, VEntity> entities)
        {
            if(Source!= null)
                return;

            Source = container;
            RestoreEntityReferencesInternal(container,entities);
        }

        protected virtual void RestoreEntityReferencesInternal(VEntity container, Dictionary<int, VEntity> entities)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}