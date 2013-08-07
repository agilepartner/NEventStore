using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NEventStore.Example.CF
{
    internal class AggregateMemento
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
