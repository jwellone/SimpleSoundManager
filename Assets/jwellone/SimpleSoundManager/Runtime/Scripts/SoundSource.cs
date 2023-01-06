using System;
using UnityEngine;

#nullable enable

namespace jwellone
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundSource : MonoBehaviour
    {
        public enum State
        {
            Wait = 0,
            PlayWait,
            Play,
            StopWait,
            Stop,
        }

        [SerializeField] AudioSource? _audioSource;

        bool _isPause = false;
        bool _isUpdateVolume;
        int _prevTimeSamples = 0;
        uint _playbackCompleteNum;
        float _volume = 1.0f;
        float _volumeRate = 1.0f;
        float _workTime;
        float _targetFade;
        float _targetDelay;
        float _targetVolumeRate;
        float _calcVolumeRate = 1.0f;
        SoundHandle _handle = SoundHandle.None;
        readonly FunctionStateMachine _stateMachine = new FunctionStateMachine();

        public State currentState => (State)_stateMachine.currentIndex;

        public State prevState => (State)_stateMachine.prevIndex;

        public int priority
        {
            get => _audioSource!.priority;
            set => _audioSource!.priority = value;
        }

        public float volume
        {
            get => _volume;
            set { _volume = value; _isUpdateVolume = true; }
        }

        public float volumeRate
        {
            get => _volumeRate;
            set { _volumeRate = value; _isUpdateVolume = true; }
        }

        public float blend3DRate
        {
            get => _audioSource!.spatialBlend;
            set => _audioSource!.spatialBlend = value;
        }

        public bool isPlaying => _audioSource!.isPlaying;

        public bool isLoop => _audioSource!.loop;

        public bool isMute
        {
            get => _audioSource!.mute;
            set => _audioSource!.mute = value;
        }

        public bool isPause
        {
            get => _isPause;
            private set
            {
                _isPause = value;
                if (_isPause)
                {
                    _audioSource!.Pause();
                }
                else
                {
                    _audioSource!.UnPause();
                }
            }
        }

        public long playbackStartTick
        {
            get;
            private set;
        }

        public uint playbackCompleteNum
        {
            get => _playbackCompleteNum;
            private set => _playbackCompleteNum = value;
        }

        public int timeSamples => _audioSource!.timeSamples;

        public SoundHandle handle => _handle;

        public AudioClip audioClip => _audioSource!.clip;

        public Action<SoundSource>? playNotify
        {
            get;
            set;
        }

        public Action<SoundSource>? playbackCompleteNotify
        {
            get;
            set;
        }

        public Action<SoundSource>? stoppedCompleteNotify
        {
            get;
            set;
        }

        void Awake()
        {
            _stateMachine.Init(
                    new[]
                    {
                        new FunctionStateMachine.Func(WaitEnter),
                        new FunctionStateMachine.Func(PlayWaitEnter, PlayWaitExecute),
                        new FunctionStateMachine.Func(PlayEnter, PlayExecute),
                        new FunctionStateMachine.Func(StopWaitEnter, StopWaitExecute),
                        new FunctionStateMachine.Func(StopEnter, StopExecute),
                    }, (int)State.Wait);

            _audioSource ??= gameObject.GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        void Update()
        {
            _stateMachine.Execute(Time.deltaTime);
        }

        public void Play(SoundHandle handle, AudioClip clip, float fadeTime = 0f, float delayTime = 0f, bool isLoop = false)
        {
            gameObject.SetActive(true);

            _handle = handle;
            _targetFade = fadeTime;
            _targetDelay = delayTime;
            _audioSource!.loop = isLoop;
            _audioSource!.clip = clip;
            playbackStartTick = DateTime.Now.Ticks;
            _stateMachine.ChangeState((int)State.PlayWait);
        }

        public void Stop(float fadeTime = 0f, float delayTime = 0f, bool isImmediateExecution = true)
        {
            switch ((State)_stateMachine.currentIndex)
            {
                case State.PlayWait:
                case State.Play:
                    break;

                default:
                    {
                        var nextState = (State)_stateMachine.nextIndex;
                        if (nextState == State.PlayWait)
                        {
                            break;
                        }
                    }
                    return;
            }

            _targetFade = fadeTime;
            _targetDelay = delayTime;
            _stateMachine.ChangeState((int)State.StopWait);

            if (isImmediateExecution)
            {
                _stateMachine.Execute(0f);
            }
        }

        public void Pause()
        {
            if (isPlaying)
            {
                isPause = true;
            }
        }

        public void UnPause()
        {
            if (isPause)
            {
                isPause = false;
            }
        }

        void WaitEnter()
        {
            _workTime = 0.0f;
            playbackStartTick = 0L;
            _playbackCompleteNum = 0u;
            playbackCompleteNotify = null;
            stoppedCompleteNotify = null;
            _audioSource!.clip = null;
            _handle = SoundHandle.None;
            gameObject.SetActive(false);
        }

        void PlayWaitEnter()
        {
            _workTime = 0.0f;
        }

        void PlayWaitExecute(float deltaTime)
        {
            if (!UpdateDelay(deltaTime))
            {
                _stateMachine.ChangeState((int)State.Play);
                _stateMachine.OnRepeat();
            }
        }

        void PlayEnter()
        {
            _isUpdateVolume = true;
            _workTime = 0.0f;
            _calcVolumeRate = 0.0f;
            _targetVolumeRate = 1.0f;
            _prevTimeSamples = 0;

            _audioSource!.Play();
            playNotify?.Invoke(this);
        }

        void PlayExecute(float deltaTime)
        {
            UpdateVolume(deltaTime, true);

            if (timeSamples < _prevTimeSamples)
            {
                ++_playbackCompleteNum;

                if (!isLoop)
                {
                    Stop(0.0f, 0.0f, false);
                    _stateMachine.OnRepeat();
                }
                else
                {
                    playbackCompleteNotify?.Invoke(this);
                }
            }

            _prevTimeSamples = timeSamples;
        }

        void StopWaitEnter()
        {
            _workTime = 0.0f;
        }

        void StopWaitExecute(float deltaTime)
        {
            if (!UpdateDelay(deltaTime))
            {
                _stateMachine.ChangeState((int)State.Stop);
                _stateMachine.OnRepeat();
            }
        }

        void StopEnter()
        {
            _isUpdateVolume = true;
            _targetVolumeRate = _calcVolumeRate;
            _workTime = _targetFade;
        }

        void StopExecute(float deltaTime)
        {
            if (!UpdateVolume(deltaTime, false) || (!isPlaying && 0 >= timeSamples))
            {
                _audioSource!.Stop();
                stoppedCompleteNotify?.Invoke(this);

                _stateMachine.ChangeState((int)State.Wait);
                _stateMachine.OnRepeat();
            }
        }

        bool UpdateDelay(float deltaTime)
        {
            if (isPause)
            {
                return true;
            }

            _workTime += deltaTime;
            var rate = (0.0f >= _targetDelay) ? 1.0f : Mathf.Min((_workTime / _targetDelay), 1.0f);

            return (1.0f > rate);
        }

        bool UpdateVolume(float deltaTime, bool isPlus)
        {
            if (isPause)
            {
                return true;
            }

            if (!_isUpdateVolume)
            {
                return false;
            }

            deltaTime = isPlus ? deltaTime : -deltaTime;
            var rate = (0.0f >= _targetFade) ? (isPlus ? 1.0f : 0.0f) : Mathf.Clamp((_workTime + deltaTime) / _targetFade, 0.0f, 1.0f);

            _workTime = _targetFade * rate;
            _calcVolumeRate = _targetVolumeRate * rate;

            var prev = _isUpdateVolume;
            if (0.0f >= rate)
            {
                _isUpdateVolume &= isPlus;
            }
            else if (1.0f <= rate)
            {
                _isUpdateVolume &= !isPlus;
            }

            _audioSource!.volume = volume * volumeRate * _calcVolumeRate;

            return _isUpdateVolume;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlaying && _audioSource == null)
            {
                _audioSource = gameObject.GetComponent<AudioSource>();
            }
        }
#endif
    }
}