using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AB资源加载类
/// 功能：
///     1、管理与加载指定AB的资源
///     2、加载具有“缓存功能”的资源，带选用参数
///     3、卸载、释放AB资源
///     4、查看当前AB资源
/// </summary>
public class AssetLoader : System.IDisposable
{
    private AssetBundle m_currentAssetBundle;
    //缓存容器集合
    private Hashtable m_ht;

    public AssetLoader(AssetBundle abObj)
    {
        if (abObj != null)
        {
            m_currentAssetBundle = abObj;
            m_ht = new Hashtable();
        }
        else
        {
            Debug.Log(GetType() + "/ 构造函数AssetBundle()/参数 abObj == null! 请检查!");
        }
    }

    /// <summary>
    /// 加载当前包中指定的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="isCache"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadAsset(string assetName, bool isCache = false)
    {
        return LoadResource<UnityEngine.Object>(assetName,isCache);
    }

    /// <summary>
    /// 加载当前AB包的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetName"></param>
    /// <param name="isCache"></param>
    /// <returns></returns>
    private T LoadResource<T>(string assetName, bool isCache) where T : UnityEngine.Object
    {
        if (m_ht.Contains(assetName))
        {
            return m_ht[assetName] as T;
        }

        T asset = m_currentAssetBundle.LoadAsset<T>(assetName);
        if (asset!=null && isCache)
        {
            m_ht.Add(assetName, asset);
        }
        else if (asset == null)
        {
            Debug.Log(GetType() + "/LoadResource<T>()/参数 asset == null,请检查");
        }

        return asset;
    }

    /// <summary>
    /// 卸载指定资源
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public bool UnLoadAsset(UnityEngine.Object asset)
    {
        if (asset != null)
        {
            Resources.UnloadAsset(asset);
            return true;
        }
        Debug.LogError(GetType() + "/UnLoadAsset()/参数asset==null,请检查！");
        return false;
    }

    /// <summary>
    /// 释放当前AssetBundle内存镜像资源
    /// </summary>
    public void Dispose()
    {
        m_currentAssetBundle.Unload(false);
    }

    /// <summary>
    /// 释放当前AssetBundle内存镜像资源且释放内存资源
    /// </summary>
    public void DisposeAll()
    {
        m_currentAssetBundle.Unload(true);
    }

    /// <summary>
    /// 查询当前AssetBundle中包含的所有资源名称
    /// </summary>
    /// <returns></returns>
    public string[] RetriveAllAssetName()
    {
        return m_currentAssetBundle.GetAllAssetNames();
    }
}
