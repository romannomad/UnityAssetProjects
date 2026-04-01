using UnityEngine;
using UnityEditor;

public class BatchRenameWindow : EditorWindow
{
    [MenuItem("Tools/Batch Rename Tool")]

    public static void ShowWindow()
    {
        GetWindow<BatchRenameWindow>("Batch Rename");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Tool", EditorStyles.boldLabel);

        int selectedCount = Selection.gameObjects.Length;
        EditorGUILayout.LabelField("Selected Objects:", selectedCount.ToString());

        if (selectedCount == 0)
        {
            EditorGUILayout.HelpBox("Select one or more GameObjects in the Hierarchy to rename.", MessageType.Info);
        }
    }
}
