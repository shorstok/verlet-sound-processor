using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class FixedSpringConstraint : ConstraintBase,IUndirectionalConstraint
    {
        
        [DataMember] public double KElasticity { get; set; } = 1e-5;
        [DataMember] public double X0 { get; set; }
        [DataMember] public double Y0 { get; set; }


        protected override void RestoreEntityReferencesInternal(VEntity container, Dictionary<int, VEntity> entities)
        {
            X0 = container.X;
            Y0 = container.Y;
            
            base.RestoreEntityReferencesInternal(container, entities);
        }

        public override void Satisfy(VEntity one,
            VEntity other,
            ConstraintBase oneConstraint)
        {
            var c1 = oneConstraint as FixedSpringConstraint;

            if(c1 == null)    
                return;

            var distanceSquared = (one.X - X0) * (one.X - X0) +
                                  (one.Y - Y0) * (one.Y - Y0);

            //How much do we need to push balls apart
            var excessLength = (- Math.Sqrt(distanceSquared)) * KElasticity;

            var nx = (one.X - X0);
            var ny = (one.Y - Y0);

            var l = Math.Sqrt(nx * nx + ny * ny);

            if(Math.Abs(l) < double.Epsilon)
                return;
            
            nx /= l;
            ny /= l;

            one.X += nx * excessLength / 2;
            one.Y += ny * excessLength / 2;
        }

        public ConstraintBase SetElasticity(double value)
        {
            KElasticity = value;

            return this;
        }
    }
}