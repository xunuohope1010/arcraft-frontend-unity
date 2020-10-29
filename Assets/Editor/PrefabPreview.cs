using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
/*
[CustomPreview(typeof(GameObject))]
public class UIPreview : ObjectPreview
{
    Texture preview;

    const string cachePreviewPath = "CachePreviews";

    public override bool HasPreviewGUI()
    {
        return true;
    }


    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        base.OnPreviewGUI(r, background);
        if (target == null)
            return;

        var targetGameObject = target as GameObject;

        if (targetGameObject == null)
            return;


        GUI.Label(r, target.name + " is being previewed");
        GameObject obj = Resources.Load("Dirt") as GameObject;
        preview = AssetPreview.GetAssetPreview(obj);
        string guid = targetGameObject.name;
        string pathname = Path.Combine(cachePreviewPath, guid + ".png");
        SaveTexture2D(preview as Texture2D, Path.Combine(Application.dataPath, pathname));


    }

    public static bool SaveTexture2D(Texture2D png, string save_file_name)
    {
        byte[] bytes = png.EncodeToPNG();
        string directory = Path.GetDirectoryName(save_file_name);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        FileStream file = File.Open(save_file_name, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();

        return true;
    }

}
*/