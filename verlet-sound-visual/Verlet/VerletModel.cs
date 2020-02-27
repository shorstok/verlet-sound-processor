using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using verlet_sound_visual.Annotations;
using verlet_sound_visual.Config;

namespace verlet_sound_visual.Verlet
{
    public partial class VerletModel : INotifyPropertyChanged
    {
        [DataMember]
        public List<VEntity> Entities { get; protected set; } = new List<VEntity>();

        [DataMember]
        public double XSpan
        {
            get
            {
                if (_xSpan == null)
                    _xSpan = Entities.DefaultIfEmpty().Max(e => e.X);

                return _xSpan ?? 0;
            }
            set
            {
                if (value.Equals(_xSpan)) return;
                _xSpan = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public double YSpan
        {
            get
            {
             
                if (_ySpan == null)
                    _ySpan = Entities.DefaultIfEmpty().Max(e => e.Y);

                return _ySpan ?? 0;
            }
            set
            {
                if (value.Equals(_ySpan)) return;
                _ySpan = value;
                OnPropertyChanged();
            }
        }

        public double Energy
        {
            get => _energy;
            set
            {
                if (value.Equals(_energy)) return;
                _energy = value;
                OnPropertyChanged();
            }
        }

        public int GeneratorSeed { get; set; }

        protected IActuator _actuator = null;
        protected ISensor _sensor = null;

        private const double spanDefault = 600;

        private double? _xSpan = spanDefault;
        private double? _ySpan = spanDefault;
        private double _energy;

        
        private void Initialize()
        {
            var entities = Entities.ToDictionary(t => t.Id, t=>t);

            foreach (var entity in Entities)
            {
                entity.Initialize(entities);

                foreach (var constraint in entity.Constraints) 
                    constraint.RestoreEntityReferences(entity, entities);
            }
        }

        public double Step(double input)
        {
            if (_actuator == null)
                _actuator = Entities.OfType<IActuator>().FirstOrDefault();
            if (_sensor == null)
                _sensor = Entities.OfType<ISensor>().FirstOrDefault();

            _actuator?.ApplyInput(input);
            _sensor?.StartSignalCalulationStep();
            
            //CollideWithWalls();
            
            foreach (var one in Entities)
            {
                foreach (var oneConstraint in one.Constraints)
                {
                    if (oneConstraint is IUndirectionalConstraint)
                    {
                        oneConstraint.Satisfy(one, null, oneConstraint);
                        continue;
                    }

                    foreach (var other in Entities)
                    {
                        if (ReferenceEquals(one, other))
                            continue;

                        oneConstraint.Satisfy(one, other, oneConstraint);
                    }
                }
            }

            foreach (var entity in Entities)
                entity.Move();

            _sensor?.FinishSignalCalculation();

            var energy = 0.0;

            foreach (var entity in Entities) 
                energy += (entity.Dx * entity.Dx + entity.Dy * entity.Dy) / 2;

            Energy = energy;
            
            return _sensor?.Signal ?? 0;
        }


        public TEntity Add<TEntity>(TEntity entity) where TEntity : VEntity
        {
            Entities.Add(entity);

            return entity;
        }

        private void CollideWithWalls()
        {
            foreach (var entity in Entities)
            {
                var vx = entity.X - entity.XPrev;
                var vy = entity.Y - entity.YPrev;

                if (entity.Y < 0)
                {
                    entity.Y = 0;
                    entity.YPrev = entity.Y + vy;
                }
                if (entity.X < 0)
                {
                    entity.X = 0;
                    entity.XPrev = entity.X + vx;
                }

                if (entity.Y > YSpan)
                {
                    entity.Y = YSpan;
                    entity.YPrev = entity.Y + vy;
                }
                if (entity.X > XSpan)
                {
                    entity.X = XSpan;
                    entity.XPrev = entity.X + vx;
                }
            }

        }

        public VerletModel Clone()
        {
            var clone = JsonConvert.DeserializeObject<VerletModel>(JsonConvert.SerializeObject(this,
                    JsonSerializers.IndentedAutotypeIgnoreNull),
                JsonSerializers.IndentedAutotypeIgnoreNull);

            clone.Initialize();

            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}