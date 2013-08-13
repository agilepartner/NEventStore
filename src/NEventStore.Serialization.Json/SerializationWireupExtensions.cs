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
		public static SerializationWireup UsingJsonSerialization(this PersistenceWireup wireup) 
		{
			return UsingJsonSerialization(wireup, null);
		}

        public static SerializationWireup UsingJsonSerialization(this PersistenceWireup wireup, Func<IEnumerable<Type>> knownTypesFactory)
        {
			return wireup.UsingCustomSerialization(new JsonSerializer(knownTypesFactory != null ? knownTypesFactory().ToArray() : null));
        }

		public static SerializationWireup UsingBsonSerialization(this PersistenceWireup wireup)
		{
			return UsingBsonSerialization(wireup, null);
		}

		public static SerializationWireup UsingBsonSerialization(this PersistenceWireup wireup, Func<IEnumerable<Type>> knownTypesFactory)
        {
			return wireup.UsingCustomSerialization(new BsonSerializer(knownTypesFactory != null ? knownTypesFactory().ToArray() : null));
        }
    }
}