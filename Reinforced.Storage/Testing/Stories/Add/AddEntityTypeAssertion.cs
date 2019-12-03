﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reinforced.Storage.SideEffects;
using Reinforced.Storage.SideEffects.Exact;

namespace Reinforced.Storage.Testing.Stories.Add
{
    public class AddEntityTypeAssertion<T> : SideEffectAssertion<AddSideEffect>, IMemorizing
    {
        private readonly Memorize<T> _memorizedValue;

        public AddEntityTypeAssertion(Memorize<T> mem = null)
        {
            _memorizedValue = mem;
        }

        public override string GetMessage(AddSideEffect effect)
        {
            if (effect == null) return $"expected added entity of type {typeof(T).Name}, but story unexpectedly ends";
            return $"expected added entity of type {typeof(T).Name}, but got one of {effect.EntityType.Name}";
        }

        public override bool IsValid(AddSideEffect effect)
        {
            if (effect == null) return false;
            return effect.EntityType == typeof(T);
        }

        public void Memorize(SideEffectBase seb)
        {
            _memorizedValue.SetValue(((AddSideEffect)seb).Entity);
        }
    }
}
