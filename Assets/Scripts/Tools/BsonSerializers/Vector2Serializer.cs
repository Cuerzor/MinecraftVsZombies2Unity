﻿using MongoDB.Bson.Serialization;
using UnityEngine;

namespace Tools.BsonSerializers
{
    public class Vector2Serializer : ArraySerializerBase<Vector2>
    {
        protected override void WriteArrayValues(BsonSerializationContext context, BsonSerializationArgs args, Vector2 value)
        {
            var writer = context.Writer;
            writer.WriteDouble(value.x);
            writer.WriteDouble(value.y);
        }

        protected override Vector2 ReadArrayValues(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            float x = (float)reader.ReadDouble();
            float y = (float)reader.ReadDouble();
            return new Vector2(x, y);
        }
    }
}
