#nullable enable

using PVZEngine;
using UnityEngine;

namespace MVZ2.Models
{
    public static class ModelFactories
    {
        public static Model? Create(NamespaceID? id, Camera camera, Transform parentTransform, int seed = 0)
        {
            if (factory == null)
            {
                return null;
            }
            return factory.CreateModel(id, camera, parentTransform, seed);
        }
        public static void SetFactory(IModelFactory value)
        {
            factory = value;
        }
        private static IModelFactory? factory;
    }
    public interface IModelFactory
    {
        public Model? CreateModel(NamespaceID? id, Camera camera, Transform parentTransform, int seed = 0);
    }
}