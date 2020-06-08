using UnityEngine;
using System.Collections;
using System.Text;

public class PathTool
{
    public static string GetPath()
    {
        StringBuilder path = new StringBuilder();

#if UNITY_ANDROID && !UNITY_EDITOR
                //path.Append("file://");
                path.Append(Application.dataPath );
                path.Append("!assets/");
                path.Append("android/");
#else
        path.Append(Application.streamingAssetsPath);
                path.Append("/");
                path.Append("window/");
#endif

        return path.ToString();
    }
    /// <summary>
    /// 更新资源存放在Application.persistentDataPath+"/Resources/"目录下
    /// </summary>
    /// <returns></returns>
    public static string GetAssetsBundlePersistentPath()
    {
        return Application.persistentDataPath + "/Resources/";
    }

    /// <summary>
    /// 组合绝对路径
    /// </summary>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="relativelyPath">相对路径</param>
    /// <returns>绝对路径</returns>
    public static string GetAbsolutePath(string relativelyPath)
    {
        return GetPath() + relativelyPath;
    }

#if UNITY_WEBGL
    /// <summary>
    /// 获取加载URL
    /// </summary>
    /// <param name="relativelyPath">相对路径</param>
    /// <returns></returns>
    public static string GetLoadURL(string relativelyPath)
    {
#if UNITY_EDITOR
        return "file://" + Application.streamingAssetsPath + "/" + relativelyPath;
#else
        return Application.absoluteURL + "StreamingAssets/" + relativelyPath;
#endif
    }
#endif

    //获取相对路径
    public static string GetRelativelyPath(string path, string fileName, string expandName)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(path);
        builder.Append("/");
        builder.Append(fileName);
        builder.Append(".");
        builder.Append(expandName);

        return builder.ToString();
    }

    /// <summary>
    /// 获取某个目录下的相对路径
    /// </summary>
    /// <param name="FullPath">完整路径</param>
    /// <param name="DirectoryPath">目标目录</param>
    public static string GetDirectoryRelativePath(string DirectoryPath,string FullPath)
    {
       DirectoryPath = DirectoryPath.Replace(@"\", "/");
       FullPath = FullPath.Replace(@"\", "/");

       FullPath = FullPath.Replace(DirectoryPath, "");

        return FullPath;
    }

    public static string GetWWWPath()
    {
        string strReturnWWWPath = string.Empty;

        switch(Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                strReturnWWWPath = "file://" + GetPath();
                //strReturnWWWPath = GetPath();
                break;
            case RuntimePlatform.Android:
                strReturnWWWPath = "jar:file://" + GetPath();
                break;
            default:
                break;
        }

        return strReturnWWWPath;
    }


    #if UNITY_EDITOR

    /// <summary>
    /// 获取编辑器下的路径
    /// </summary>
    /// <param name="directoryName">目录名</param>
    /// <param name="fileName">文件名</param>
    /// <param name="expandName">拓展名</param>
    /// <returns></returns>
    public static string GetEditorPath(string directoryName, string fileName, string expandName)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(Application.dataPath);
        builder.Append("/Editor");
        builder.Append(directoryName);
        builder.Append("/");
        builder.Append(fileName);
        builder.Append(".");
        builder.Append(expandName);

        return builder.ToString();
    }

    #endif
}
