using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// UnityWebRequest加载AssetBundle
/// </summary>
public class SingleABLoader : System.IDisposable
{
    private AssetLoader m_assetLoader;

    private LoadCompeleteHandle m_loadCompeleteHandle;

    //ab包名称
    private string m_abName;
    //ab下载路径
    private string m_abDownLoadPath;

    public SingleABLoader(string abName,LoadCompeleteHandle loadCompelete)
    {
        m_abName = abName;
        m_abDownLoadPath = PathTool.GetWWWPath() + abName;
        //m_abDownLoadPath = m_abDownLoadPath.Replace("/", "\\");
        m_loadCompeleteHandle = loadCompelete;
    }

    public IEnumerator LoadAssetBundle()
    {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(m_abDownLoadPath))
        {
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                // 下载出错
                Debug.Log(GetType() + "/LoadAssetBundle/UnityWebRequest 下载出错，请检查" + m_abDownLoadPath + "错误原因" + request.error);
            }
            else
            {
                // 下载完成
                AssetBundle bundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                if (bundle != null)
                {
                    m_assetLoader = new AssetLoader(bundle);

                    //AssetBundle下载完毕，调用委托
                    if (m_loadCompeleteHandle != null)
                    {
                        m_loadCompeleteHandle(m_abName);
                    }
                }
                else
                {
                    Debug.Log(GetType() + "/LoadAssetBundle/UnityWebRequest 下载出错，请检查" + m_abDownLoadPath + "错误原因" + request.error);
                }
                // 优先释放request 会降低内存峰值
                request.Dispose();
            }
        }
    }

    /// <summary>
    /// 加载AB包资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="isCache"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadAsset(string assetName, bool isCache)
    {
        if (m_assetLoader != null)
        {
            return m_assetLoader.LoadAsset(assetName, isCache);
        }
        Debug.LogError(GetType() + "/LoadAsset()/参数 m_assetLoader==null,请检查！");
        return null;
    }

    /// <summary>
    /// 卸载AB包资源
    /// </summary>
    /// <param name="asset"></param>
    public void UnLoadAsset(UnityEngine.Object asset)
    {
        if (m_assetLoader != null)
        {
            m_assetLoader.UnLoadAsset(asset);
        }
        else
        {
            Debug.LogError(GetType() + "/UnLoadAsset()/参数 m_assetLoader==null,请检查！");
        }
    }

   /// <summary>
   /// 释放资源
   /// </summary>
    public void Dispose()
    {
        if (m_assetLoader != null)
        {
            m_assetLoader.Dispose();
            m_assetLoader = null;
        }
        else
        {
            Debug.LogError(GetType() + "/Dispose()/参数 m_assetLoader==null,请检查！");
        }
    }

    /// <summary>
    /// 释放当前AssetBundle资源包，且卸载所有资源
    /// </summary>
    public void DisposeAll()
    {
        if (m_assetLoader != null)
        {
            m_assetLoader.DisposeAll();
            m_assetLoader = null;
        }
        else
        {
            Debug.LogError(GetType() + "/DisposeAll()/参数 m_assetLoader==null,请检查！");
        }
    }

    /// <summary>
    /// 检查当前AssetBundle所有资源
    /// </summary>
    /// <returns></returns>
    public string[] RetriveAllAssetName()
    {
        if (m_assetLoader != null)
        {
            return m_assetLoader.RetriveAllAssetName();
        }
        Debug.LogError(GetType() + "/RetriveAllAssetName()/参数 m_assetLoader==null,请检查！");
        return null;
    }
}
