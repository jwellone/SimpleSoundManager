using UnityEngine;

#nullable enable

namespace jwellone
{
    public class RegisteredAudioClip
    {
        int _registeredCount;
        public int registeredCount => _registeredCount;
        public readonly AudioClip clip;

        public RegisteredAudioClip(in AudioClip clip)
        {
            this.clip = clip;
        }

        public int Increment()
        {
            return ++_registeredCount;
        }

        public int Decrement()
        {
            return --_registeredCount;
        }
    }
}
