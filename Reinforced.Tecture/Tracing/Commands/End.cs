﻿using System.IO;
using Reinforced.Tecture.Commands;

namespace Reinforced.Tecture.Tracing.Commands
{
    /// <summary>
    /// Synthetic command that means end of the commands queue
    /// </summary>
    [CommandCode(" ! ")]
    public sealed class End : CommandBase
    {
        internal End() { }


        /// <summary>
        /// Describes actions that are being performed within side effect
        /// </summary>
        /// <param name="tw"></param>
        public override void Describe(TextWriter tw)
        {
            tw.Write("<END>");
        }

        /// <summary>
        /// Clones command for tracing purposes
        /// </summary>
        /// <returns>Command clone</returns>
        protected override CommandBase DeepCloneForTracing()
        {
            return new End();
        }
    }
}
