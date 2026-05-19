#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public class EditorBootstrap
    {
        static EditorBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                const string mainMenuPath = "Assets/Scenes/MainMenu.unity";
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(mainMenuPath);
            }
        }
    }
}
#endif