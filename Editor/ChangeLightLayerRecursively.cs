using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that recursively changes the "Rendering Layer Mask" 
/// (Light Layers) of all models inside a specific folder.
/// This affects which lights can illuminate the models.
/// </summary>
public class ChangeRenderingLayerMaskRecursively : EditorWindow {

    // UI variables
    private string folderPath = "Assets";
    private uint renderingLayerMask = 1; // Default = Light Layer 0 (bit 0)

    // Creates the menu item under Tools in Unity Editor
    [MenuItem("Tools/Change Rendering Layer Mask Recursively")]
    public static void ShowWindow() {
        GetWindow<ChangeRenderingLayerMaskRecursively>("Rendering Layer Mask Changer");
    }

    // Draws the Editor Window GUI
    private void OnGUI() {
        GUILayout.Label("Change Rendering Layer Mask (Light Layers)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        folderPath = EditorGUILayout.TextField("Folder Path (in Assets)", folderPath);
        // ==================== DYNAMIC FOLDER PATH ====================
        EditorGUILayout.BeginHorizontal();

        //[OLD] folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        // Folder picker button
        if (GUILayout.Button("Select Folder", GUILayout.Width(110))) {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Models Folder", folderPath, "");

            if (!string.IsNullOrEmpty(selectedPath)) {
                // Convert absolute path to Unity relative path (Assets/...)
                if (selectedPath.StartsWith(Application.dataPath)) {
                    folderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else {
                    EditorUtility.DisplayDialog("Warning", "Please select a folder inside your Assets directory.", "OK");
                }
            }
        }


        EditorGUILayout.EndHorizontal();

        // Show current full path for clarity
        GUILayout.Label($"Full Path: {folderPath}", EditorStyles.helpBox);

        GUILayout.Space(10);

        // ==================== RENDERING LAYER MASK ====================
        GUILayout.Label("Rendering Layer Mask", EditorStyles.boldLabel);

        // Show individual layer toggles (most user-friendly)
        renderingLayerMask = DrawRenderingLayerMaskField(renderingLayerMask);

        GUILayout.Space(10);

        // Big button to start the process
        if (GUILayout.Button("Apply Rendering Layer Mask to All Models", GUILayout.Height(40))) {

            // Safety confirmation dialog
            if (EditorUtility.DisplayDialog("Confirm Action",
                $"This will change the Rendering Layer Mask of ALL MeshRenderers\n" +
                $"in folder: {folderPath} and all subfolders.\n\n" +
                $"New Mask: {System.Convert.ToString(renderingLayerMask, 2).PadLeft(32, '0')}\n\nContinue?",
                "Yes", "Cancel")) {
                ApplyRenderingLayerMaskRecursively();
            }
        }
    }

    //  UI with checkboxes for each light layer / Returns the new bitmask based on user selection.
    private uint DrawRenderingLayerMaskField(uint mask) {
        uint newMask = 0;

        for (int i = 0; i < 5; i++) { // i < 32
            bool isSet = (mask & (1u << i)) != 0;
            bool newSet = EditorGUILayout.ToggleLeft($"Light Layer {i} ", isSet); //bool newSet = EditorGUILayout.ToggleLeft($"Light Layer {i}  ({LayerMask.LayerToName(i)})", isSet);


            if (newSet)
                newMask |= (1u << i);
        }

        return newMask;
    }

    //Main function: Finds all prefabs recursively and updates their Rendering Layer Mask
    private void ApplyRenderingLayerMaskRecursively() {

        if (!folderPath.StartsWith("Assets")) {
            EditorUtility.DisplayDialog("Error", "Path must start with Assets/", "OK");
            return;
        }

        // Convert Unity path to system path
        string fullPath = Path.Combine(Application.dataPath, folderPath.Substring(7)); // remove "Assets/"

        if (!Directory.Exists(fullPath)) {
            EditorUtility.DisplayDialog("Error", $"Folder not found:\n{folderPath}", "OK");
            return;
        }

        int changedCount = 0;
        string[] prefabFiles = Directory.GetFiles(fullPath, "*.prefab", SearchOption.AllDirectories);

        foreach (string file in prefabFiles) {
            string assetPath = file.Replace(Application.dataPath, "Assets").Replace('\\', '/');
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) continue;

            bool prefabChanged = false;
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in renderers) {
                if (renderer.renderingLayerMask != renderingLayerMask) {
                    // Record change for Undo
                    Undo.RecordObject(renderer, "Change Rendering Layer Mask");
                    renderer.renderingLayerMask = renderingLayerMask;
                    prefabChanged = true;
                    changedCount++;
                }
            }

            // Mark prefab as dirty so Unity saves the changes
            if (prefabChanged)
                EditorUtility.SetDirty(prefab);
        }

        // Save all changes to disk
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Final feedback to user
        EditorUtility.DisplayDialog("Success",
            $"Processed {prefabFiles.Length} prefabs.\n" +
            $"Updated Rendering Layer Mask on {changedCount} MeshRenderers.", "OK");
    }
}
