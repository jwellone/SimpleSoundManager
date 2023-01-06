using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace jwellone
{
    [DefaultExecutionOrder(-1)]
    public class SimpleSoundManager : SingletonMonoBehaviour<SimpleSoundManager>
    {
        [SerializeField, Range(0f, 1f)] float _masterVolume = 1f;
        [SerializeField] SoundGroup[] _soundGroupArray = null!;

        bool _isBackground;
        readonly Dictionary<string, SoundGroup> _soundGroupByName = new Dictionary<string, SoundGroup>();
        readonly Dictionary<int, SoundGroup> _soundGroupByHashCode = new Dictionary<int, SoundGroup>();
        readonly Dictionary<string, RegisteredAudioClip> _audioClips = new Dictionary<string, RegisteredAudioClip>();

        public float masterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp(value, 0f, 1f);
                UpdateGroupVolumeRate();
            }
        }

        protected override void OnAwakened()
        {
            foreach (var group in _soundGroupArray)
            {
                group.changeVolume += () =>
                {
                    UpdateGroupVolumeRate();
                };

                _soundGroupByName.Add(group.name, group);
                _soundGroupByHashCode.Add(group.GetHashCode(), group);
            }
        }

        void Start()
        {
            UpdateGroupVolumeRate();
        }

        protected override void OnDestroyed()
        {
        }

        void OnApplicationPause(bool pauseStatus)
        {
            OnBackground(pauseStatus);
        }

        void OnApplicationFocus(bool hasFocus)
        {
#if !UNITY_EDITOR
            OnBackground(!hasFocus);
#endif
        }

        void OnBackground(bool isBackground)
        {
            if (_isBackground == isBackground)
            {
                return;
            }

            _isBackground = isBackground;
            if (_isBackground)
            {
                PauseAll();
            }
            else
            {
                UnPauseAll();
            }
        }

        public void RegisterPlayCallback(string groupName, Action<SoundHandle> callback)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.playNotify += callback;
            }
        }

        public void UnRegisterPlayCallback(string groupName, Action<SoundHandle> callback)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.playNotify -= callback;
            }
        }

        public void RegisterPlaybackCompleteCallback(string groupName, Action<SoundHandle> callback)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.playbackCompleteNotify += callback;
            }
        }

        public void UnRegisterPlaybackCompleteCallback(string groupName, Action<SoundHandle> callback)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.playbackCompleteNotify -= callback;
            }
        }

        public void RegisterStopCallback(string groupName, Action<SoundHandle> callback)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.stoppedCompleteNotify += callback;
            }
        }

        public void UnRegisterStopCallback(string groupName, Action<SoundHandle> callback)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.stoppedCompleteNotify -= callback;
            }
        }

        public void RegisterAudioClip(AudioClip clip)
        {
            if (!_audioClips.ContainsKey(clip.name))
            {
                _audioClips.Add(clip.name, new RegisteredAudioClip(clip));
            }

            _audioClips[clip.name].Increment();
        }

        public void UnRegisterAudioClip(AudioClip clip)
        {
            if (!_audioClips.ContainsKey(clip.name))
            {
                return;
            }

            if (_audioClips[clip.name].Decrement() <= 0)
            {
                _audioClips.Remove(clip.name);
            }
        }

        public void SetVolume(string groupName, float volume)
        {
            var group = GetGroup(groupName);
            if (group != null)
            {
                group.volume = volume;
            }
        }

        public float GetVolume(string groupName)
        {
            return GetGroup(groupName)?.volume ?? 0f;
        }

        public bool IsPlaying(SoundHandle handle)
        {
            if (handle == SoundHandle.None)
            {
                return false;
            }
            return _soundGroupByName.Values.FirstOrDefault(_ => _.GetHashCode() == handle.groupHashCode)?.IsPlaying(handle) ?? false;
        }

        public bool IsPlaying(string groupName)
        {
            return GetGroup(groupName)?.IsPlaying() ?? false;
        }

        public SoundHandle Play(string groupName, string fileName, float fadeTime = 0f, float delayTime = 0f)
        {
            return GetGroup(groupName)?.Play(GetAudioClip(fileName), fadeTime, delayTime) ?? SoundHandle.None;
        }

        public SoundHandle PlayLoop(string groupName, string fileName, float fadeTime = 0f, float delayTime = 0f)
        {
            return GetGroup(groupName)?.PlayLoop(GetAudioClip(fileName), fadeTime, delayTime) ?? SoundHandle.None;
        }

        public void Stop(SoundHandle handle, float fadeTime = 0f, float delayTime = 0f)
        {
            GetGroup(handle.groupHashCode)?.Stop(handle, fadeTime, delayTime);
        }

        public void StopAll(string groupName, float fadeTime = 0f)
        {
            GetGroup(groupName)?.StopAll(fadeTime);
        }

        public void StopAll(float fadeTime = 0f)
        {
            foreach (var group in _soundGroupByName.Values)
            {
                group.StopAll(fadeTime);
            }
        }

        public void Pause(SoundHandle handle)
        {
            GetGroup(handle.groupHashCode)?.Pause(handle);
        }

        public void PauseAll(string groupName)
        {
            GetGroup(groupName)?.PauseAll();
        }

        public void PauseAll()
        {
            foreach (var group in _soundGroupByName.Values)
            {
                group.PauseAll();
            }
        }

        public void UnPause(SoundHandle handle)
        {
            GetGroup(handle.groupHashCode)?.UnPause(handle);
        }

        public void UnPauseAll(string groupName)
        {
            GetGroup(groupName)?.UnPauseAll();
        }

        public void UnPauseAll()
        {
            foreach (var group in _soundGroupByName.Values)
            {
                group.UnPauseAll();
            }
        }

        void UpdateGroupVolumeRate()
        {
            foreach (var group in _soundGroupByName.Values)
            {
                group.volumeRate = group.volume * _masterVolume;
            }
        }

        SoundGroup? GetGroup(int hashCode)
        {
            if (_soundGroupByHashCode.TryGetValue(hashCode, out var group))
            {
                return group;
            }

            return null;
        }

        SoundGroup? GetGroup(string groupName)
        {
            if (_soundGroupByName.TryGetValue(groupName, out var group))
            {
                return group;
            }

            Debug.LogWarning($"[Sound]{groupName} is not found.");
            return null;
        }

        AudioClip? GetAudioClip(string fileName)
        {
            if (_audioClips.TryGetValue(fileName, out var resource))
            {
                return resource.clip;
            }

            Debug.LogWarning($"[Sound]{fileName} audio clip is not found.");

            return null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }

            masterVolume = _masterVolume;
        }
#endif
    }
}