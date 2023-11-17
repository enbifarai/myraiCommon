using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TOutput> TryConvertItemsTo<TSource, TOutput>(this IEnumerable<TSource> source, Func<TSource, TOutput> castFunc)
        {
            return source.Select(x => castFunc(x));
        }
    }
}