using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public enum Colour
    {
        Red = 0,
        Green = 1,
        Yellow = 2
    }

    public class Apple//<T>
    {
        private int taste;
        public Colour Colour { get; set; }

        public object x;

        public Apple(Colour colour, int taste)
        {
            this.Colour = colour;
            this.taste = taste;
            //x = new Apple<string>() {
            //    Colour = Colour.Yellow,
            //    taste = 23432,
            //    x = new Apple<Apple<Dictionary<bool, ConsoleColor>>>() {
            //        Colour = Colour.Red,
            //        taste = 1,
            //        x = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
            //    }
            //};
        }

        static int _cnt = 0;

        public Apple()
        {
            taste = --_cnt;
        }
    }

    struct User
    {
        public string name;
        public int age;
        
    }

    class Program
    {

        static void Main(string[] args)
        {
            var serializer = new MySerializer();
            var stream = new MemoryStream();

            User usr = new User();
            usr.age = 18;
            usr.name = "Tom";

            var lst = new List<object>();

            var apple = new Apple(Colour.Green, 4);
            lst.Add(new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
            lst.Add(apple);
            lst.Add(ConsoleColor.Yellow);
            lst.Add("abcd");
            lst.Add(4321);

            var x = new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } };

            Apple[] apples = new Apple[2] { new Apple(Colour.Yellow, 9), new Apple(Colour.Red, 10) };

            var l = new List<Apple>();
            l.Add(new Apple(Colour.Yellow, 9));
            l.Add(new Apple(Colour.Green, 7));
            l.Add(new Apple(Colour.Yellow, 5));
            l.Add(new Apple(Colour.Red, 8));
            l.Add(new Apple(Colour.Yellow, 2));

            Colour c = Colour.Yellow;
            var d = 2424.655;
            serializer.Serialize(l, stream);
            stream.Position = 0;

            //var str = Encoding.UTF8.GetString(stream.ToArray()).Replace('\0', '.');
            //Console.WriteLine(str);

            var v = serializer.Deserialize(stream);
        }
    }
}
