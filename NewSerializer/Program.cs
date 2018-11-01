using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    public enum Colour
    {
        Red = 0,
        Green = 1,
        Yellow = 2
    }

    public class Apple
    {
        private int taste;
        public Colour Colour { get; set; }

        public object x;

        public Apple(Colour colour, int taste)
        {
            this.Colour = colour;
            this.taste = taste;
            x = this;
        }

        static int _cnt = 0;

        public Apple()
        {
            taste = --_cnt;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var stream = new MemoryStream();
            var serializer = new MyBinarySerializer();

            var lst = new List<object>();

            var apple = new Apple(Colour.Green, 4);
            lst.Add(new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
            lst.Add(apple);
            lst.Add(ConsoleColor.Yellow);
            lst.Add("abcd");
            lst.Add(4321);

            var x = new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } };

            //Apple[] apples = new Apple[2] { new Apple(Colour.Yellow, 9), new Apple(Colour.Red, 10) };

            var l = new LinkedList<Apple>();
            var app1 = new Apple(Colour.Yellow, 9);
            var app2 = new Apple(Colour.Green, 7);
            var app3 = new Apple(Colour.Yellow, 5);
            var app4 = new Apple(Colour.Red, 8);
            var app5 = new Apple(Colour.Yellow, 2);
            var n1 = l.AddFirst(app1);
            var n2 = l.AddAfter(n1, app3);
            var n3 = l.AddBefore(n2, app2);
            var n4 = l.AddAfter(n3, app4);
            var n5 = l.AddAfter(n4, app5);
            lst.Add(l);
            Colour c = Colour.Yellow;
            var d = 2424.655;


            var s1 = lst.Collect();

            serializer.Serialize(lst, stream);

            File.WriteAllBytes(@"V:\test.bin", stream.ToArray());

            stream.Position = 0;

            var r = new MyBinarySerializer().Deserialize(stream);
            var s2 = r.Collect();

            Console.WriteLine(s1);
            Console.WriteLine(s1 == s2);
        }
    }
}
