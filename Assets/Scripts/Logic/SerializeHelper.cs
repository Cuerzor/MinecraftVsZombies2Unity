using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MVZ2Logic.Level;
using MVZ2Logic.Saves;
using PVZEngine;
using PVZEngine.Armors;
using PVZEngine.BsonSerializers;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.Level.BsonSerializers;
using PVZEngine.Level.Collisions;
using PVZEngine.SeedPacks;
using Tools;
using Tools.BsonSerializers;
using UnityEngine;

namespace MVZ2Logic
{
    public static class SerializeHelper
    {
        public static void init(string defaultNsp)
        {
            if (isInited)
                return;
            RegisterSerializer(new Vector2Serializer());
            RegisterSerializer(new Vector3Serializer());
            RegisterSerializer(new ColorSerializer());
            RegisterSerializer(new RandomGeneratorSerializer());
            RegisterSerializer(new NamespaceIDSerializer(defaultNsp));
            RegisterSerializer(new PropertyBlockSerializer());

            // Tools
            RegisterClass<FrameTimer>();
            RegisterClass<RandomGenerator>();

            // PVZEngine.Base
            RegisterClass<NamespaceID>();
            RegisterClass<SerializablePropertyDictionary>();
            RegisterClass<SerializablePropertyDictionaryString>();

            // PVZEngine.Level
            RegisterClass<EntityReferenceChain>();
            RegisterClass<EntityID>();
            RegisterClass<SerializableHitbox>();
            RegisterClass<SerializableCustomHitbox>();
            RegisterClass<SerializableEntityHitbox>();
            RegisterClass<SerializableEntityCollider>();
            RegisterClass<SerializableBuiltinCollisionSystem>();
            RegisterClass<SerializableBuiltinCollisionSystemEntity>();

            RegisterClass<BuffReference>();
            RegisterClass<BuffReferenceEntity>();
            RegisterClass<BuffReferenceArmor>();
            RegisterClass<BuffReferenceLevel>();
            RegisterClass<BuffReferenceClassicSeedPack>();
            RegisterClass<BuffReferenceConveyorSeedPack>();

            RegisterClass<SerializableEntity>();
            RegisterClass<SerializableArmor>();
            RegisterClass<SerializableBuff>();
            RegisterClass<SerializableBuffList>();
            RegisterClass<SerializableGrid>();
            RegisterClass<SerializableLevel>();
            RegisterClass<SerializableDelayedEnergy>();
            RegisterClass<SerializableLevelOption>();
            RegisterClass<SerializableClassicSeedPack>();
            RegisterClass<SerializableConveyorSeedPack>();
            RegisterClass<SerializableConveyorSeedSpendRecords>();
            RegisterClass<SerializableConveyorSeedSendRecordEntry>();


            // MVZ2.Logic
            RegisterClass<ShakeInt>();
            RegisterClass<ShakeFloat>();
            RegisterClass<SerializableModSaveData>();
            RegisterClass<SerializableUserStats>();
            RegisterClass<SerializableUserStatCategory>();
            RegisterClass<SerializableUserStatEntry>();
            RegisterClass<SerializableEndlessRecord>();
            RegisterClass<SerializableLevelDifficultyRecord>();

            isInited = true;
        }
        public static void WriteCompressedJson(string path, string json)
        {
            try
            {
                using var stream = File.Open(path, FileMode.Create);
                using var gzipStream = new GZipStream(stream, CompressionMode.Compress);
                using var textWriter = new StreamWriter(gzipStream);
                textWriter.Write(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write compressed json to file {path}: {e}");
            }
        }
        public static string ReadCompressedJson(string path)
        {
            using var stream = File.Open(path, FileMode.Open);
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            using var memory = new MemoryStream();
            gzipStream.CopyTo(memory);
            memory.Seek(0, SeekOrigin.Begin);
            using var textReader = new StreamReader(memory);
            return textReader.ReadToEnd();
        }
        public static void WriteJson(string path, string json)
        {
            try
            {
                using var stream = File.Open(path, FileMode.Create);
                using var textWriter = new StreamWriter(stream);
                textWriter.Write(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write compressed json to file {path}: {e}");
            }
        }
        public static string ReadJson(string path)
        {
            using var stream = File.Open(path, FileMode.Open);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            memory.Seek(0, SeekOrigin.Begin);
            using var textReader = new StreamReader(memory);
            return textReader.ReadToEnd();
        }
        public static string ToBson(this object obj, bool readFriendly = false)
        {
            ConventionRegistry.Register(nameof(CustomClassMapConvention), conventions, t => true);
            JsonWriterSettings setting = readFriendly ? readFriendlyWriterSettings : null;
            string bson = obj.ToJson(setting);
            ConventionRegistry.Remove(nameof(CustomClassMapConvention));
            return bson;
        }
        public static T FromBson<T>(string bson)
        {
            ConventionRegistry.Register(nameof(CustomClassMapConvention), conventions, t => true);
            T obj = BsonSerializer.Deserialize<T>(bson);
            ConventionRegistry.Remove(nameof(CustomClassMapConvention));
            return obj;
        }
        public static void RegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            BsonSerializer.RegisterSerializer(serializer);
        }
        public static void RegisterClass<T>()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                BsonClassMap.RegisterClassMap<T>();
        }
        public static bool isInited { get; private set; } = false;
        public static JsonWriterSettings readFriendlyWriterSettings = new JsonWriterSettings()
        {
            Indent = true
        };
        static ConventionPack conventions = new ConventionPack()
        {
            new CustomClassMapConvention(),
        };
    }
    /// <summary>
    /// 自定义类映射规则
    /// 只映射字段，忽略属性
    /// </summary>
    public class CustomClassMapConvention : IClassMapConvention
    {
        public string Name { get; } = "FieldOnlyClassMapConvention";
        public void Apply(BsonClassMap classMap)
        {
            //递归父类
            while (classMap != null)
            {
                BsonMemberMap[] memberMaps = classMap.DeclaredMemberMaps.ToArray();
                foreach (BsonMemberMap memberMap in memberMaps)
                {
                    //去掉所有属性
                    if (memberMap.MemberInfo.MemberType == MemberTypes.Property)
                        classMap.UnmapMember(memberMap.MemberInfo);
                }

                var fields = classMap.ClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                //忽略所有带有忽略特性的字段
                var noSerializeFields = fields.Where(f => f.GetCustomAttribute<BsonIgnoreAttribute>() == null && f.GetCustomAttribute<NonSerializedAttribute>() == null);
                foreach (var fieldInfo in noSerializeFields)
                {
                    BsonMemberMap memberMap = classMap.MapMember(fieldInfo);
                    //如果为null或者默认值则忽略
                    if (fieldInfo.FieldType.IsValueType)
                    {
                        //值类型的默认值可以直接创建
                        memberMap.SetDefaultValue(Activator.CreateInstance(fieldInfo.FieldType));
                        memberMap.SetIgnoreIfDefault(true);
                    }
                    else
                    {
                        //引用类型的默认值为null
                        memberMap.SetIgnoreIfNull(true);
                    }
                }
                classMap = classMap.BaseClassMap;
            }
        }
    }
}
