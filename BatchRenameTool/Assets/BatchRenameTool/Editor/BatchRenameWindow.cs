using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace BatchRenameTool
{
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
        private enum CaseMode { None, Lowercase, Uppercase, TitleCase }
        private CaseMode caseMode = CaseMode.None;

        private enum SortingMode { Hierarchy, Alphabetical, ReverseAlphabetical };
        private SortingMode sortingMode = SortingMode.Hierarchy;
        private bool renameChildren = false;

        bool useRegex = false;
        string regexPattern = "";
        string regexReplace = "";

        string nameTemplate = "{name}";

        private enum NumberingMode { Global, RestartPerParent, SiblingsOnly, ChildrenOnly }
        private NumberingMode numberingMode = NumberingMode.Global;
        private Dictionary<Transform, int> parentCounters = new Dictionary<Transform, int>();


        // GUI
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

            // UI
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

                numberingMode = (NumberingMode)EditorGUILayout.EnumPopup("Numbering Mode", numberingMode);
            }

            EditorGUILayout.Space();

            caseMode = (CaseMode)EditorGUILayout.EnumPopup("Case Conversion", caseMode);

            EditorGUILayout.Space();

            sortingMode = (SortingMode)EditorGUILayout.EnumPopup("Sorting Mode", sortingMode);
            EditorGUILayout.Space();

            renameChildren = EditorGUILayout.Toggle("Rename Children", renameChildren);
            EditorGUILayout.Space();

            if (selectedCount > 0)
            {
                GeneratePreview();
            }

            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);

            int previewCount = Mathf.Min(5, previewNames.Length);
            for (int i = 0; i < previewCount; i++)
            {
                EditorGUILayout.LabelField($"• {previewNames[i]}");
            }

            if (previewNames.Length > 5)
            {
                EditorGUILayout.LabelField($"... and {previewNames.Length - 5} more");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Rename Selected Objects"))
            {
                RenameObjects();
            }

            if (GUILayout.Button("Reset Settings"))
            {
                ResetSettings();
            }
        }

        // Rename Logic
        private void RenameObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            // Sorting
            switch (sortingMode)
            {
                case SortingMode.Hierarchy:
                    System.Array.Sort(selectedObjects, (a, b) =>
                        a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                    break;

                case SortingMode.Alphabetical:
                    System.Array.Sort(selectedObjects, (a, b) =>
                        a.name.CompareTo(b.name));
                    break;

                case SortingMode.ReverseAlphabetical:
                    System.Array.Sort(selectedObjects, (a, b) =>
                        b.name.CompareTo(a.name));
                    break;
            }

            //Counters

            parentCounters.Clear();

            int number = startNumber;

            foreach (GameObject obj in selectedObjects)
            {
                RenameSingleObject(obj, ref number);

                if (renameChildren)
                    RenameChildrenRecursive(obj.transform, ref number);
            }
        }
        private void RenameSingleObject(GameObject obj, ref int number)
        {
            Undo.RecordObject(obj, "Batch Rename");

            string newName = obj.name;

            // Replace
            if (!string.IsNullOrEmpty(replaceFrom))
                newName = newName.Replace(replaceFrom, replaceTo);

            // Regex Replace
            if (useRegex && !string.IsNullOrEmpty(regexPattern))
            {
                newName = System.Text.RegularExpressions.Regex.Replace(newName, regexPattern, regexReplace);
            }

            // Prefix + Suffix
            newName = prefix + newName + suffix;

            // Numbering
            int currentNumber = number;

            switch (numberingMode)
            {
                case NumberingMode.Global:
                    currentNumber = number;
                    number++;
                    break;

                case NumberingMode.RestartPerParent:
                    Transform parent = obj.transform.parent;
                    if (!parentCounters.ContainsKey(parent))
                        parentCounters[parent] = startNumber;

                    currentNumber = parentCounters[parent];
                    parentCounters[parent]++;
                    break;

                case NumberingMode.SiblingsOnly:
                    currentNumber = obj.transform.GetSiblingIndex() + startNumber;
                    break;

                case NumberingMode.ChildrenOnly:
                    if (obj.transform.parent != null)
                        currentNumber = obj.transform.GetSiblingIndex() + startNumber;
                    else
                        currentNumber = 0;
                    break;
            }

            // Append number
            string num = currentNumber.ToString().PadLeft(numberPadding, '0');
            newName += "_" + num;


            // Case conversion
            switch (caseMode)
            {
                case CaseMode.Lowercase:
                    newName = newName.ToLower();
                    break;
                case CaseMode.Uppercase:
                    newName = newName.ToUpper();
                    break;
                case CaseMode.TitleCase:
                    newName = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(newName.ToLower());
                    break;
            }
            // Apply template
            if (!string.IsNullOrEmpty(nameTemplate))
            {
                string parentName = obj.transform.parent ? obj.transform.parent.name : "";
                string indexStr = num;
                string depthStr = obj.transform.GetSiblingIndex().ToString();

                newName = nameTemplate
                    .Replace("{name}", newName)
                    .Replace("{parent}", parentName)
                    .Replace("{index}", indexStr)
                    .Replace("{depth}", depthStr);
            }

            obj.name = newName;
        }

        private void RenameChildrenRecursive(Transform parent, ref int number)
        {
            foreach (Transform child in parent)
            {
                RenameSingleObject(child.gameObject, ref number);
                RenameChildrenRecursive(child, ref number);
            }
        }

        private void GeneratePreview()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            // Sorting
            switch (sortingMode)
            {
                case SortingMode.Hierarchy:
                    System.Array.Sort(selectedObjects, (a, b) =>
                        a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                    break;

                case SortingMode.Alphabetical:
                    System.Array.Sort(selectedObjects, (a, b) =>
                        a.name.CompareTo(b.name));
                    break;

                case SortingMode.ReverseAlphabetical:
                    System.Array.Sort(selectedObjects, (a, b) =>
                        b.name.CompareTo(a.name));
                    break;
            }

            parentCounters.Clear();

            List<string> names = new List<string>();
            int number = startNumber;

            foreach (GameObject obj in selectedObjects)
            {
                int depth = GetDepth(obj.transform);
                string indent = new string(' ', depth * 3);
                names.Add(indent + GeneratePreviewName(obj, ref number));


                if (renameChildren)
                    GeneratePreviewChildren(obj.transform, names, ref number);
            }

            previewNames = names.ToArray();
        }

        private string GeneratePreviewName(GameObject obj, ref int number)
        {
            string newName = obj.name;

            if (!string.IsNullOrEmpty(replaceFrom))
            {
                newName = newName.Replace(replaceFrom, replaceTo);
            }

            if (useRegex && !string.IsNullOrEmpty(regexPattern))
            {
                newName = System.Text.RegularExpressions.Regex.Replace(newName, regexPattern, regexReplace);
            }

            newName = prefix + newName + suffix;

            // Numbering
            int currentNumber = number;

            switch (numberingMode)
            {
                case NumberingMode.Global:
                    currentNumber = number;
                    number++;
                    break;

                case NumberingMode.RestartPerParent:
                    Transform parent = obj.transform.parent;
                    if (!parentCounters.ContainsKey(parent))
                        parentCounters[parent] = startNumber;

                    currentNumber = parentCounters[parent];
                    parentCounters[parent]++;
                    break;

                case NumberingMode.SiblingsOnly:
                    currentNumber = obj.transform.GetSiblingIndex() + startNumber;
                    break;

                case NumberingMode.ChildrenOnly:
                    if (obj.transform.parent != null)
                        currentNumber = obj.transform.GetSiblingIndex() + startNumber;
                    else
                        currentNumber = 0;
                    break;
            }

            string numStr = currentNumber.ToString().PadLeft(numberPadding, '0');
            newName += "_" + numStr;

            string indexStr = numStr;


            switch (caseMode)
            {
                case CaseMode.Lowercase:
                    newName = newName.ToLower();
                    break;

                case CaseMode.Uppercase:
                    newName = newName.ToUpper();
                    break;

                case CaseMode.TitleCase:
                    newName = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(newName.ToLower());
                    break;
            }
            // Apply template preview
            if (!string.IsNullOrEmpty(nameTemplate))
            {
                string parentName = obj.transform.parent ? obj.transform.parent.name : "";
                string depthStr = obj.transform.GetSiblingIndex().ToString();

                newName = nameTemplate
                    .Replace("{name}", newName)
                    .Replace("{parent}", parentName)
                    .Replace("{index}", indexStr)
                    .Replace("{depth}", depthStr);
            }

            return newName;
        }

        private void GeneratePreviewChildren(Transform parent, List<string> names, ref int number)
        {
            foreach (Transform child in parent)
            {
                int depth = GetDepth(child);
                string indent = new string(' ', depth * 3);
                names.Add(indent + GeneratePreviewName(child.gameObject, ref number));
                GeneratePreviewChildren(child, names, ref number);
            }
        }

        private void ResetSettings()
        {
            prefix = "";
            suffix = "";
            replaceFrom = "";
            replaceTo = "";
            useNumbering = false;
            startNumber = 1;
            numberPadding = 2;
            caseMode = CaseMode.None;
            sortingMode = SortingMode.Hierarchy;
            renameChildren = false;

            previewNames = new string[0];

            // Force UI refresh
            Repaint();

        }

        private int GetDepth(Transform t)
        {
            int depth = 0;
            while (t.parent != null)
            {
                depth++;
                t = t.parent;
            }
            return depth;
        }

    }
}