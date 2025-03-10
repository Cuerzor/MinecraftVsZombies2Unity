using System;
using System.Linq;
using UnityEngine;

namespace MVZ2.Animation
{
    public class AnimationParticlePlayer : MonoBehaviour
    {
        public void Play(string name)
        {
            var particle = particles.FirstOrDefault(p => p.name == name);
            if (particle == null)
            {
                Debug.LogWarning($"AnimatorParticlePlayer on {gameObject.name} cannot find particle with name \"{name}\" to play!");
                return;
            }
            particle.particle.Play();
        }
        [SerializeField]
        private ParticlePair[] particles;
    }
    [Serializable]
    public class ParticlePair
    {
        public string name;
        public ParticleSystem particle;
    }
}
