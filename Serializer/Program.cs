using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    class Program
    {
        public enum Colour : byte
        {
            Red = 0,
            Green = 1,
            Yellow = 2
        }

        static void Main(string[] args)
        {
            //collections
            var serializer = new MySerializer();
            int[][]arr = new int[2][];
            arr[0] = new int[2];
            arr[1] = new int[3];

            
            var binary = serializer.Serialize(arr);
            var v = serializer.Deserialize(binary);
        }
    }
}
