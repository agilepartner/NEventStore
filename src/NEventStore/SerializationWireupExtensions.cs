namespace NEventStore
{
    using NEventStore.Serialization;

    public static class SerializationWireupExtensions
    {
        public static SerializationWireup UsingBinarySerialization(this PersistenceWireup wireup)
        {
#if PocketPC
            return wireup.UsingCustomSerialization(new SharpBinarySerializer());
#else
            return wireup.UsingCustomSerialization(new BinarySerializer()); 
#endif
        }

        public static SerializationWireup UsingCustomSerialization(this PersistenceWireup wireup, ISerialize serializer)
        {
            return new SerializationWireup(wireup, serializer);
        }
    }
}