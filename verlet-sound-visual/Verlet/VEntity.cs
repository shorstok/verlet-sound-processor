using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Documents;
using System.Xml.Serialization;
using Newtonsoft.Json;
using verlet_sound_visual.Annotations;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class VEntity : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public double X
        {
            get => _x;
            set
            {
                if (value.Equals(_x)) return;
                _x = value;
                OnPropertyChanged();
            }
        }
        [DataMember]
        public double Y
        {
            get => _y;
            set
            {
                if (value.Equals(_y)) return;
                _y = value;

                OnPropertyChanged();
            }
        }

        [DataMember]
        public List<ConstraintBase> Constraints { get; set; } = new List<ConstraintBase>();


        private Dictionary<Type, ConstraintBase> _constraints = null;
        public TConstraint GetConstraint<TConstraint>() where TConstraint : ConstraintBase
        {
            if (_constraints == null)
                _constraints = Constraints.ToDictionary(t => t.GetType(), t => t);

            return (_constraints.TryGetValue(typeof(TConstraint), out var constraint) ? constraint : null) as TConstraint;
        }

        [DataMember]
        public double XPrev { get; set; }
        [DataMember]
        public double YPrev { get; set; }

        public double Dx => X - XPrev;
        public double Dy => Y - YPrev;

        public double SquaredDistanceTo(VEntity other) => (X - other.X) * (X - other.X) +
                                                          (Y - other.Y) * (Y - other.Y);

        public void Move()
        {
            (X, Y, XPrev, YPrev) = (X * 2 - XPrev, Y * 2 - YPrev, X,Y);
        }
        
        public VEntity AsMovingInDirection(double dx, double dy)
        {
            XPrev = X - dx;
            YPrev = Y - dy;

            return this;
        }

        public static TEntity PlaceAt<TEntity>(double x, double y, VerletModel model) where TEntity : VEntity, new()
        {
            return new TEntity
            {
                Id = model.GeneratorSeed++,
                X = x,
                XPrev = x,
                Y = y,
                YPrev = y
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public VEntity AddConstraint<T>() where T : ConstraintBase, new()
        {
            Constraints.Add(new T());

            return this;
        }

        public VEntity AddConstraint(ConstraintBase constraint)
        {
            Constraints.Add(constraint);

            return this;
        }

        public virtual void Initialize(Dictionary<int, VEntity> entities)
        {

        }
    }
}
