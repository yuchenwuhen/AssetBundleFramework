/***
 * 
 *  Title:自动给资源文件添加标记
 *  
 *  Description:
 *      开发思路：
 *          1： 定义需要打包资源的文件夹目录
 *          2：遍历每个“场景”文件夹（目录）
 *              2.1：遍历本场景目录下所有的目录或者文件
 *                  如果是目录，则继续“递归”访问里面的文件，直到定位到文件
 *              2.2：找到文件，则使用AssetImporter类，标记包名与后缀名
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;

public class AutoSetLabels
{
    [MenuItem("Tools/AssetBundle/Set AB Label")]
    public static void SetABLabel()
    {
        //需要给AB做标记的根目录
        string strNeedSetLabelBoot = string.Empty;
        //目录信息（场景目录信息数组，表示所有根目录下场景目录）
        DirectoryInfo[] dirScenesDIRArray = null;

        //清空无用AR标记
        AssetDatabase.RemoveUnusedAssetBundleNames();

        //定义需要打包资源的文件夹目录
        strNeedSetLabelBoot = Application.dataPath + "/AB_Res";
        DirectoryInfo dirTempInfo = new DirectoryInfo(strNeedSetLabelBoot);
        dirScenesDIRArray = dirTempInfo.GetDirectories();

        //遍历每个“场景”文件夹（目录）
        foreach (DirectoryInfo curDir in dirScenesDIRArray)
        {
            //2.1：遍历本场景目录下所有的目录或者文件
            //     如果是目录，则继续“递归”访问里面的文件，直到定位到文件
            string tempScenesDir = strNeedSetLabelBoot + "/" + curDir.Name;
            DirectoryInfo tempScenesDirInfo = new DirectoryInfo(tempScenesDir);
            int tempIndex = tempScenesDir.LastIndexOf("/");
            string tempScenesName = tempScenesDir.Substring(tempIndex + 1);

            //2.2：找到文件，则使用AssetImporter类，标记包名与后缀名
            JudgeDirFileByRecursive(curDir, tempScenesName);
        }


        //刷新
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 递归判断是否为目录与文件，修改AssetBundle 的标记（label）
    /// </summary>
    /// <param name="fileSystemInfo">当前目录信息(DirectoryInfo和FileSystemInfo可以相互转换)</param>
    /// <param name="tempScenesName">当前场景名字</param>
    private static void JudgeDirFileByRecursive(FileSystemInfo fileSystemInfo, string tempScenesName)
    {
        if (!fileSystemInfo.Exists)
        {
            Debug.LogError("文件或目录名称：" + fileSystemInfo + "不存在，请检查");
            return;
        }

        //得到当前目录下一级的文件信息集合
        DirectoryInfo dirInfo = fileSystemInfo as DirectoryInfo;
        FileSystemInfo[] fileSystemArray = dirInfo.GetFileSystemInfos();
        foreach (FileSystemInfo fileInfo in fileSystemArray)
        {
            FileInfo fileinfoObj = fileInfo as FileInfo;
            //文件类型
            if (fileinfoObj != null)
            {
                //修改此文件的AssetBundle标签

            }
            //目录类型
            else
            {
                //如果是目录
                JudgeDirFileByRecursive(fileInfo, tempScenesName);
            }
        }
    }

    /// <summary>
    /// 设置文件信息
    /// </summary>
    /// <param name="fileinfoObj"></param>
    /// <param name="sceneName"></param>
    private static void SetFileABLabel(FileInfo fileinfoObj,string sceneName)
    {
        string strABName = string.Empty;

        string strAssetFilePath = string.Empty;

        //参数检查
        if (fileinfoObj.Extension == ".meta")
        {
            return;
        }

        //得到AB包名称
        strABName = GetABName(fileinfoObj,sceneName);
        //获取资源文件的相对路径
        int tempIndex = fileinfoObj.FullName.IndexOf("Assets");
        strAssetFilePath = fileinfoObj.FullName.Substring(tempIndex);

        AssetImporter tempImporterObj = AssetImporter.GetAtPath(strAssetFilePath);
        tempImporterObj.assetBundleName = strABName;
        if (fileinfoObj.Extension == ".unity")
        {
            //扩展名
            tempImporterObj.assetBundleVariant = "u3d";
        }
        else
        {
            tempImporterObj.assetBundleVariant = "ab";
        }
    }

    private static string GetABName(FileInfo fileinfoObj, string sceneName)
    {
        string strABName = string.Empty;

        
    }
}
