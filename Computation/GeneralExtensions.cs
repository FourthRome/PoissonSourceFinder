namespace Computation
{
    using System.Collections.Generic;

    public static class GeneralExtensions
    {
        // Thanks to Paul Mitchell (https://stackoverflow.com/users/38966/paul-mitchell) for this useful snippet
        // Source: https://stackoverflow.com/a/45239105/6819449
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> input, int start = 0)
        {
            int i = start;
            foreach (var t in input)
            {
                yield return (i++, t);
            }
        }
    }
}
