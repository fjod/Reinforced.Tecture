﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reinforced.Tecture.Commands;
using Reinforced.Tecture.Methodics.Orm.Commands.Update;
using Reinforced.Tecture.Testing;

namespace Reinforced.Tecture.Methodics.Orm.Testing.Runners
{
    public class UpdateAssumptionArgument<T>
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public UpdateAssumptionArgument(T entity, UpdateCommand sideEffect, ICollectionProvider collectionProvider)
        {
            Entity = entity;
            SideEffect = sideEffect;
            CollectionProvider = collectionProvider;
        }

        public T Entity { get; private set; }

        public UpdateCommand SideEffect { get; private set; }

        public ICollectionProvider CollectionProvider { get; private set; }
    }
    class UpdateCommandRunner : ICommandRunner<UpdateCommand>
    {
        private readonly ICollectionProvider _env;
        private readonly Dictionary<Type, List<Delegate>> _assumedActions = new Dictionary<Type, List<Delegate>>();

        public UpdateCommandRunner(ICollectionProvider env)
        {
            _env = env;
        }

        public UpdateCommandRunner Assume<T>(Action<UpdateAssumptionArgument<T>> ua)
        {
            if (!_assumedActions.ContainsKey(typeof(T))) _assumedActions[typeof(T)] = new List<Delegate>();
            _assumedActions[typeof(T)].Add(ua);
            return this;
        }

        /// <summary>
        /// Runs side effect 
        /// </summary>
        /// <param name="effect">Side effect</param>
        public void Run(UpdateCommand effect)
        {
            var coll = _env.GetCollection(effect.EntityType);
            coll.Remove(effect.Entity);
            if (_assumedActions.ContainsKey(effect.EntityType))
            {
                var l = _assumedActions[effect.EntityType];
                var inst = Activator.CreateInstance(typeof(UpdateAssumptionArgument<>).MakeGenericType(effect.EntityType), new[] { effect.Entity, effect, _env });
                foreach (var del in l)
                {
                    del.DynamicInvoke(inst);
                }
            }
        }

        /// <summary>
        /// Runs side effect asynchronously
        /// </summary>
        /// <param name="effect">Side effect</param>
        /// <returns>Side effect</returns>
        public Task RunAsync(UpdateCommand effect)
        {
            Run(effect);
            return Task.FromResult(0);
        }
    }
}