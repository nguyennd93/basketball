using System.Collections;
using System.Collections.Generic;
using Basketball.Utilities.Pool;
using UnityEngine;

namespace Basketball.Controller
{
    [System.Serializable]
    public struct AudioInfo
    {
        public string AudioId;
        public bool Loop;
        public AudioClip Clip;
        public float Volume;
    }

    public class SoundController : MonoBehaviour
    {
        [field: SerializeField] public AudioSource PrefabSource { get; private set; }

        [field: Header("Audio Map")]
        [field: SerializeField]
        public List<AudioInfo> AudioMap { get; private set; }

        public SoundController Instance { get; private set; }

        private readonly Dictionary<string, AudioInfo> _audioMap = new();
        private ObjectPool<AudioSource> _poolAudios;
        private readonly List<AudioSource> _activeAudios = new();

        void Awake()
        {
            Instance = this;
            _poolAudios = new(PrefabSource, this.transform);
        }

        public void Initialize()
        {
            foreach (var item in AudioMap)
            {
                _audioMap[item.AudioId] = item;
            }
        }

        public void Dispose()
        {
            StopAllCoroutines();
            foreach (var item in _activeAudios)
            {
                StartCoroutine(CRStopAudio(item, true));
            }
            _activeAudios.Clear();
        }

        public void PlayAudio(string audioId)
        {
            if (_audioMap.TryGetValue(audioId, out var info))
            {
                var audioItem = _poolAudios.Get();
                audioItem.clip = info.Clip;
                audioItem.volume = info.Volume;
                audioItem.loop = info.Loop;
                audioItem.Play();
                _activeAudios.Add(audioItem);

                if (!info.Loop)
                {
                    StartCoroutine(CRStopAudio(audioItem, false));
                }
            }
        }

        private IEnumerator CRStopAudio(AudioSource source, bool force = false)
        {
            while (!force && source.isPlaying)
            {
                yield return null;
            }

            if (!force) _activeAudios.Remove(source);
            _poolAudios.Store(source);
        }
    }
}