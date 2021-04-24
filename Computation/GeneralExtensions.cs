namespace Computation
{
    using System;
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

        // Thanks to Mak from Makolyte (https://makolyte.com/about/) for this inplace dictionary merge method
        // Source: https://makolyte.com/csharp-merge-two-dictionaries-in-place/
        public static Dictionary<TKey, TValue> MergeInPlace<TKey, TValue>(this Dictionary<TKey, TValue> left, Dictionary<TKey, TValue> right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left), "Can't merge into a null dictionary");
            }
            else if (right == null)
            {
                return left;
            }

            foreach (var kvp in right)
            {
                if (!left.ContainsKey(kvp.Key))
                {
                    left.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    left[kvp.Key] = kvp.Value; // Here's the difference from the original snippet: I want method to override existing values
                }
            }

            return left;
        }
    }
}
