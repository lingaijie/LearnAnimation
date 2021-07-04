using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using System.Text.RegularExpressions;

public class AJEditorScripts
{
#region Public Function
    static List<string> SelectedPath()
    {
        var guids = Selection.assetGUIDs;
        List<string> assetPath = new List<string>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                assetPath.AddRange(GetChildPath(path));
            }
            else
            {
                assetPath.Add(path);
            }
        }
#if LING
        foreach (var temp in assetPath)
        {
            Debug.Log(temp);
        }
#endif
        return assetPath;
    }

    static List<string> GetChildPath(string parent)
    {
        var files = Directory.GetFiles(parent).ToList().Where(p => !p.EndsWith(".meta")).ToList();
        var dirs = Directory.GetDirectories(parent);
        for (int i = 0; i < dirs.Length; i++)
        {
            files.AddRange(GetChildPath(dirs[i]));
        }
        return files;
    }
    public static void ModifyMeshReadable(string path, bool refreshAssets = true)
    {
#if LING
        Debug.Log("ModifyMeshReadable："+path);
#endif
        ModelImporter modelImporter = ModelImporter.GetAtPath(path) as ModelImporter;
        if (modelImporter == null) return;
        if (modelImporter.isReadable)
        {
            modelImporter.isReadable = false;
            modelImporter.SaveAndReimport();
            if (refreshAssets) AssetDatabase.Refresh();
        }
    }

    static Dictionary<int, List<string>> dicLayers = new Dictionary<int, List<string>>();
    [MenuItem("Assets/打印layer")]
    static void GetAllPrefab()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        dicLayers.Clear();
        for (int i = 0; i < guids.Length; i++)
        {

            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (go != null)
            {
                if (!dicLayers.ContainsKey(go.layer))
                {
                    dicLayers.Add(go.layer, new List<string> { path });
                }
                else
                {
                    dicLayers[go.layer].Add(path);
                }
            }
        }
        TxtLayerLog();
    }

    static void TxtLayerLog()
    {

        if (dicLayers.Count > 0)
        {
            string path = "Assets/LayerLog.txt";
            StreamWriter sw = new StreamWriter(path);
            foreach (var temp in dicLayers)
            {
                sw.WriteLine(LayerMask.LayerToName(temp.Key));
                for (int i = 0; i < temp.Value.Count; i++)
                {
                    sw.WriteLine(i.ToString() + ":" + temp.Value[i]);
                }
                sw.WriteLine("\n");
            }
            sw.Flush();
            sw.Close();
        }
    }
    static Dictionary<string, string> dicfbxinfo = new Dictionary<string, string>();
    static List<AnimationClip> GetClips(List<string> aPath)
    {
        List<AnimationClip> listClips = new List<AnimationClip>();
        dicfbxinfo = new Dictionary<string, string>();
        for (int i = 0; i < aPath.Count; i++)
        {
            if (aPath[i].EndsWith(".anim"))
            {
                var clip = AssetDatabase.LoadAssetAtPath(aPath[i], typeof(AnimationClip)) as AnimationClip;
                listClips.Add(clip);
            }
            else if (aPath[i].ToLower().EndsWith(".fbx"))
            {
                listClips.AddRange(CopyClips(aPath[i]));
            }
        }
        return listClips;
    }

    public static List<Mesh> GetMesh(List<string> path)
    {
        List<Mesh> listmesh = new List<Mesh>();
        for (int i = 0; i < path.Count; i++)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(path[i]);
            foreach (var o in objects)
            {
                if (o is Mesh)
                {
                    listmesh.Add((Mesh)o);
                }
            }
        }
        return listmesh;
    }

    public static List<AnimationClip> CopyClips(string path)
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
        string folder = Path.GetDirectoryName(path);
        string fbxguid = AssetDatabase.AssetPathToGUID(path);
        string aniguid = null;
        foreach (var o in objects)
        {
            if (o is AnimationClip)
            {
                if (o.name.Contains("__preview__Take")) continue; //我也不知道这是啥，每个fbx中都包含一个这个clip文件
                AnimationClip clip = new AnimationClip();
                EditorUtility.CopySerialized((AnimationClip)o, clip);
                string savepath = folder + "\\" + o.name + ".anim";
                AssetDatabase.CreateAsset(clip, savepath);
                EditorUtility.SetDirty(clip);
                clips.Add(clip);
                aniguid = AssetDatabase.AssetPathToGUID(savepath);
            }
        }
        if (!dicfbxinfo.ContainsKey(fbxguid))
        {
            dicfbxinfo.Add(fbxguid, aniguid);
        }
        else
        {
            Debug.Log("出现具有相同的guid的fbx文件了");
        }
        return clips;
    }
#endregion

#region 面板显示
    [MenuItem("Assets/优化/压缩动画(scale)")]
    static void CompressAnimation()
    {
        List<string> aPath = SelectedPath();
        List<AnimationClip> listClips = GetAllAnimationClips();
        int max = listClips.Count;
        for (int i = 0; i < listClips.Count; i++)
        {
            EditorUtility.DisplayProgressBar("压缩动画中", $"{i}/{max}", (float)i / max);
            CompressAnim(listClips[i], "scale");
            EditorUtility.SetDirty(listClips[i]);
        }
        EditorUtility.ClearProgressBar();
        ReplaceAnimator();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); 
    }
    [MenuItem("Assets/优化/压缩动画(scale-postion)")]
    static void CompressAnimation1()
    {
        List<string> aPath = SelectedPath();
        //List<AnimationClip> listClips = GetClips(aPath);
        List<AnimationClip> listClips = GetAllAnimationClips();
        int max = listClips.Count;
        for (int i = 0; i < listClips.Count; i++)
        {
            EditorUtility.DisplayProgressBar("压缩动画中", $"{i}/{max}", (float)i / max);
            CompressAnim(listClips[i], "scale");
            CompressAnim(listClips[i], "position");
            EditorUtility.SetDirty(listClips[i]);
        }
        EditorUtility.ClearProgressBar();
        ReplaceAnimator();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();   
    }
    [MenuItem("Assets/优化/压缩Bip001动画(scale)")]
    static void CompressBip001AnimationScale()
    {
        List<string> aPath = SelectedPath();
        List<AnimationClip> listClips = GetAllAnimationClips();
        int max = listClips.Count;
        for (int i = 0; i < listClips.Count; i++)
        {
            EditorUtility.DisplayProgressBar("压缩动画中", $"{i}/{max}", (float)i / max);
            ReduceKeys(listClips[i], new string[] {"scale" },"bip001");
            EditorUtility.SetDirty(listClips[i]);
        }
        EditorUtility.ClearProgressBar();
        ReplaceAnimator();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/优化/压缩Bip001动画(scale-postion)")]
    static void CompressBip001AnimationSP()
    {
        List<string> aPath = SelectedPath();
        List<AnimationClip> listClips = GetAllAnimationClips();
        int max = listClips.Count;
        for (int i = 0; i < listClips.Count; i++)
        {
            EditorUtility.DisplayProgressBar("压缩动画中", $"{i}/{max}", (float)i / max);
            ReduceKeys(listClips[i], new string[] { "postion", "scale" }, "bip001");
            EditorUtility.SetDirty(listClips[i]);
        }
        EditorUtility.ClearProgressBar();
        ReplaceAnimator();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/优化/关闭MeshRW")]
    public static void CloseMeshReadable()
    {
        var listPath = SelectedPath();
        for(int i=0;i<listPath.Count;i++)
        {
            ModifyMeshReadable(listPath[i]);
        }
    }

    //[MenuItem("Assets/优化/场景格式批处理")]
    //public static void ImportSceneModel()
    //{
    //    var listPath = SelectedPath();
    //    for(int i=0;i<listPath.Count;i++)
    //    {
    //        if(listPath[i].ToLower().Contains(".fbx"))
    //        {
    //            ModelImporter modelImporter = ModelImporter.GetAtPath(listPath[i]) as ModelImporter;
    //            ImportConfig config = new ImportConfig();
    //            var fields = config.GetType().GetFields();
    //            foreach(var t in fields)
    //            {
    //                var modelpro = modelImporter.GetType().GetProperty(t.Name);
    //                if(modelpro==null)
    //                {
    //                    Debug.Log(t.Name + "Is Null");
    //                }
    //                var tv = modelpro.GetValue(modelImporter);
    //                modelpro.SetValue(modelImporter, t.GetValue(config));
    //                Debug.Log(t.Name + ":" + t.GetValue(config));
    //            }
    //        }
    //    }
    //}

#endregion

#region 打包
    static string resPath = "Assets/";
    [MenuItem("Assets/打包")]
    public static void Build()
    {
        string outPutPath = "Assets/StreamAssets";
        BuildPipeline.BuildAssetBundles(outPutPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
    [MenuItem("Assets/设置Name")]
    public static void SetLastDirABName()
    {
        string outPutPath = resPath;
        List<string> lastDirs = new List<string>();
        lastDirs = GetLastDir(outPutPath);

        if (lastDirs.Count < 1)
        {
            EditorUtility.DisplayDialog("警告", "LastDir为空", "OK");
        }

        int maxcount = lastDirs.Count;
        for (int i = 0; i < lastDirs.Count; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("SetABName", "Progress", (float)i / maxcount);
            SetABName(lastDirs[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    private static void SetABName(string floder)
    {
        var fiels = Directory.GetFiles(floder);
        int length = resPath.Length;
        if (floder.Length <= resPath.Length) return;
        string abName = floder.Remove(0, length);
        foreach (var f in fiels)
        {
            if (f.EndsWith(".meta") || f.EndsWith(".cs")) continue;
            var item = AssetImporter.GetAtPath(f);
            if (item != null)
            {
                string ABName = abName + ".ab";
                item.SetAssetBundleNameAndVariant(abName, "ab");
            }
        }
    }

    public static List<string> GetLastDir(string path)
    {
        var dirs = Directory.GetDirectories(path);
        if (dirs.Length < 1)
        {
            return new List<string> { path };
        }
        List<string> dirss = new List<string>();
        if (Directory.GetFiles(path).Length > 0) dirss.Add(path);
        for (int i = 0; i < dirs.Length; i++)
        {
            dirss.AddRange(GetLastDir(dirs[i]));
        }
        return dirss;
    }
#endregion

#region 压缩动画

    static Dictionary<string, List<FbxAnimInfo>> AllFbxClipsInfo = new Dictionary<string, List<FbxAnimInfo>>();
    public static List<AnimationClip> GetFbxAnimationInfo(string path)
    {
        
        string folder = Path.GetDirectoryName(path);
        string fbxname = Path.GetFileNameWithoutExtension(path);
        string metaPath = path + ".meta";
        string[] content = File.ReadAllLines(metaPath);
        List<AnimationClip> tclips = new List<AnimationClip>();
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
        List<FbxAnimInfo> fainfos = new List<FbxAnimInfo>();
        string fbxguid= AssetDatabase.AssetPathToGUID(path);
        foreach (var o in objects)
        {
            if (o is AnimationClip)
            {
                tclips.Add((AnimationClip)o);
                if (o.name.Contains("__preview__Take")) continue; //我也不知道这是啥，每个fbx中都包含一个这个clip文件
                AnimationClip clip = new AnimationClip();
                EditorUtility.CopySerialized((AnimationClip)o, clip);
                string savepath = folder + "\\" + fbxname + o.name + ".anim";
                AssetDatabase.CreateAsset(clip, savepath);
                EditorUtility.SetDirty(clip);
                tclips.Add(clip);
                for (int i = 0; i < content.Length; i++)
                {
                    if (content[i].EndsWith(o.name) && !content[i].Contains(o.name + "_"))
                    {
                        string[] value = content[i].Split(':');
                        if (value.Length < 1) continue;
                        value[0] = value[0].Replace(" ", "");
                        int fildid;
                        bool state = int.TryParse(value[0], out fildid);
                        if (!state) continue;   //不是fileid
                        FbxAnimInfo info = new FbxAnimInfo();
                        info.fbxguid = fbxguid;
                        info.name = o.name;
                        info.fileid = value[0];
                        info.newguid = AssetDatabase.AssetPathToGUID(savepath);
                        if(!fainfos.Contains(info))
                        {
                            fainfos.Add(info);
                        }
                    } 
                }   
            }
        }
        if(AllFbxClipsInfo.ContainsKey(fbxguid))
        {
            Debug.Log("出现重复Guid的fbx文件了"+path);
        }
        else
        {
            AllFbxClipsInfo.Add(fbxguid, fainfos);
        }
        Debug.Log("获取数据完毕");
        return tclips;
    }

    static string strMotion = "fileID: {0}, guid: {1}, type: {2}";
    /// <summary>
    /// 修改Animator中的引用
    /// </summary>
    
    static List<AnimationClip> GetAllAnimationClips()
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        var selectedPath = SelectedPath();
        AllFbxClipsInfo = new Dictionary<string, List<FbxAnimInfo>>();
        float maxCount = selectedPath.Count;
        for (int i = 0; i < selectedPath.Count; i++)
        {
            EditorUtility.DisplayProgressBar("获取所有动画信息中", $"{i}/{maxCount}", (float)i / maxCount);
            if (selectedPath[i].ToLower().EndsWith(".fbx"))
            {
                clips.AddRange(GetFbxAnimationInfo(selectedPath[i]));
            }
            else if(selectedPath[i].EndsWith(".anim"))
            {
                var clip = AssetDatabase.LoadAssetAtPath(selectedPath[i], typeof(AnimationClip)) as AnimationClip;
                clips.Add(clip);
            }
        }
        EditorUtility.ClearProgressBar();
        return clips;
    }
    public static void CompressAnim(AnimationClip clip, string keyName)
    {
        ReduceScaleKey(clip, keyName);
        //ReduceFloatPrecision(clip);
    }
    private static void ReduceFloatPrecision(AnimationClip clip)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        for (int j = 0; j < bindings.Length; j++)
        {
            EditorCurveBinding curveBinding = bindings[j];
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

            if (curve == null || curve.keys == null)
            {
                continue;
            }

            Keyframe[] keys = curve.keys;
            for (int k = 0; k < keys.Length; k++)
            {
                Keyframe key = keys[k];
                key.value = float.Parse(key.value.ToString("f3"));
                key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                keys[k] = key;
            }
            curve.keys = keys;

            AnimationUtility.SetEditorCurve(clip, curveBinding, curve);
        }
    }
    /// <summary>
    /// keynams:属性  path:骨骼?地址 (全部需要小写)
    /// </summary>
    private static void ReduceKeys(AnimationClip clip, string[] keynames ,string path = null)
    {
        if (keynames.Length < 1) return;
        EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);

        for (int j = 0; j < curves.Length; j++)
        {
            EditorCurveBinding curveBinding = curves[j];
            if (path!=null && !curveBinding.path.ToLower().Contains(path)) continue;
            if (keynames.Length == 1)
            {
                if (curveBinding.propertyName.ToLower().Contains(keynames[0]))
                {
                    AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                }
            }
            else
            {
                for (int i = 0; i < keynames.Length; i++)
                {
                    if (curveBinding.propertyName.ToLower().Contains(keynames[i]))
                    {
                        AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                    }
                }
            }
        }
    }

    private static void ReduceScaleKey(AnimationClip clip, string keyName)
    {
        EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);

        for (int j = 0; j < curves.Length; j++)
        {
            EditorCurveBinding curveBinding = curves[j];

            if (curveBinding.propertyName.ToLower().Contains(keyName))
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
            }
            else if (curveBinding.propertyName.ToLower().Contains("editorcurves"))
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
            }
        }
    }

    static void ReplaceAnimator()
    {
        var controllerPath = AssetDatabase.FindAssets("t:AnimatorController");
        var overcontrollerPath = AssetDatabase.FindAssets("t:AnimatorOverrideController");
        for (int i = 0; i < controllerPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(controllerPath[i]);
            string[] content = File.ReadAllLines(path);
            for (int k = 0; k < content.Length; k++)
            {
                if (content[k].Contains("m_Motion") && !content[k].Contains("m_Motions"))
                {
                    try
                    {
                        string[] motion = content[k].Split('{');
                        string[] kvdata = motion[1].Replace("{", "").Replace("}", "").Split(',');
                        string guid = kvdata[1].Split(':')[1].Replace(" ", "");
                        if (AllFbxClipsInfo.ContainsKey(guid))
                        {
                            string fileid = kvdata[0].Split(':')[1].Replace(" ", "");
                            foreach (var temp in AllFbxClipsInfo[guid])
                            {
                                if (temp.fileid == fileid)
                                {
                                    content[k] = content[k].Replace(fileid, "7400000").Replace(temp.fbxguid, temp.newguid).Replace("type: 3", "type: 2");
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            File.WriteAllLines(path, content);
        }
        for (int i = 0; i < overcontrollerPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(overcontrollerPath[i]);
            string[] content = File.ReadAllLines(path);
            for (int k = 0; k < content.Length; k++)
            {
                if (content[k].Contains("m_OriginalClip") || content[k].Contains("m_OverrideClip"))
                {
                    try
                    {
                        string motion = content[k].Split('{')[1].Split('}')[0];
                        string[] kvdata = motion.Split(',');
                        string guid = kvdata[1].Split(':')[1].Replace(" ", "");
                        if (AllFbxClipsInfo.ContainsKey(guid))
                        {
                            string fileid = kvdata[0].Split(':')[1].Replace(" ", "");
                            foreach (var temp in AllFbxClipsInfo[guid])
                            {
                                if (temp.fileid == fileid)
                                {
                                    content[k] = content[k].Replace(fileid, "7400000").Replace(temp.fbxguid, temp.newguid).Replace("type: 3", "type: 2");
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            File.WriteAllLines(path, content);
        }
    }

#endregion
}

public class FbxAnimInfo
{
    public string fbxguid;
    public string newguid;
    public string fileid;
    public string name;
}