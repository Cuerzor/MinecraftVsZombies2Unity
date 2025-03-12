using System.Collections.Generic;
using System.Linq;
using MVZ2.Collision;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.Collisions
{
    public class UnityCollisionEntity : MonoBehaviour
    {
        public void Init(Entity entity)
        {
            Entity = entity;
            gameObject.name = Entity.ToString();
            gameObject.layer = UnityCollisionHelper.ToObjectLayer(Entity.Type);
            Simulate();
        }
        public void Simulate()
        {
            rigid.position = Entity.Position;

            foreach (var collider in colliders)
            {
                if (collider.Name == EntityCollisionHelper.NAME_MAIN)
                {
                    var size = Entity.GetScaledSize();
                    var offset = Vector3.up * (size.y * 0.5f);
                    collider.UpdateBounds(offset, size);
                }
                collider.Simulate();
            }
        }
        public UnityEntityCollider CreateCollider(string name)
        {
            var collider = Instantiate(colliderTemplate, colliderRoot).GetComponent<UnityEntityCollider>();
            collider.gameObject.SetActive(true);
            collider.gameObject.layer = gameObject.layer;
            collider.Init(Entity, name);
            colliders.Add(collider);
            return collider;
        }
        public bool DestroyCollider(string name)
        {
            var collider = colliders.FirstOrDefault(c => c.Name == name);
            if (collider)
            {
                Destroy(collider.gameObject);
            }
            return colliders.Remove(collider);
        }
        public void GetCollisions(List<EntityCollision> collisions)
        {
            foreach (var collider in colliders)
            {
                collider.GetCollisions(collisions);
            }
        }
        public void AddCollider(UnityEntityCollider collider)
        {
            colliders.Add(collider);
        }
        public bool RemoveCollider(string name)
        {
            var collider = colliders.FirstOrDefault(c => c.Name == name);
            return colliders.Remove(collider);
        }
        public UnityEntityCollider GetCollider(string name)
        {
            return colliders.FirstOrDefault(c => c.Name == name);
        }

        #region 序列化
        public SerializableUnityCollisionEntity ToSerializable()
        {
            return new SerializableUnityCollisionEntity()
            {
                id = Entity.ID,
                colliders = colliders.Select(c => c.ToSerializable()).ToArray()
            };
        }
        public void LoadFromSerializable(SerializableUnityCollisionEntity seri, Entity entity)
        {
            Entity = entity;
            foreach (var extraCollider in seri.colliders)
            {
                var collider = CreateCollider(extraCollider.name);
                collider.LoadFromSerializable(extraCollider, entity);
            }
        }
        public void LoadCollisions(LevelEngine level, SerializableUnityCollisionEntity seri)
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                var colliderSeri = seri.colliders[i];
                var collider = colliders[i];
                collider.LoadCollisions(level, colliderSeri);
            }
        }
        #endregion
        public Entity Entity { get; private set; }
        [SerializeField]
        private Rigidbody rigid;
        [SerializeField]
        private List<UnityEntityCollider> colliders = new List<UnityEntityCollider>();
        [SerializeField]
        private GameObject colliderTemplate;
        [SerializeField]
        private Transform colliderRoot;
    }
    public class SerializableUnityCollisionEntity
    {
        public long id;
        public SerializableUnityEntityCollider[] colliders;
    }
}
