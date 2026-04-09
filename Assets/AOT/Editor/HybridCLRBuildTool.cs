using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 该命名空间必须放在名为Editor的文件夹下，否则会报错
using HybridCLR.Editor;
using System.IO;

public class HybridCLRBuildTool
{
    [MenuItem("BuildTool/HybridCLRCopyDLLs")]
    public static void HybridCLRBuild()
    {
        // 1.获取需要热更新程序集的信息
        List<string> assemblys = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
        foreach (string assembly in assemblys)
        {
            Debug.Log(assembly);
        }

        // 2.获取打包后的程序集所在路径
        string path = ($"{ Application.dataPath }/../HybridCLRData/HotUpdateDlls/{EditorUserBuildSettings.activeBuildTarget.ToString() }");
        //Debug.Log(path);

        // 3.获取指定路径文件夹下的所有文件，并于需要热更新程序集的信息进行对比
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles();

        string targetPath = ($"{Application.dataPath}/AddressableResources/HotUpdateDLLs/");
        Debug.Log("代码生成的路径: " + targetPath);
        Debug.Log("目录是否存在: " + Directory.Exists(targetPath));
        //Directory.CreateDirectory(targetPath);

        foreach (FileInfo file in files)
        {
            //Debug.Log(file.Name);
            //Debug.Log(file.Extension);
            // 4.复制需要热更新的程序集到指定路径文件夹下
            if (file.Extension ==".dll" && assemblys.Contains(file.Name.Substring(0, file.Name.Length - 4)))
            {
                string fileName = Path.Combine(targetPath, file.Name + ".bytes");
                file.CopyTo(fileName, true);
            }
        }

        // 5.保存
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
