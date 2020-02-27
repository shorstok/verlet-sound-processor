using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class SpringLinkConstraint : ConstraintBase
    {
        
        [DataMember]
        public int LinkedEntityId { get; set; }


        [DataMember] public double KElasticity { get; set; } = 1e-8;

        public static SpringLinkConstraint LinkTo(VEntity source, VEntity target)
        {
            return new SpringLinkConstraint
            {
                LinkedEntityId = target.Id,
                Source = source,
                Target = target
            };
        }

        protected override void RestoreEntityReferencesInternal(VEntity container, Dictionary<int, VEntity> entities)
        {
            Target = entities[LinkedEntityId];
            base.RestoreEntityReferencesInternal(container, entities);
        }

        public override void Satisfy(VEntity one,
            VEntity other,
            ConstraintBase oneConstraint)
        {
            var c1 = oneConstraint as SpringLinkConstraint;

            if(c1 == null)    
                return;

            if(other.Id!=c1.LinkedEntityId)
                return;
            
            var distanceSquared = (one.X - Target.X) * (one.X - Target.X) +
                                  (one.Y - Target.Y) * (one.Y - Target.Y);

            //How much do we need to push balls apart
            var excessLength = (0 - Math.Sqrt(distanceSquared)) * KElasticity;

            var nx = (one.X - other.X);
            var ny = (one.Y - other.Y);

            var l = Math.Sqrt(nx * nx + ny * ny);

            
            if(Math.Abs(l) < double.Epsilon)
                return;

            nx /= l;
            ny /= l;

            one.X += nx * excessLength / 2;
            one.Y += ny * excessLength / 2;

            other.X -= nx * excessLength / 2;
            other.Y -= ny * excessLength / 2;
        }

        public ConstraintBase SetElasticity(double value)
        {
            KElasticity = value;

            return this;
        }
    }
}