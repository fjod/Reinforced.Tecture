﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace Reinforced.Tecture.Features.SqlStroke.Reveal.Visitor.Expressions
{
    /// <summary>
    /// Ternary operator
    /// </summary>
    public class SqlTernaryExpression : SqlQueryExpression
    {
        public SqlQueryExpression Condition { get; internal set; }
        public SqlQueryExpression IfTrue { get; internal set; }
        public SqlQueryExpression IfFalse { get; internal set; }
    }
}