﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;

namespace Guflow.Decider
{
    internal class Limit
    {
        private readonly uint _count;

        private Limit(uint count)
        {
            _count = count;
        }
        /// <summary>
        /// Limit the rescheduling to giving number of counts.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limit Count(uint count)
        {
            return new Limit(count);
        }

        public bool IsExceeded(WorkflowItem workflowItem)
        {
            var allEvents = workflowItem.LastSimilarEvents();
            return allEvents.Count() > _count;
        }
    }
}