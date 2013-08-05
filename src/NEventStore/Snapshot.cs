namespace NEventStore
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents a materialized view of a stream at specific revision.
    /// </summary>
#if !PocketPC
    [DataContract]
#endif
    [Serializable]
    public class Snapshot
    {
        /// <summary>
        ///     Initializes a new instance of the Snapshot class.
        /// </summary>
        /// <param name="streamId">The value which uniquely identifies the stream to which the snapshot applies.</param>
        /// <param name="streamRevision">The position at which the snapshot applies.</param>
        /// <param name="payload">The snapshot or materialized view of the stream at the revision indicated.</param>
        public Snapshot(Guid streamId, int streamRevision, object payload)
            : this()
        {
            StreamId = streamId;
            StreamRevision = streamRevision;
            Payload = payload;
        }

        /// <summary>
        ///     Initializes a new instance of the Snapshot class.
        /// </summary>
        protected Snapshot()
        {}

        /// <summary>
        ///     Gets the value which uniquely identifies the stream to which the snapshot applies.
        /// </summary>
#if !PocketPC
        [DataMember] 
#endif
        public virtual Guid StreamId { get; private set; }

        /// <summary>
        ///     Gets the position at which the snapshot applies.
        /// </summary>
#if !PocketPC
        [DataMember] 
#endif
        public virtual int StreamRevision { get; private set; }

        /// <summary>
        ///     Gets the snapshot or materialized view of the stream at the revision indicated.
        /// </summary>
#if !PocketPC
        [DataMember] 
#endif
        public virtual object Payload { get; private set; }
    }
}