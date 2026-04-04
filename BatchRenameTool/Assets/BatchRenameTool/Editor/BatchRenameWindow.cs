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

    private string[] previewNames = new string[0];

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

        //UI
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

    private void RenameObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        // Sort by hierarchy order for predictable numbering
        System.Array.Sort(selectedObjects, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex())
        );

        Undo.RecordObjects(selectedObjects, "Batch Rename");

        int number = startNumber;

        foreach (GameObject obj in selectedObjects)
        {
            string newName = obj.name;

            //Replace
            if (!string.IsNullOrEmpty(replaceFrom))
            {
                newName = newName.Replace(replaceFrom, replaceTo);
            }

            //Prefix and Suffix
            newName = prefix + newName + suffix;

            //Numbering
            if (useNumbering)
            {
                string num = number.ToString().PadLeft(numberPadding, '0');
                newName += "-" + num;
                number++;
            }

            obj.name = newName;
        }
    }

    private void GeneratePreview()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        System.Array.Sort(selectedObjects, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex())
        );

        previewNames = new string[selectedObjects.Length];

        int number = startNumber;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string newName = selectedObjects[i].name;

            if (!string.IsNullOrEmpty(replaceFrom))
            {
                newName = newName.Replace(replaceFrom, replaceTo);
            }

            newName = prefix + newName + suffix;

            if (useNumbering)
            {
                string num = number.ToString().PadLeft(numberPadding, '0');
                newName += "_" + num;
                number++;
            }

            previewNames[i] = newName;
        }
    }
}
