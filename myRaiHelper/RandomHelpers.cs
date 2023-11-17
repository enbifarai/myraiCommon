using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiHelper
{
    public static class RandomHelpers
    {
        public static int GetRandomInt(int max)
        {
            Random rnd = new Random();
            System.Threading.Thread.Sleep(2);
            int value = rnd.Next(max);

            return value;
        }

        public static decimal GetRandomDecimal(decimal min, decimal max)
        {
            Random rnd = new Random();
            System.Threading.Thread.Sleep(4);
            return ((decimal)rnd.NextDouble() * (max - min) + min);
        }
    }
}