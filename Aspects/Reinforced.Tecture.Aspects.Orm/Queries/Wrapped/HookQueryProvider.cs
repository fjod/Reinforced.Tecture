﻿using System.Linq;
using System.Linq.Expressions;
using Reinforced.Tecture.Aspects.Orm.Queries.Hashing;
using Reinforced.Tecture.Aspects.Orm.Queries.Wrapped.Queryables;
using Reinforced.Tecture.Query;
using Reinforced.Tecture.Tracing.Promises;

namespace Reinforced.Tecture.Aspects.Orm.Queries.Wrapped
{
    class HookQueryProvider : IQueryProvider
    {
        public IQueryProvider Original { get; }

        public Query Aspect { get; }

        public DescriptionHolder Description { get; }

        public HookQueryProvider(IQueryProvider original, Query aspect, DescriptionHolder description)
        {
            Original = original;
            Aspect = aspect;
            Description = description;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var bs = Original.CreateQuery(expression);
            return new WrappedQueryable(bs, Aspect, Description);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var bs = Original.CreateQuery<TElement>(expression);
            return new WrappedQueryable<TElement>(bs, Aspect, Description);
        }

        public object Execute(Expression expression)
        {
            var p = Aspect.Aux.Promise<object>();
            if (p is Containing<object> c)
                return c.Get(expression.CalculateHash(), Description.Description);

            var result = Original.Execute(expression);

            if (p is Demanding<object> d)
                d.Fullfill(result, expression.CalculateHash(), Description.Description);

            return result;
        }

        /// <summary>Executes the strongly-typed query represented by a specified expression tree.</summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <typeparam name="TResult">The type of the value that results from executing the query.</typeparam>
        /// <returns>The value that results from executing the specified query.</returns>
        public TResult Execute<TResult>(Expression expression)
        {
            var p = Aspect.Aux.Promise<TResult>();
            if (p is Containing<TResult> c)
                return c.Get(expression.CalculateHash(), Description.Description);

            var result = Original.Execute<TResult>(expression);

            if (p is Demanding<TResult> d)
                d.Fullfill(result, expression.CalculateHash(), Description.Description);

            return result;
        }
    }
}
