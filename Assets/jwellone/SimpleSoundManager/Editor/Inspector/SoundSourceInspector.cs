using System;
using UnityEngine;
using UnityEditor;
using jwellone;

#nullable enable

namespace jwelloneEditor
{
    [CustomEditor(typeof(SoundSource))]
    class SoundSourceInspector : Editor
    {
        SerializedProperty? _audioSourceProperty;

        void OnEnable()
        {
            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_audioSourceProperty);

            serializedObject.ApplyModifiedProperties();

            var instance = (SoundSource)target;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("State");
            ++EditorGUI.indentLevel;
            EditorGUILayout.EnumFlagsField("current", instance.currentState);
            EditorGUILayout.EnumFlagsField("prev", instance.prevState);
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();

            EditorGUILayout.Toggle("playing", instance.isPlaying);
            EditorGUILayout.Toggle("loop", instance.isLoop);
            EditorGUILayout.Toggle("mute", instance.isMute);
            EditorGUILayout.Toggle("pause", instance.isPause);

            EditorGUILayout.Space();

            EditorGUILayout.FloatField("volume", instance.volume);
            EditorGUILayout.FloatField("volume rate", instance.volumeRate);

            EditorGUILayout.IntField("playbackCompoleteNum", (int)instance.playbackCompleteNum);
            EditorGUILayout.TextField("playbackStartTime", new DateTime(instance.playbackStartTick).ToString("yyyy/MM/dd/HH:mm:ss"));
            EditorGUILayout.IntField("timeSamples", instance.timeSamples);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Handle");
            ++EditorGUI.indentLevel;
            EditorGUILayout.TextField("groupHashCode", instance.handle.groupHashCode.ToString());
            EditorGUILayout.TextField("id", instance.handle.id.ToString());
            --EditorGUI.indentLevel;

            var audioSource = _audioSourceProperty!.objectReferenceValue as AudioSource;
            EditorGUILayout.ObjectField("Audio Clip", audioSource?.clip, typeof(AudioClip), false);

            EditorGUI.EndDisabledGroup();

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            EditorUtility.SetDirty(target);
        }
    }
}