using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace myRaiHelper
{
    class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression from, to;
        public ReplaceVisitor(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }
        public override Expression Visit(Expression node)
        {
            return node == from ? to : base.Visit(node);
        }
    }
    public static class LinqHelper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey> ( this IEnumerable<TSource> source , Func<TSource , TKey> keySelector )
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>( );
            foreach ( TSource element in source )
            {
                if ( seenKeys.Add( keySelector( element ) ) )
                {
                    yield return element;
                }
            }
        }

        public static Expression<Func<T, bool>> PutInAndTogether<T>(params Expression<Func<T, bool>>[] expressions)
        {
            Expression<Func<T, bool>> result = x => true;

            if (expressions != null && expressions.Any())
            {
                int skip = 1;
                result = expressions.First();
                if (result == null)
                {
                    skip++;
                    result = expressions[1];
                }
                foreach (var nextFilter in expressions.Skip(skip).Where(x=>x!=null))
                {
                    var nextExpression = new ReplaceVisitor(result.Parameters[0], nextFilter.Parameters[0]).Visit(result.Body);
                    result = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(nextExpression, nextFilter.Body), nextFilter.Parameters);
                }
            }

            return result;
        }

        public static Expression<Func<T, bool>> PutInOrTogether<T>(params Expression<Func<T, bool>>[] expressions)
        {
            Expression<Func<T, bool>> result = x => true;

            if (expressions != null && expressions.Any())
            {
                int skip = 1;
                result = expressions.First();
                if (result==null)
                {
                    skip++;
                    result = expressions[1];
                }
                foreach (var nextFilter in expressions.Skip(skip).Where(x => x != null))
                {
                    var nextExpression = new ReplaceVisitor(result.Parameters[0], nextFilter.Parameters[0]).Visit(result.Body);
                    result = Expression.Lambda<Func<T, bool>>(Expression.OrElse(nextExpression, nextFilter.Body), nextFilter.Parameters);
                }
            }

            return result;
        }
    }
}
