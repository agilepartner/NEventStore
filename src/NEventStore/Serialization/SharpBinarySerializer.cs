using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Polenter.Serialization;
using NEventStore.Logging;

namespace NEventStore.Serialization
{
    public class SharpBinarySerializer : ISerialize
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SharpBinarySerializer));
        private readonly SharpSerializerBinarySettings _settings;
        private readonly SharpSerializer _serializer;

        public SharpBinarySerializer()
        {
            _settings = new SharpSerializerBinarySettings(BinarySerializationMode.Burst);
            //_settings.IncludeAssemblyVersionInTypeName = false;
            //_settings.IncludeCultureInTypeName = false;
            //_settings.IncludePublicKeyTokenInTypeName = false;

            _serializer = new SharpSerializer(true);
        }

        public void Serialize<T>(Stream output, T graph)
        {
            Logger.Verbose(Messages.SerializingGraph, typeof(T));
            _serializer.Serialize(graph, output);
        }

        public T Deserialize<T>(Stream input)
        {
            Logger.Verbose(Messages.DeserializingStream, typeof(T));
            return (T)_serializer.Deserialize(input);
        }

    }
}
