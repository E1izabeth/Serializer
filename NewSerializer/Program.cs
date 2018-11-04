using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            //var y = new Counter();

            //var arr = new I1[] { y, y, y, y, y, y, y, y };

            //arr[0].Inc();
            //arr[1].Inc();
            //arr[2].Inc();
            //arr[3].Inc();


            //return;

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
            lst.Add(new Dictionary<string, Apple>() { { "x", app3 }, { "y", app4 }, { "z", app5 } });
            lst.Add(l);
            lst.Add(null);
            Colour c = Colour.Yellow;
            var d = 2424.655;


            var s1 = lst.Collect();

            serializer.Serialize(lst, stream);

            WriteFile(@"test.bin", stream.ToArray());

            stream.Position = 0;

            var deserializer = new MyBinarySerializer();
            var r = deserializer.Deserialize(stream);
            var s2 = r.Collect();

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s1 == s2);

            WriteFile(@"reader-log.txt", deserializer.LastLog);

            // WriteFile(@"s1.txt", s1);
            // WriteFile(@"s2.txt", s2);
        }

        static void WriteFile(string filename, string text)
        {
            DeleteFile(filename);
            File.WriteAllText(filename, text);
        }

        static void WriteFile(string filename, byte[] data)
        {
            DeleteFile(filename);
            File.WriteAllBytes(filename, data);
        }

        static void DeleteFile(string filename)
        {
            for (int i = 0; i < 3 && File.Exists(filename); i++)
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            if (File.Exists(filename))
                File.Delete(filename);
        }
    }

    interface I1
    {
        void Inc();
    }

    struct Counter : I1
    {
        public int value;

        public void Inc()
        {
            this.value++;
        }

        public override string ToString()
        {
            return "Counter: " + this.value;
        }
    }
}
