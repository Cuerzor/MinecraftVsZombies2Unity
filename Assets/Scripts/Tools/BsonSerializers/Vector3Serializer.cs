﻿using MongoDB.Bson.Serialization;
using UnityEngine;

namespace Tools.BsonSerializers
{
    public class Vector3Serializer : ArraySerializerBase<Vector3>
    {
        protected override void WriteArrayValues(BsonSerializationContext context, BsonSerializationArgs args, Vector3 value)
        {
            var writer = context.Writer;
            writer.WriteDouble(value.x);
            writer.WriteDouble(value.y);
            writer.WriteDouble(value.z);
        }

        protected override Vector3 ReadArrayValues(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            float x = (float)reader.ReadDouble();
            float y = (float)reader.ReadDouble();
            float z = (float)reader.ReadDouble();
            return new Vector3(x, y, z);
        }
    }
}
