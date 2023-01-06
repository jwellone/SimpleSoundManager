using System;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace jwellone
{
    public class SoundPlayComponent : MonoBehaviour
    {
        [Serializable]
        public class NotifyEvent : UnityEvent<SoundHandle>
        {
        }

        [SerializeField] bool _playOnStart;
        [SerializeField] AudioClip _audioClip = null!;
        [SerializeField] string _groupName = string.Empty;
        [SerializeField] float _fadeTime = 0f;
        [SerializeField] float _delayTime = 0f;
        [SerializeField] NotifyEvent _playEvent = new NotifyEvent();
        [SerializeField] NotifyEvent _playbackEvent = new NotifyEvent();
        [SerializeField] NotifyEvent _stopEvent = new NotifyEvent();

        SoundHandle _hPlay;
        SoundHandle _hStop;
        SimpleSoundManager? _manager => SimpleSoundManager.instance;

        public bool isPlaying => _manager?.IsPlaying(_hPlay) ?? false;

        public NotifyEvent playEvent => _playEvent;
        public NotifyEvent playbackEvent => _playbackEvent;
        public NotifyEvent stopEvent => _stopEvent;

        void Awake()
        {
            _manager?.RegisterAudioClip(_audioClip);
        }

        void OnEnable()
        {
            _manager?.RegisterPlayCallback(_groupName, OnPlay);
            _manager?.RegisterPlaybackCompleteCallback(_groupName, OnPlaybackComplete);
            _manager?.RegisterStopCallback(_groupName, OnStop);
        }

        void Start()
        {
            if (_playOnStart)
            {
                Play(_fadeTime, _delayTime);
            }
        }

        void OnDisable()
        {
            Stop();
            _manager?.UnRegisterStopCallback(_groupName, OnStop);
            _manager?.UnRegisterPlaybackCompleteCallback(_groupName, OnPlaybackComplete);
            _manager?.UnRegisterPlayCallback(_groupName, OnPlay);
        }

        void OnDestroy()
        {
            _manager?.UnRegisterAudioClip(_audioClip);
        }

        public SoundHandle Play(float fadeTime = 0f, float delayTime = 0f)
        {
            Stop(0f, 0f);
            _hPlay = _manager?.Play(_groupName, _audioClip.name, fadeTime, delayTime) ?? SoundHandle.None;
            return _hPlay;
        }

        public SoundHandle PlayLoop(float fadeTime = 0f, float delayTime = 0f)
        {
            Stop(0f, 0f);
            _hPlay = _manager?.PlayLoop(_groupName, _audioClip.name, fadeTime, delayTime) ?? SoundHandle.None;
            return _hPlay;
        }

        public void Stop(float fadeTime = 0f, float delayTime = 0f)
        {
            if (isPlaying)
            {
                _hStop = _hPlay;
                _manager?.Stop(_hStop, fadeTime, delayTime);
                _hPlay = SoundHandle.None;
            }
        }

        void OnPlay(SoundHandle handle)
        {
            if (_hPlay == handle)
            {
                _playEvent.Invoke(handle);
            }
        }

        void OnPlaybackComplete(SoundHandle handle)
        {
            if (_hPlay == handle)
            {
                _playbackEvent.Invoke(handle);
            }
        }

        void OnStop(SoundHandle handle)
        {
            if (_hStop == handle)
            {
                _stopEvent.Invoke(handle);
                _hStop = SoundHandle.None;
            }
            else if (_hPlay == handle)
            {
                _stopEvent.Invoke(handle);
                _hPlay = SoundHandle.None;
            }
        }
    }
}