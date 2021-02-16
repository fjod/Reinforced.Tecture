﻿using Reinforced.Tecture.Aspects.DirectSql.Reveal.Visit.Expressions;
using Reinforced.Tecture.Aspects.DirectSql.Reveal.LanguageInterpolate;
using Reinforced.Tecture.Aspects.DirectSql.Reveal.Visit;

namespace Reinforced.Tecture.Aspects.DirectSql.Reveal.LanguageInterpolate
{
    public partial class LanguageInterpolator
    {
        private bool IsNullExpression(SqlBinaryExpression bex)
        {
            if (!((bex.Right is SqlNullExpression || bex.Left is SqlNullExpression))) return false;
            if (bex.Operator == SqlOperator.Equal) return true;
            if (bex.Operator == SqlOperator.NotEqual) return true;
            return false;
        }

        protected virtual string VisitBinary(SqlBinaryExpression bex)
        {
            string result;
            if (IsSetExpression(bex))
            {
                _isParsingSet = true;
                result = VisitSet(bex);
                _isParsingSet = false;
                return result;
            }

            if (!_isParsingSet && IsNullExpression(bex))
            {
                result = VisitIsNull(bex);
            }
            else
            {
                if (bex.Operator == SqlOperator.Coalesce)
                {
                    result = $"{IfNull}({Visit(bex.Left)},{Visit(bex.Right)})";
                }
                else result = $"{Visit(bex.Left)} {OperatorText(bex.Operator)} {Visit(bex.Right)}";
            }

            return result.Braces(!bex.IsTop);
        }

        protected virtual string VisitIsNull(SqlBinaryExpression bex)
        {
            var activePart = bex.Left is SqlNullExpression ? bex.Right : bex.Left;
            var nullPart = bex.Left is SqlNullExpression ? bex.Left : bex.Right;
            var ap = Visit(activePart);
            return IsNull(ap, null, bex.Operator == SqlOperator.NotEqual).Braces(!bex.IsTop);
        }

    }
}
