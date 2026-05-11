using Misc;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ShadowCreator))]
    public class ShadowCreatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var creator = (ShadowCreator)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Create Shadow"))
                creator.CreateShadow();

            if (GUILayout.Button("Remove Shadow"))
                creator.RemoveShadow();
        }
    }
}