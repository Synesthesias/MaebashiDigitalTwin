using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MaterialTextureExtractor : EditorWindow
{
    private string outputFolder = "Assets/ExtractedAssets";
    private bool extractMaterials = true;
    private bool extractTextures = true;
    private bool overwriteExisting = false;
    private bool processSelectedOnly = false;
    
    [MenuItem("Tools/Material & Texture Extractor")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureExtractor>("Material & Texture Extractor");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Material & Texture Extractor", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 出力フォルダ設定
        GUILayout.Label("Output Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a folder within the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 抽出オプション
        GUILayout.Label("Extraction Options", EditorStyles.boldLabel);
        extractMaterials = EditorGUILayout.Toggle("Extract Materials", extractMaterials);
        extractTextures = EditorGUILayout.Toggle("Extract Textures", extractTextures);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Files", overwriteExisting);
        processSelectedOnly = EditorGUILayout.Toggle("Process Selected Objects Only", processSelectedOnly);
        
        GUILayout.Space(20);
        
        // 実行ボタン
        GUI.enabled = extractMaterials || extractTextures;
        if (GUILayout.Button("Extract Assets", GUILayout.Height(30)))
        {
            ExtractAssets();
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        // 情報表示
        EditorGUILayout.HelpBox(
            "This tool extracts embedded materials and textures from mesh assets in the scene.\n" +
            "- Materials will be saved as .mat files\n" +
            "- Textures will be saved as .png files\n" +
            "- Original references will be updated to use the extracted assets",
            MessageType.Info);
    }
    
    private void ExtractAssets()
    {
        try
        {
            // 出力フォルダを作成
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            
            // 処理対象のオブジェクトを取得
            GameObject[] targetObjects = processSelectedOnly ? 
                Selection.gameObjects : 
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            var extractedMaterials = new Dictionary<Material, string>();
            var extractedTextures = new Dictionary<Texture2D, string>();
            int materialCount = 0;
            int textureCount = 0;
            
            EditorUtility.DisplayProgressBar("Extracting Assets", "Processing...", 0f);
            
            // 全てのレンダラーコンポーネントを取得
            var renderers = new List<Renderer>();
            foreach (var rootObj in targetObjects)
            {
                renderers.AddRange(rootObj.GetComponentsInChildren<Renderer>(true));
            }
            
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                EditorUtility.DisplayProgressBar("Extracting Assets", 
                    $"Processing {renderer.name}", (float)i / renderers.Count);
                
                var materials = renderer.sharedMaterials;
                var newMaterials = new Material[materials.Length];
                
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] == null) continue;
                    
                    Material material = materials[j];
                    Material workingMaterial = material;
                    
                    // テクスチャの抽出を先に行う（元のマテリアルから）
                    if (extractTextures)
                    {
                        ExtractTexturesFromMaterial(material, extractedTextures, ref textureCount);
                    }
                    
                    // マテリアルが埋め込まれている場合
                    if (IsEmbeddedAsset(material))
                    {
                        if (extractMaterials)
                        {
                            if (!extractedMaterials.ContainsKey(material))
                            {
                                // テクスチャ抽出後にマテリアルを抽出
                                string materialPath = ExtractMaterial(material, renderer.name, j);
                                extractedMaterials[material] = materialPath;
                                materialCount++;
                            }
                            workingMaterial = AssetDatabase.LoadAssetAtPath<Material>(extractedMaterials[material]);
                            newMaterials[j] = workingMaterial;
                        }
                        else
                        {
                            newMaterials[j] = material;
                        }
                    }
                    else
                    {
                        newMaterials[j] = material;
                    }
                }
                
                // マテリアルの更新
                if (extractMaterials)
                {
                    renderer.sharedMaterials = newMaterials;
                    EditorUtility.SetDirty(renderer);
                }
            }
            
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Extraction Complete", 
                $"Extracted {materialCount} materials and {textureCount} textures to:\n{outputFolder}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", $"An error occurred during extraction:\n{e.Message}", "OK");
        }
    }
    
    private bool IsEmbeddedAsset(Object asset)
    {
        if (asset == null) return false;
        string assetPath = AssetDatabase.GetAssetPath(asset);
        
        // アセットパスが空の場合は確実に埋め込まれている
        if (string.IsNullOrEmpty(assetPath)) return true;
        
        // 3Dモデルファイル内に含まれている場合
        if (assetPath.Contains(".fbx") || assetPath.Contains(".obj") || 
            assetPath.Contains(".dae") || assetPath.Contains(".3ds") || 
            assetPath.Contains(".blend") || assetPath.Contains(".max"))
        {
            return true;
        }
        
        // サブアセットかどうかをチェック
        Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (mainAsset != null && mainAsset != asset)
        {
            return true;
        }
        
        return false;
    }
    
    private string ExtractMaterial(Material material, string rendererName, int materialIndex)
    {
        string materialName = string.IsNullOrEmpty(material.name) ? 
            $"{rendererName}_Material_{materialIndex}" : 
            SanitizeFileName(material.name);
        
        string materialPath = Path.Combine(outputFolder, "Materials", $"{materialName}.mat");
        
        // フォルダ作成
        string materialDir = Path.GetDirectoryName(materialPath);
        if (!Directory.Exists(materialDir))
        {
            Directory.CreateDirectory(materialDir);
        }
        
        // 既存ファイルのチェック
        if (File.Exists(materialPath) && !overwriteExisting)
        {
            materialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
        }
        
        // マテリアルのコピーを作成（テクスチャ参照を含む）
        Material newMaterial = new Material(material);
        
        // 既に抽出されたテクスチャがあれば、参照を更新
        UpdateMaterialTextureReferences(newMaterial);
        
        AssetDatabase.CreateAsset(newMaterial, materialPath);
        AssetDatabase.ImportAsset(materialPath);
        
        return materialPath;
    }
    
    private void UpdateMaterialTextureReferences(Material material)
    {
        var shader = material.shader;
        if (shader == null) return;
        
        // 既に抽出済みのテクスチャファイルをチェックして参照を更新
        string texturesFolder = Path.Combine(outputFolder, "Textures");
        if (!Directory.Exists(texturesFolder)) return;
        
        string[] commonTextureProperties = {
            "_MainTex", "_BaseMap", "_AlbedoMap", "_DiffuseMap",
            "_BumpMap", "_NormalMap", "_MetallicGlossMap", "_SpecGlossMap",
            "_OcclusionMap", "_EmissionMap", "_DetailAlbedoMap", "_DetailNormalMap"
        };
        
        foreach (string propertyName in commonTextureProperties)
        {
            if (material.HasProperty(propertyName))
            {
                Texture2D currentTexture = material.GetTexture(propertyName) as Texture2D;
                if (currentTexture != null && IsEmbeddedAsset(currentTexture))
                {
                    // 対応する抽出済みテクスチャファイルを探す
                    string textureName = string.IsNullOrEmpty(currentTexture.name) ? 
                        $"Texture_{propertyName}" : 
                        SanitizeFileName(currentTexture.name);
                    
                    string[] possiblePaths = {
                        Path.Combine(texturesFolder, $"{textureName}.png"),
                        Path.Combine(texturesFolder, $"{currentTexture.name}.png")
                    };
                    
                    foreach (string texturePath in possiblePaths)
                    {
                        if (File.Exists(texturePath))
                        {
                            Texture2D extractedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                            if (extractedTexture != null)
                            {
                                material.SetTexture(propertyName, extractedTexture);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void ExtractTexturesFromMaterial(Material material, Dictionary<Texture2D, string> extractedTextures, ref int textureCount)
    {
        var shader = material.shader;
        if (shader == null) return;
        
        // よく使われるテクスチャプロパティ名
        string[] commonTextureProperties = {
            "_MainTex", "_BaseMap", "_AlbedoMap", "_DiffuseMap",
            "_BumpMap", "_NormalMap", "_MetallicGlossMap", "_SpecGlossMap",
            "_OcclusionMap", "_EmissionMap", "_DetailAlbedoMap", "_DetailNormalMap"
        };
        
        // 共通のテクスチャプロパティをチェック
        foreach (string propertyName in commonTextureProperties)
        {
            if (material.HasProperty(propertyName))
            {
                Texture2D texture = material.GetTexture(propertyName) as Texture2D;
                if (texture != null && IsEmbeddedAsset(texture))
                {
                    ProcessTexture(material, texture, propertyName, extractedTextures, ref textureCount);
                }
            }
        }
        
        // シェーダーの全テクスチャプロパティもチェック
        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                
                // 既にチェック済みの場合はスキップ
                if (System.Array.IndexOf(commonTextureProperties, propertyName) >= 0) continue;
                
                Texture2D texture = material.GetTexture(propertyName) as Texture2D;
                if (texture != null && IsEmbeddedAsset(texture))
                {
                    ProcessTexture(material, texture, propertyName, extractedTextures, ref textureCount);
                }
            }
        }
    }
    
    private void ProcessTexture(Material material, Texture2D texture, string propertyName, Dictionary<Texture2D, string> extractedTextures, ref int textureCount)
    {
        if (!extractedTextures.ContainsKey(texture))
        {
            string texturePath = ExtractTexture(texture, propertyName);
            if (!string.IsNullOrEmpty(texturePath))
            {
                extractedTextures[texture] = texturePath;
                textureCount++;
                
                // AssetDatabaseを更新してからロード
                AssetDatabase.ImportAsset(texturePath);
                Texture2D extractedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (extractedTexture != null)
                {
                    material.SetTexture(propertyName, extractedTexture);
                    EditorUtility.SetDirty(material);
                }
            }
        }
        else
        {
            // 既に抽出済みのテクスチャを使用
            Texture2D extractedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(extractedTextures[texture]);
            if (extractedTexture != null)
            {
                material.SetTexture(propertyName, extractedTexture);
                EditorUtility.SetDirty(material);
            }
        }
    }
    
    private string ExtractTexture(Texture2D texture, string propertyName)
    {
        string textureName = string.IsNullOrEmpty(texture.name) ? 
            $"Texture_{propertyName}" : 
            SanitizeFileName(texture.name);
        
        string texturePath = Path.Combine(outputFolder, "Textures", $"{textureName}.png");
        
        // フォルダ作成
        string textureDir = Path.GetDirectoryName(texturePath);
        if (!Directory.Exists(textureDir))
        {
            Directory.CreateDirectory(textureDir);
        }
        
        // 既存ファイルのチェック
        if (File.Exists(texturePath) && !overwriteExisting)
        {
            texturePath = AssetDatabase.GenerateUniqueAssetPath(texturePath);
        }
        
        try
        {
            // テクスチャの読み取り可能性をチェック
            bool wasReadable = texture.isReadable;
            TextureImporter importer = null;
            string originalPath = AssetDatabase.GetAssetPath(texture);
            
            // 埋め込まれたテクスチャの場合、一時的に読み取り可能にする
            if (!wasReadable && !string.IsNullOrEmpty(originalPath))
            {
                importer = AssetImporter.GetAtPath(originalPath) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    AssetDatabase.ImportAsset(originalPath);
                }
            }
            
            // RenderTextureを使用してテクスチャをコピー
            RenderTexture renderTex = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, renderTex);
            
            RenderTexture.active = renderTex;
            Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTex);
            
            // PNGとして保存
            byte[] pngData = readableTexture.EncodeToPNG();
            if (pngData != null && pngData.Length > 0)
            {
                File.WriteAllBytes(texturePath, pngData);
            }
            else
            {
                Debug.LogError($"Failed to encode texture {texture.name} to PNG");
                return null;
            }
            
            // 一時的なテクスチャを破棄
            DestroyImmediate(readableTexture);
            
            // 元の読み取り設定を復元
            if (!wasReadable && importer != null)
            {
                importer.isReadable = false;
                AssetDatabase.ImportAsset(originalPath);
            }
            
            // インポート設定を適用
            AssetDatabase.ImportAsset(texturePath);
            TextureImporter newImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (newImporter != null)
            {
                // 元のテクスチャの設定をコピー
                if (importer != null)
                {
                    newImporter.wrapMode = importer.wrapMode;
                    newImporter.filterMode = importer.filterMode;
                    newImporter.anisoLevel = importer.anisoLevel;
                    newImporter.mipmapEnabled = importer.mipmapEnabled;
                }
                else
                {
                    // デフォルト設定
                    newImporter.wrapMode = texture.wrapMode;
                    newImporter.filterMode = texture.filterMode;
                    newImporter.anisoLevel = texture.anisoLevel;
                }
                
                AssetDatabase.ImportAsset(texturePath);
            }
            
            return texturePath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to extract texture {texture.name}: {e.Message}");
            return null;
        }
    }
    
    private void SaveTextureAsAsset(Texture2D texture, string path)
    {
        // この関数は不要になったため削除
        // RenderTextureを使用した新しい方式で処理
    }
    
    private string SanitizeFileName(string fileName)
    {
        string sanitized = fileName;
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }
        return sanitized;
    }
}