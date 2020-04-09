﻿using System;

namespace Reinforced.Tecture.Methodics.Orm.Testing.Checks.Update
{
    public class UpdatePredicateCheck<T> : CommandCheck<UpdateSideEffect>, IMemorizing
    {
        private readonly Memorize<T> _memorizedValue;
        private readonly Func<T, bool> _predicate;
        private readonly string _explanation;

        public UpdatePredicateCheck(Func<T, bool> predicate, string explanation, Memorize<T> mem = null)
        {
            _predicate = predicate;
            _explanation = explanation;
            _memorizedValue = mem;
        }

        public override string GetMessage(UpdateSideEffect command)
        {
            if (command == null) return $"expected updated entity {_explanation}, but story unexpectedly ends";
            if (command.EntityType != typeof(T))
            {
                return
                    $"expected updated entity of type {typeof(T).Name} and {_explanation}, but got one of {command.EntityType.Name}";
            }
            return $"expected updated entity {_explanation}, but seems that it does not";
        }

        public override bool IsActuallyValid(UpdateSideEffect effect)
        {
            if (effect == null) return false;
            if (effect.EntityType != typeof(T)) return false;
            return _predicate((T) effect.Entity);
        }

        public void Memorize(SideEffectBase seb)
        {
            _memorizedValue.SetValue(((UpdateSideEffect)seb).Entity);
        }
    }
}