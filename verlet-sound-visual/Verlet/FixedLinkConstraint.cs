using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class FixedLinkConstraint : ConstraintBase
    {
        [DataMember]
        public int LinkedEntityId { get; set; }
        [DataMember]
        public double Distance { get; set; }


        public static FixedLinkConstraint LinkTo(VEntity source, VEntity target)
        {
            return new FixedLinkConstraint
            {
                LinkedEntityId = target.Id,
            };
        }
        protected override void RestoreEntityReferencesInternal(VEntity container, Dictionary<int, VEntity> entities)
        {
            Target = entities[LinkedEntityId];
            Distance = Math.Sqrt(container.SquaredDistanceTo(Target));
            base.RestoreEntityReferencesInternal(container, entities);
        }
        
        public override void Satisfy(VEntity one,
            VEntity other,
            ConstraintBase oneConstraint)
        {
            var c1 = oneConstraint as FixedLinkConstraint;
            
            if(c1 == null)    
                return;

            if(other.Id!=c1.LinkedEntityId)
                return;

            var distanceSquared = (one.X - other.X) * (one.X - other.X) +
                                  (one.Y - other.Y) * (one.Y - other.Y);

            var minRadiiSquared = (c1.Distance * c1.Distance);

            //How much do we need to push balls apart
            var excessLength = Math.Sqrt(minRadiiSquared) - Math.Sqrt(distanceSquared);

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
    }
}