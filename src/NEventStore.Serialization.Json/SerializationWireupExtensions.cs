// ReSharper disable CheckNamespace
namespace NEventStore
// ReSharper restore CheckNamespace
{
    using System;
	using System.Linq;
	using System.Collections.Generic;
	using NEventStore.Serialization;

    public static class WireupExtensions
    {
		public static SerializationWireup UsingJsonSerialization(this PersistenceWireup wireup, params Type[] knownTypes)
        {
			return wireup.UsingCustomSerialization(new JsonSerializer(knownTypes));
        }

		public static SerializationWireup UsingBsonSerialization(this PersistenceWireup wireup, params Type[] knownTypes)
        {
			return wireup.UsingCustomSerialization(new BsonSerializer(knownTypes));
        }
    }
}