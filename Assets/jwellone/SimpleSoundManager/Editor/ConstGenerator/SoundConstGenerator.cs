using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
    public static class SoundConstGenerator
    {
        public static void Write(string path, string text)
        {
            var guids = AssetDatabase.FindAssets($"t:script {Path.GetFileNameWithoutExtension(path)}");
            var outputPath = guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : path;
            var sb = new StringBuilder();

            sb.AppendLine("// =================================================");
            sb.AppendLine("// This is an automatically generated file.");
            sb.AppendLine("// Unable to edit.");
            sb.AppendLine("// =================================================");

            sb.AppendLine("");

            sb.AppendLine(text);

            using (var stream = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                stream.NewLine = "\r\n";
                stream.Write(sb.ToString().Replace(Environment.NewLine, stream.NewLine));
                Debug.Log($"{outputPath} is write.");
            }
        }
    }
}
