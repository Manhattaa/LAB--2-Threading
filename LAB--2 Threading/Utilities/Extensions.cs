using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB__2_Threading.Utilities
{
    public static class Extensions
    {
        public static T FindMax<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
            where TKey : IComparable<TKey>
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                var maxElement = enumerator.Current;
                var maxValue = selector(maxElement);

                while (enumerator.MoveNext())
                {
                    var element = enumerator.Current;
                    var value = selector(element);

                    if (value.CompareTo(maxValue) > 0)
                    {
                        maxElement = element;
                        maxValue = value;
                    }
                }

                return maxElement;
            }
        }
    }
}