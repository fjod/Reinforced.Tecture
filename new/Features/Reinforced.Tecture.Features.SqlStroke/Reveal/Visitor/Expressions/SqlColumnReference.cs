﻿using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Reinforced.Tecture.Features.SqlStroke.Reveal.Visitor.Expressions
{
    /// <summary>
    /// Column reference expression
    /// </summary>
    public class SqlColumnReference : SqlQueryExpression
    {
        /// <summary>
        /// Table reference
        /// </summary>
        public TableReference Table { get; internal set; }

        /// <summary>
        /// Column that is being referenced
        /// </summary>
        public PropertyInfo Column { get; internal set; }
    }
}