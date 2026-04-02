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

        EditorGUILayout.Space();

        prefix = EditorGUILayout.TextField("Prefix", prefix);
        suffix = EditorGUILayout.TextField("Suffix", suffix);

        EditorGUILayout.Space();

        replaceFrom = EditorGUILayout.TextField("Replace From", replaceFrom);
        replaceTo = EditorGUILayout.TextField("Replace To", replaceTo);

        EditorGUILayout.Space();

        useNumbering = EditorGUILayout.Toggle("Use Numbering", useNumbering);
        if (useNumbering)
        {
            startNumber = EditorGUILayout.IntField("Start Number", startNumber);
            numberPadding = EditorGUILayout.IntField("Number Padding", numberPadding);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Rename Selected Objects"))
        {
            RenameObjects();
        }
    }
}
