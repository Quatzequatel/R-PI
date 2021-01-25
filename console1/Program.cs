using System;

namespace console1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.In.ReadLineAsync().GetAwaiter().GetResult();
        }
    }
}
