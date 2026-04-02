using UnityEngine;
using UnityEditor;

public class BatchRenameWindow : EditorWindow
{

    [MenuItem("Tools/Batch Rename Tool")]
    public static void ShowWindow()
    {
        GetWindow<BatchRenameWindow>("Batch Rename");
    }
    private string prefix = "";
    private string suffix = "";
    private string replaceFrom = "";
    private string replaceTo = "";
    private bool useNumbering = false;
    private int startNumber = 1;
    private int numberPadding = 2;

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
