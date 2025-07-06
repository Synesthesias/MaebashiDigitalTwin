using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChildObjectNameReplacer : EditorWindow
{
    private string prefixToAdd = "";
    private string suffixToAdd = "";
    private string replaceFrom = "";
    private string replaceTo = "";
    private List<Transform> selectedObjects = new List<Transform>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Child Object Name Replacer")]
    public static void ShowWindow()
    {
        GetWindow<ChildObjectNameReplacer>("Child Object Name Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Child Object Name Replacer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 選択されたオブジェクトの表示
        EditorGUILayout.LabelField("Selected Objects:", EditorStyles.boldLabel);
        if (Selection.transforms.Length > 0)
        {
            foreach (var obj in Selection.transforms)
            {
                EditorGUILayout.LabelField($"• {obj.name} ({obj.childCount} children)");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select one or more GameObjects in the Hierarchy.", MessageType.Info);
        }

        EditorGUILayout.Space();

        // プレフィックス追加
        GUILayout.Label("Add Prefix", EditorStyles.boldLabel);
        prefixToAdd = EditorGUILayout.TextField("Prefix:", prefixToAdd);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Prefix to All Children"))
        {
            AddPrefixToChildren();
        }
        if (GUILayout.Button("Remove Prefix from All Children"))
        {
            RemovePrefixFromChildren();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // サフィックス追加
        GUILayout.Label("Add Suffix", EditorStyles.boldLabel);
        suffixToAdd = EditorGUILayout.TextField("Suffix:", suffixToAdd);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Suffix to All Children"))
        {
            AddSuffixToChildren();
        }
        if (GUILayout.Button("Remove Suffix from All Children"))
        {
            RemoveSuffixFromChildren();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 文字列置換
        GUILayout.Label("Replace Text", EditorStyles.boldLabel);
        replaceFrom = EditorGUILayout.TextField("Replace From:", replaceFrom);
        replaceTo = EditorGUILayout.TextField("Replace To:", replaceTo);
        
        if (GUILayout.Button("Replace Text in All Children"))
        {
            ReplaceTextInChildren();
        }

        EditorGUILayout.Space();

        // プレビュー
        if (Selection.transforms.Length > 0)
        {
            GUILayout.Label("Preview (First 10 children):", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var parent in Selection.transforms)
            {
                EditorGUILayout.LabelField($"Parent: {parent.name}", EditorStyles.boldLabel);
                
                int count = 0;
                foreach (Transform child in parent)
                {
                    if (count >= 10) break;
                    
                    string currentName = child.name;
                    string previewName = GetPreviewName(currentName);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {currentName}", GUILayout.Width(200));
                    EditorGUILayout.LabelField("→", GUILayout.Width(20));
                    EditorGUILayout.LabelField(previewName, GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();
                    
                    count++;
                }
                
                if (parent.childCount > 10)
                {
                    EditorGUILayout.LabelField($"  ... and {parent.childCount - 10} more children");
                }
                
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private string GetPreviewName(string originalName)
    {
        string previewName = originalName;
        
        // プレフィックス追加のプレビュー
        if (!string.IsNullOrEmpty(prefixToAdd))
        {
            previewName = prefixToAdd + previewName;
        }
        
        // サフィックス追加のプレビュー
        if (!string.IsNullOrEmpty(suffixToAdd))
        {
            previewName = previewName + suffixToAdd;
        }
        
        // 文字列置換のプレビュー
        if (!string.IsNullOrEmpty(replaceFrom) && !string.IsNullOrEmpty(replaceTo))
        {
            previewName = previewName.Replace(replaceFrom, replaceTo);
        }
        
        return previewName;
    }

    private void AddPrefixToChildren()
    {
        if (string.IsNullOrEmpty(prefixToAdd))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a prefix to add.", "OK");
            return;
        }

        int totalChanged = 0;
        
        foreach (var parent in Selection.transforms)
        {
            foreach (Transform child in parent)
            {
                if (!child.name.StartsWith(prefixToAdd))
                {
                    Undo.RecordObject(child, "Add Prefix");
                    child.name = prefixToAdd + child.name;
                    totalChanged++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("Complete", $"Added prefix '{prefixToAdd}' to {totalChanged} child objects.", "OK");
    }

    private void RemovePrefixFromChildren()
    {
        if (string.IsNullOrEmpty(prefixToAdd))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a prefix to remove.", "OK");
            return;
        }

        int totalChanged = 0;
        
        foreach (var parent in Selection.transforms)
        {
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith(prefixToAdd))
                {
                    Undo.RecordObject(child, "Remove Prefix");
                    child.name = child.name.Substring(prefixToAdd.Length);
                    totalChanged++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("Complete", $"Removed prefix '{prefixToAdd}' from {totalChanged} child objects.", "OK");
    }

    private void AddSuffixToChildren()
    {
        if (string.IsNullOrEmpty(suffixToAdd))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a suffix to add.", "OK");
            return;
        }

        int totalChanged = 0;
        
        foreach (var parent in Selection.transforms)
        {
            foreach (Transform child in parent)
            {
                if (!child.name.EndsWith(suffixToAdd))
                {
                    Undo.RecordObject(child, "Add Suffix");
                    child.name = child.name + suffixToAdd;
                    totalChanged++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("Complete", $"Added suffix '{suffixToAdd}' to {totalChanged} child objects.", "OK");
    }

    private void RemoveSuffixFromChildren()
    {
        if (string.IsNullOrEmpty(suffixToAdd))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a suffix to remove.", "OK");
            return;
        }

        int totalChanged = 0;
        
        foreach (var parent in Selection.transforms)
        {
            foreach (Transform child in parent)
            {
                if (child.name.EndsWith(suffixToAdd))
                {
                    Undo.RecordObject(child, "Remove Suffix");
                    child.name = child.name.Substring(0, child.name.Length - suffixToAdd.Length);
                    totalChanged++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("Complete", $"Removed suffix '{suffixToAdd}' from {totalChanged} child objects.", "OK");
    }

    private void ReplaceTextInChildren()
    {
        if (string.IsNullOrEmpty(replaceFrom))
        {
            EditorUtility.DisplayDialog("Error", "Please enter text to replace.", "OK");
            return;
        }

        int totalChanged = 0;
        
        foreach (var parent in Selection.transforms)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(replaceFrom))
                {
                    Undo.RecordObject(child, "Replace Text");
                    child.name = child.name.Replace(replaceFrom, replaceTo);
                    totalChanged++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("Complete", $"Replaced '{replaceFrom}' with '{replaceTo}' in {totalChanged} child objects.", "OK");
    }
}