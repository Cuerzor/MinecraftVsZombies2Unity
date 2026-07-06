#nullable enable

using PVZEngine;
using UnityEngine;

namespace MVZ2Logic.Models
{
    public static class ModelFactories
    {
        public static Model? Create(NamespaceID? id, Camera camera, Transform parentTransform, int seed = 0)
        {
            var builder = GetModelBuilder(id, camera, seed);
            if (builder == null)
            {
                return null;
            }
            return builder.Build(parentTransform);
        }
        public static IModelBuilder? GetModelBuilder(NamespaceID? id, Camera camera, int seed = 0)
        {
            if (factory == null)
            {
                return null;
            }
            return factory.GetModelBuilder(id, camera, seed);
        }
        public static void SetFactory(IModelFactory value)
        {
            factory = value;
        }
        private static IModelFactory? factory;
    }
    public interface IModelFactory
    {
        public IModelBuilder? GetModelBuilder(NamespaceID? id, Camera camera, int seed = 0);
    }
}