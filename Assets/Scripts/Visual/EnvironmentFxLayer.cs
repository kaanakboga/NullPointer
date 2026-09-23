using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Environment FX Layer")]
    public sealed class EnvironmentFxLayer : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] _particles = System.Array.Empty<ParticleSystem>();
        [SerializeField] private GameObject[] _visualLayers = System.Array.Empty<GameObject>();
        [SerializeField] private bool _activeByDefault;

        public bool IsActive { get; private set; }

        public void Configure(ParticleSystem[] particles, GameObject[] visualLayers, bool activeByDefault)
        {
            _particles = particles ?? System.Array.Empty<ParticleSystem>();
            _visualLayers = visualLayers ?? System.Array.Empty<GameObject>();
            _activeByDefault = activeByDefault;
            SetActive(activeByDefault);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            foreach (GameObject layer in _visualLayers)
            {
                layer?.SetActive(active);
            }

            foreach (ParticleSystem particles in _particles)
            {
                if (particles == null)
                {
                    continue;
                }

                if (active)
                {
                    particles.Play(true);
                }
                else
                {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        private void Awake()
        {
            SetActive(_activeByDefault);
        }
    }
}
