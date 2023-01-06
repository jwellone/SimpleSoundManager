using UnityEngine;

namespace jwellone
{
    public class SoundGroupBgm : SoundGroup
    {
        SoundHandle _currentHandle = SoundHandle.None;

        public override SoundHandle Play(AudioClip clip, float fadeTime = 0f, float delayTime = 0f)
        {
            var handle = base.Play(clip, fadeTime, delayTime);
            Stop(_currentHandle, fadeTime, delayTime);
            _currentHandle = handle;
            return _currentHandle;
        }

        public override SoundHandle PlayLoop(AudioClip clip, float fadeTime = 0f, float delayTime = 0f)
        {
            var handle = base.PlayLoop(clip, fadeTime, delayTime);
            Stop(_currentHandle, fadeTime, delayTime);
            _currentHandle = handle;
            return _currentHandle;
        }
    }
}