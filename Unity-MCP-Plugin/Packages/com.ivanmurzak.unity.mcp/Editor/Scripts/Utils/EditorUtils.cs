/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

namespace com.IvanMurzak.Unity.MCP.Editor.Utils
{
    /// <summary>
    /// Utility class for Unity Editor operations.
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// Repaints all editor windows including Project, Hierarchy, Animation, and all views.
        /// </summary>
        public static void RepaintAllEditorWindows()
        {
            UnityEditor.EditorApplication.RepaintProjectWindow();
            UnityEditor.EditorApplication.RepaintHierarchyWindow();
            UnityEditor.EditorApplication.RepaintAnimationWindow();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        static System.Reflection.MethodInfo? savePrefabMethod;

        /// <summary>
        /// Saves a prefab stage the way the editor's own Save button does, so <see cref="UnityEditor.SceneManagement.PrefabStage.prefabSaving"/>
        /// and <see cref="UnityEditor.SceneManagement.PrefabStage.prefabSaved"/> fire. A plain <c>PrefabUtility.SaveAsPrefabAsset</c>
        /// raises neither, so editor-only state that listeners strip on save (such as localization preview text) is written into the asset.
        /// </summary>
        public static void SavePrefabStage(UnityEditor.SceneManagement.PrefabStage prefabStage)
        {
            savePrefabMethod ??= typeof(UnityEditor.SceneManagement.PrefabStage).GetMethod("SavePrefab",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, System.Type.EmptyTypes, null)
                ?? throw new System.MissingMethodException(typeof(UnityEditor.SceneManagement.PrefabStage).FullName, "SavePrefab");

            if (!(bool)savePrefabMethod.Invoke(prefabStage, null))
                throw new System.InvalidOperationException($"Saving prefab stage '{prefabStage.assetPath}' failed.");
        }
    }
}
