using System.Collections.Generic;
using MVZ2Logic;
using MVZ2Logic.Serialization;
using NUnit.Framework;
using PVZEngine;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using PVZEngine.Tools.Random;
using UnityEngine;

namespace MVZ2.EditorTests
{
    public static class SerializationTests
    {
        [Test]
        public static void RNGTest()
        {
            SerializeHelper.init(Global.BuiltinNamespace);

            var rng = new RandomGenerator(1234);

            var json = rng.ToBson();
            var seri = SerializeHelper.FromBson<RandomGenerator>(json);
            var json2 = seri.ToBson();

            Debug.Log(json);
            Debug.Log(json2);
            Assert.AreEqual(json, json2);

            rng.Next();
            rng.Next();
            rng.Next();
            var json3 = rng.ToBson();
            seri.Next();
            seri.Next();
            seri.Next();
            var json4 = seri.ToBson();
            Debug.Log(json3);
            Debug.Log(json4);
            Assert.AreEqual(json3, json4);
        }
    }
}
