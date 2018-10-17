using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public enum Colour : byte
    {
        Red = 0,
        Green = 1,
        Yellow = 2
    }
    public class Apple
    {
        public Colour Colour { get; set; }
        private int taste;

        public Apple(Colour colour, int taste)
        {
            this.Colour = colour;
            this.taste = taste;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            var serializer = new MySerializer();
            var apple = new Apple(Colour.Red, 5);

            var binary = serializer.Serialize(apple);
            var v = serializer.Deserialize(binary);
        }
    }
}
