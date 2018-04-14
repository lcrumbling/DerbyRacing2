using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRacing.RacingObjects
{
    public static class Extensions
    {
        // from http://stackoverflow.com/a/1287572/1888137
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        // from http://stackoverflow.com/a/1471034/656243
        public static void RotateLeft<T>(this T[] array, int places)
        {
            T[] temp = new T[places];
            Array.Copy(array, 0, temp, 0, places);
            Array.Copy(array, places, array, 0, array.Length - places);
            Array.Copy(temp, 0, array, array.Length - places, places);
        }

    }
}
