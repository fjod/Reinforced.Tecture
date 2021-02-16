﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reinforced.Tecture.Testing.Validation;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reinforced.Tecture.Testing.Data.Format
{
    partial class CodeFormatter
    {
        private readonly Stack<Type> _ctx = new Stack<Type>();
        protected TabsManager Tabs { get; } = new TabsManager();

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node == null) return null;
            _ctx.Push(node.GetType());
            
            var r = base.Visit(node);
            if (r is StatementSyntax st)
            {
                if (!(st is EmptyStatementSyntax) && !(st is BlockSyntax))
                {
                    if (!st.HasAnnotation(Annotations.IfStatement))
                    {
                        r = st.WithoutTrivia().WithLeadingTrivia(Tabs.BrTabs());
                    }
                    else
                    {
                        r = r.WithLeadingTrivia(Space);
                    }
                }
            }

            if (r is AssignmentExpressionSyntax aer)
            {
                r = aer.WithOperatorToken(aer.OperatorToken.WithoutTrivia().WithLeadingTrivia(Space).WithTrailingTrivia(Space));
            }
            if (r is EqualsValueClauseSyntax nds)
            {
                r = nds.WithEqualsToken(Token(nds.EqualsToken.Kind()).WithLeadingTrivia(Space)
                    .WithTrailingTrivia(Space));
            }
            
            _ctx.Pop();
            return r;
        }

        

        private bool IsWithin<T>() where T:SyntaxNode
        {
            if (_ctx.Count == 0) return false;
            return _ctx.Peek() == typeof(T);
        }

        private bool IsWithinAny<T>() where T : SyntaxNode
        {
            if (_ctx.Count == 0) return false;
            foreach (var type in _ctx)
            {
                if (type == typeof(T)) return true;
            }

            return false;
        }

        protected TabsScope Tab()
        {
            return new TabsScope(Tabs);
        }
    }
}
