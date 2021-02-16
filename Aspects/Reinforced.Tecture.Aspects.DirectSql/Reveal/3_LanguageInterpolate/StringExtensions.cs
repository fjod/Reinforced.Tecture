﻿// ReSharper disable CheckNamespace
namespace Reinforced.Tecture.Aspects.DirectSql.Reveal.LanguageInterpolate
{
    internal static class StringExtensions
    {
        public static string Braces(this string expr, bool need)
        {
            if (need) return $"({expr})";
            return expr;
        }
    }
}