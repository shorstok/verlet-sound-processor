using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace verlet_sound_visual.Verlet
{
    [DataContract]
    public partial class VerletModel : INotifyPropertyChanged
    {
        public static VerletModel CreateMidWeightedSpring()
        {
            var result =new VerletModel();
            var r = new Random();

            var actuator =result.Add(VEntity.PlaceAt<DirectionalStaticActuator>(10, spanDefault/2,result));
            
            var mid = result.Add(VEntity.PlaceAt<VEntity>(
                spanDefault/2,
                spanDefault/2,
                result));

            var sensor = result.Add(VEntity.PlaceAt<OffsetSensor>(
                spanDefault-10,
                spanDefault/2+1,
                result));

            actuator.AddConstraint(ElasticLinkConstraint.LinkTo(actuator,mid).SetElasticity(1e-2));
            mid.AddConstraint(ElasticLinkConstraint.LinkTo(mid,sensor).SetElasticity(1e-2));
            
            sensor.AddConstraint(new FixedSpringConstraint().SetElasticity(1e-3));

            var wings = (
                one:
                result.Add(VEntity.PlaceAt<VEntity>(
                    spanDefault / 2,
                    spanDefault / 2-40,
                    result)),
                two:
                result.Add(VEntity.PlaceAt<VEntity>(
                    spanDefault / 2,
                    spanDefault / 2+40,
                    result)));

            wings.one.AddConstraint(ElasticLinkConstraint.LinkTo(wings.one,mid).SetElasticity(1e-2));
            wings.two.AddConstraint(ElasticLinkConstraint.LinkTo(wings.two,mid).SetElasticity(1e-2));

            result.Initialize();

            return result;
        }
        
        public static VerletModel CreateSimplest()
        {
            var result =new VerletModel();
            var r = new Random();

            var actuator =result.Add(VEntity.PlaceAt<DirectionalStaticActuator>(10, spanDefault/2,result));
            
            var sensor = result.Add(VEntity.PlaceAt<OffsetSensor>(
                spanDefault-10,
                spanDefault/2+1,
                result));

            actuator.AddConstraint(ElasticLinkConstraint.LinkTo(actuator,sensor).SetElasticity(1));
            sensor.AddConstraint(new FixedSpringConstraint().SetElasticity(1e-2));
            
            result.Initialize();

            return result;
        }

        public static VerletModel CreateSpringLink()
        {
            var result =new VerletModel();
            var r = new Random();

            result.Entities.Add(VEntity.PlaceAt<DirectionalStaticActuator>(10, spanDefault/2,result));

            int x = 30;
            int xc = 0;

            while (x < spanDefault - 30)
            {
                ++xc;
                result.Entities.Add(VEntity.PlaceAt<VEntity>(
                    x, 
                    spanDefault / 2 + (xc%2 == 1 ? -r.Next(110): r.Next(110)),
                    result));
                
                x += r.Next(60,100);
            }

            result.Entities.Add(VEntity.PlaceAt<OffsetSensor>(
                spanDefault-1,
                spanDefault/2,
                result).AddConstraint(
                new FixedSpringConstraint().
                    SetElasticity(1e-3)));

            for (int i = 0; i < result.Entities.Count-1; i++)
            {
                result.Entities[i].AddConstraint(
                    ElasticLinkConstraint.LinkTo(
                            result.Entities[i],
                            result.Entities[i+1]).
                        SetElasticity(1e-2));
            }

            result.Entities[result.Entities.Count/2].AddConstraint(
                new FixedSpringConstraint().
                    SetElasticity(1e-4));
            
            
            result.Initialize();

            return result;
        }
    }
}