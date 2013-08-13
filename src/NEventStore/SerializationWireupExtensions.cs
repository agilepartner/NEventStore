namespace NEventStore
{
    using NEventStore.Serialization;

    public static class SerializationWireupExtensions
    {
#if !PocketPC
        public static SerializationWireup UsingBinarySerialization(this PersistenceWireup wireup)
        {
            return wireup.UsingCustomSerialization(new BinarySerializer()); 
        }
#endif

        public static SerializationWireup UsingCustomSerialization(this PersistenceWireup wireup, ISerialize serializer)
        {
            return new SerializationWireup(wireup, serializer);
        }
    }
}