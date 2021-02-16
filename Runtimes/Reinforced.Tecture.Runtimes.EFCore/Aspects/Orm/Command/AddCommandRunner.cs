﻿using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reinforced.Tecture.Aspects.Orm.Commands.Add;
using Reinforced.Tecture.Commands;
using Reinforced.Tecture.Query;

namespace Reinforced.Tecture.Runtimes.EFCore.Aspects.Orm.Command
{
    class AddCommandRunner : CommandRunner<Add>
    {
        private readonly ILazyDisposable<DbContext> _dc;
        private readonly Auxiliary _aux;
        public AddCommandRunner(Auxiliary aux, ILazyDisposable<DbContext> dc)
        {
            _aux = aux;
            _dc = dc;
        }



        /// <inheritdoc />
        protected override void Run(Add cmd)
        {
            if (_aux.IsCommandRunNeeded)
            {
                _dc.Value.Add(cmd.Entity);
                var ent = _dc.Value.Entry(cmd.Entity);
                ent.State = EntityState.Added;
            }
        }


        /// <inheritdoc />
        protected override Task RunAsync(Add cmd)
        {
            Run(cmd);
            return Task.FromResult(0);
        }
    }
}
