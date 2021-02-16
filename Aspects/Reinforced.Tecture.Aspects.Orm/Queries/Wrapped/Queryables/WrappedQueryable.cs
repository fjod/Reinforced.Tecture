﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reinforced.Tecture.Aspects.Orm.Queries.Hashing;
using Reinforced.Tecture.Aspects.Orm.Queries.Wrapped.Enumerators;
using Reinforced.Tecture.Query;
using Reinforced.Tecture.Tracing.Promises;

namespace Reinforced.Tecture.Aspects.Orm.Queries.Wrapped.Queryables
{
    class WrappedQueryable : IOrderedQueryable
    {
        public Query Aspect { get; }

        public IQueryable Original { get; }

        public DescriptionHolder Description { get; }

        public WrappedQueryable(IQueryable original, Query aspect, DescriptionHolder description)
        {
            Original = original;
            Aspect = aspect;
            Description = description;
        }


        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            var p = Aspect.Aux.Promise<IEnumerable<object>>();

            if (p is Containing<IEnumerable<object>> c)
            {
                var td = c.Get(Expression.CalculateHash(), Description.Description);
                return td.GetEnumerator();
            }

            var tran = Aspect.Aux.GetQueryTransaction();
            var originalEnumerator = Original.GetEnumerator();
            var result = new WrappedEnumerator(originalEnumerator, tran);

            if (p is Demanding<IEnumerable<object>> d)
            {
                result.Demands(d, Expression.CalculateHash(), Description);
            }

            return result;
        }
        

        /// <summary>Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.</summary>
        /// <returns>A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
        public Type ElementType
        {
            get
            {
                return Original.ElementType;
            }
        }

        /// <summary>Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of <see cref="T:System.Linq.IQueryable" />.</returns>
        public Expression Expression
        {
            get
            {
                return Original.Expression;
            }
        }


        private IQueryProvider _provider;

        public IQueryProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new HookQueryProvider(Original.Provider, Aspect, Description);
                }
                return _provider;
            }
        }
    }
}
