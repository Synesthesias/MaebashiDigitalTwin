using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MaterialReplacer : EditorWindow
{
    private Material targetMaterial;
    private Material replacementMaterial;
    
    [MenuItem("Tools/Material Replacer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialReplacer>("Material Replacer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Material Replacer", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);
        replacementMaterial = (Material)EditorGUILayout.ObjectField("Replacement Material", replacementMaterial, typeof(Material), false);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Replace Materials in Selected Object"))
        {
            if (Selection.activeGameObject != null)
            {
                if (targetMaterial != null && replacementMaterial != null)
                {
                    ReplaceMaterials(Selection.activeGameObject);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign both target and replacement materials.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject in the hierarchy.", "OK");
            }
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Replace ALL Materials to Replacement Material"))
        {
            if (Selection.activeGameObject != null)
            {
                if (replacementMaterial != null)
                {
                    ReplaceAllMaterials(Selection.activeGameObject);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a replacement material.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject in the hierarchy.", "OK");
            }
        }
    }
    
    private void ReplaceMaterials(GameObject root)
    {
        int replacedCount = 0;
        
        Undo.RecordObject(root, "Replace Materials");
        
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            bool materialsChanged = false;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == targetMaterial)
                {
                    materials[i] = replacementMaterial;
                    materialsChanged = true;
                    replacedCount++;
                }
            }
            
            if (materialsChanged)
            {
                Undo.RecordObject(renderer, "Replace Materials");
                renderer.sharedMaterials = materials;
            }
        }
        
        EditorUtility.DisplayDialog("Complete", $"Replaced {replacedCount} materials.", "OK");
    }
    
    private void ReplaceAllMaterials(GameObject root)
    {
        int replacedCount = 0;
        
        Undo.RecordObject(root, "Replace All Materials");
        
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    materials[i] = replacementMaterial;
                    replacedCount++;
                }
            }
            
            Undo.RecordObject(renderer, "Replace All Materials");
            renderer.sharedMaterials = materials;
        }
        
        EditorUtility.DisplayDialog("Complete", $"Replaced {replacedCount} materials.", "OK");
    }
}