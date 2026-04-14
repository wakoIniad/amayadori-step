using TestSpace;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

Util util = new Util();
Func<int> rnd = Util.GetRandFuncFollowDiscreteDistribution(new float[]{
    1
});
Console.WriteLine(rnd);
Dictionary<int, int> count = new Dictionary<int, int>();
for(int i = 0;i < 100000;i ++)
{
    int val = rnd();
    count[val] = (count?.GetValueOrDefault(val)??0)+1;
}
foreach(int key in count.Keys)
{
    Console.WriteLine($"{key}: {count[key]}");
}
namespace TestSpace {
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Diagnostics;

    class Util
    {
        /*エイリアス法*/
        public static Func<int> GetRandFuncFollowDiscreteDistribution(float[] distribution)
        {
            Queue<float> dist = new Queue<float>(distribution);
            float min = distribution.Min();
            float sum = distribution.Sum();

            float unit = min-((float)(sum%min/(Math.Ceiling(sum/min)-1)) is var a && float.IsFinite(a) ? a : 0);

            List<(float,int)> dict = new List<(float, int)>();
            float[] tmp = new float[]{ dist.Dequeue(), 0 };
            int index = 0;
            while(tmp[0] > 0)
            {

                tmp[0] -= unit;
                dict.Add(
                (
                   tmp[0] >= 0 ? 1 : 1+tmp[0]/unit, index//, -1
                ));

                Console.WriteLine(tmp[0] > 0 ? 1 : 1+tmp[0]/unit +  "," + unit);

                if(tmp[0] <= 0)
                {
                    float use = dist.Count > 0 ? dist.Dequeue() : float.NegativeInfinity;
                    use += tmp[0];
                    tmp[0] = use; 
                    index ++;
                    //dict[dict.Count-1].Item3 = index;
                }
            }
            //for(int i = 0; i < Math.Ceiling(sum/min);i++)
            //{
            //    tmp[0] -= min;
            //    if(tmp[0] > 0)
            //    {
            //        
            //    }
            //};

            System.Random rand = new System.Random();
            return () =>
            {
                (float, int) use = dict[rand.Next(0, dict.Count())];
                if(rand.NextDouble() < use.Item1)
                {
                    return use.Item2;
                } return use.Item2+1;
            };
        }
    }
};