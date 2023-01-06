using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace jwellone
{
    public class SoundGroup : MonoBehaviour
    {
        [SerializeField] int _createSoundSourceCount = 2;
        [SerializeField, Range(0f, 1f)] float _volume = 1f;

        uint _id;
        readonly List<SoundSource> _soundSources = new List<SoundSource>();

        public float volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp(value, 0f, 1f);
                changeVolume?.Invoke();
            }
        }

        public float volumeRate
        {
            set
            {
                foreach (var source in _soundSources)
                {
                    source.volumeRate = value;
                }
            }
        }

        public Action? changeVolume { get; set; }
        public Action<SoundHandle>? playNotify { get; set; }
        public Action<SoundHandle>? playbackCompleteNotify { get; set; }
        public Action<SoundHandle>? stoppedCompleteNotify { get; set; }

        void Awake()
        {
            var sources = CreateSoundSource(_createSoundSourceCount);
            _soundSources.AddRange(sources);
        }

        protected virtual SoundSource[] CreateSoundSource(int count)
        {
            var sources = new SoundSource[count];
            for (var i = 0; i < _createSoundSourceCount; ++i)
            {
                var source = new GameObject("SoundSource").AddComponent<SoundSource>();
                source.transform.SetParent(transform, false);
                source.playNotify = (target) => playNotify?.Invoke(target.handle);
                source.playbackCompleteNotify = (target) => playbackCompleteNotify?.Invoke(target.handle);
                source.stoppedCompleteNotify = (target) => stoppedCompleteNotify?.Invoke(target.handle);
                sources[i] = source;
            }

            return sources;
        }

        public bool IsPlaying(SoundHandle handle)
        {
            foreach (var source in _soundSources)
            {
                if (source.handle == handle)
                {
                    return source.isPlaying;
                }
            }

            return false;
        }

        public bool IsPlaying()
        {
            foreach (var source in _soundSources)
            {
                if (source.isPlaying)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual SoundHandle Play(AudioClip? clip, float fadeTime = 0f, float delayTime = 0f)
        {
            return OnPlay(clip, fadeTime, delayTime, false);
        }

        public virtual SoundHandle PlayLoop(AudioClip? clip, float fadeTime = 0f, float delayTime = 0f)
        {
            return OnPlay(clip, fadeTime, delayTime, true);
        }

        public virtual void Stop(SoundHandle handle, float fadeTime = 0f, float delayTime = 0f)
        {
            foreach (var source in _soundSources)
            {
                if (source.handle == handle)
                {
                    source.Stop(fadeTime, delayTime);
                    return;
                }
            }
        }

        public virtual void StopAll(float fadeTime = 0f)
        {
            foreach (var source in _soundSources)
            {
                source.Stop(fadeTime, 0f);
            }
        }

        public virtual void Pause(SoundHandle handle)
        {
            foreach (var source in _soundSources)
            {
                if (source.handle == handle)
                {
                    source.Pause();
                    return;
                }
            }
        }

        public virtual void PauseAll()
        {
            foreach (var source in _soundSources)
            {
                source.Pause();
            }
        }

        public virtual void UnPause(SoundHandle handle)
        {
            foreach (var source in _soundSources)
            {
                if (source.handle == handle)
                {
                    source.UnPause();
                    return;
                }
            }
        }

        public virtual void UnPauseAll()
        {
            foreach (var source in _soundSources)
            {
                source.UnPause();
            }
        }

        SoundHandle OnPlay(AudioClip? clip, float fadeTime, float delayTime, bool isLoop)
        {
            if (clip == null)
            {
                return SoundHandle.None;
            }

            if (++_id == 0U)
            {
                _id = 1U;
            }

            var handle = new SoundHandle(GetHashCode(), _id);

            SoundSource? target = null;
            foreach (var source in _soundSources)
            {
                if (!source.isPlaying)
                {
                    target = source;
                    break;
                }
            }

            if (target == null)
            {
                target = _soundSources[0];
                for (var i = 1; i < _soundSources.Count; ++i)
                {
                    var source = _soundSources[i];
                    if (target.playbackStartTick > source.playbackStartTick)
                    {
                        target = source;
                    }
                }

                Debug.LogWarning($"[Sound]force stop {target.audioClip.name}.");
                target.Stop();
            }

            target.Play(handle, clip, fadeTime, delayTime, isLoop);

            return handle;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }

            volume = _volume;
        }
#endif
    }
}
