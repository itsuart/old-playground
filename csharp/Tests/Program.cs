using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var dict = new Dictionary<int, int>();
            dict.Add(1,1);
            dict.Add(1,2);
            Console.WriteLine(dict[1]);
        }
    }
}
