using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NEventStore.Example.CF
{
    public abstract class Event
    {
        public Event()
        {
            Occurred = DateTime.Now;
        }

        public int Version { get; set; }
        public DateTime Occurred { get; private set; }
    }

    public abstract class TourEvent : Event
    {
        public Guid AggregateId { get; protected set; }

        public TourEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }

        public override string ToString()
        {
            return String.Format("{0} (Id : {1}, Occurred : {2})", this.GetType().Name, AggregateId, Occurred);
        }
    }

    public class TourCreated : TourEvent
    {
        public readonly string TourId;
        public readonly string LogicalDeviceId;
        public readonly string PhysicalDeviceId;

        public TourCreated(Guid aggregateId, string tourId, string logicalDeviceId, string physicalDeviceId)
            : base(aggregateId)
        {
            TourId = tourId;
            LogicalDeviceId = logicalDeviceId;
            PhysicalDeviceId = physicalDeviceId;
        }
    }

    public class TourStarted : TourEvent
    {
        public TourStarted(Guid aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class TourSuspended : TourEvent
    {
        public TourSuspended(Guid aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class TourFinished : TourEvent
    {
        public TourFinished(Guid aggregateId)
            : base(aggregateId)
        {
        }
    }
}
