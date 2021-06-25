using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 读取AssetBundle依赖关系
/// </summary>
public class ABManifestLoad : Singleton<ABManifestLoad>,System.IDisposable
{
    /// <summary>
    /// AssetBundle清单文件
    /// </summary>
    private AssetBundleManifest m_manifestObj;

    /// <summary>
    /// 清单文件路径
    /// </summary>
    private string m_strManifestPath;

    /// <summary>
    /// 读取AssetBundle
    /// </summary>
    private AssetBundle m_abReadManifest;

    private bool m_isLoadFinish;
    private bool IsLoadFinish
    {
        get { return m_isLoadFinish; }
    }

    public ABManifestLoad()
    {
        m_strManifestPath = PathTool.GetWWWPath() + "/" + PathTool.GetPlatform();
        m_manifestObj = null;
        m_abReadManifest = null;
        m_isLoadFinish = false;
    }

    public IEnumerator LoadManifestFile()
    {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(m_strManifestPath))
        {
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                // 下载出错
                Debug.Log(GetType() + "/LoadAssetBundle/UnityWebRequest 下载出错，请检查" + m_strManifestPath + "错误原因" + request.error);
            }
            else
            {
                // 下载完成
                AssetBundle bundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                if (bundle != null)
                {
                    m_abReadManifest = bundle;

                    m_manifestObj = m_abReadManifest.LoadAsset(ABDefine.assetBundleManifest) as AssetBundleManifest;

                    m_isLoadFinish = true;
                }
                else
                {
                    Debug.Log(GetType() + "/LoadAssetBundle/UnityWebRequest 下载出错，请检查" + m_strManifestPath + "错误原因" + request.error);
                }
                // 优先释放request 会降低内存峰值
                request.Dispose();
            }
        }
    }

    /// <summary>
    /// 获取清单文件
    /// </summary>
    /// <returns></returns>
    public AssetBundleManifest GetABManifest()
    {
        if (m_isLoadFinish)
        {
            if (m_manifestObj != null)
            {
                return m_manifestObj;
            }
            else
            {
                Debug.Log(GetType() + "/GetABManifest()/m_manifestObj==null ，请检查!");
            }
        }
        else
        {
            Debug.Log(GetType() + "/GetABManifest()/m_isLoadFinish==false ，Manifest没有加载完成,请检查!");
        }

        return null;
    }

    /// <summary>
    /// 查询所有依赖项
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public string[] RetrivalDepends(string abName)
    {
        if (m_manifestObj != null && string.IsNullOrEmpty(abName))
        {
            return m_manifestObj.GetAllDependencies(abName);
        }

        return null;
    }

    /// <summary>
    /// 释放本类资源
    /// </summary>
    public void Dispose()
    {
        if (m_abReadManifest != null)
        {
            m_abReadManifest.Unload(true);
        }
    }
}
