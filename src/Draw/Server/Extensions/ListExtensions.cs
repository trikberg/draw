using System;
using System.Collections.Generic;

namespace Draw.Server.Extensions
{
    static public class ListExtensions
    {
        private static Random rnd = new Random(Guid.NewGuid().GetHashCode());
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
