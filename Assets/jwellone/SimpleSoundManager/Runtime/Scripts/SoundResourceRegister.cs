using UnityEngine;

#nullable enable

namespace jwellone
{
    public sealed class SoundResourceRegister : MonoBehaviour
    {
        [SerializeField] AudioClip[] _audioClips = null!;

        void Awake()
        {
            var soundManager = SimpleSoundManager.instance;
            foreach(var clip in _audioClips)
            {
                soundManager?.RegisterAudioClip(clip);
            }
        }

        void OnDestroy()
        {
            var soundManager = SimpleSoundManager.instance;
            foreach (var clip in _audioClips)
            {
                soundManager?.UnRegisterAudioClip(clip);
            }
        }
    }
}
