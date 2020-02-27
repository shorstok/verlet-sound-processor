using System;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public class InelasticBallConstraint : ConstraintBase
    {
        [DataMember]
        public double Radius { get; set; } = 5;

        public override void Satisfy(VEntity one,
            VEntity other,
            ConstraintBase oneConstraint)
        {
            var c1 = oneConstraint as InelasticBallConstraint;
            var c2 = other.GetConstraint<InelasticBallConstraint>();
            
            if(c1 == null || c2 == null)
                return;

            var distanceSquared = (one.X - other.X) * (one.X - other.X) +
                                  (one.Y - other.Y) * (one.Y - other.Y);

            var minRadiiSquared = (c1.Radius + c2.Radius) * (c1.Radius + c2.Radius);

            if (distanceSquared > minRadiiSquared)
                return;

            //How much do we need to push balls apart
            var excessLength = Math.Sqrt(minRadiiSquared) - Math.Sqrt(distanceSquared);

            var nx = (one.X - other.X);
            var ny = (one.Y - other.Y);

            var l = Math.Sqrt(nx * nx + ny * ny);

            nx /= l;
            ny /= l;

            one.X += nx * excessLength / 2;
            one.Y += ny * excessLength / 2;

            one.XPrev -= nx * excessLength / 2;
            one.YPrev -= ny * excessLength / 2;

            other.X -= nx * excessLength / 2;
            other.Y -= ny * excessLength / 2;

            other.XPrev += nx * excessLength / 2;
            other.YPrev += ny * excessLength / 2;
        }
    }
}