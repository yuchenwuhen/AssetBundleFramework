using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSingleAssetLoader : MonoBehaviour
{
    private SingleABLoader m_singleABLoader = null;

    //AB包名称
    private string m_abName = "scene_1/prefabs.ab";
    //AB包中资源名称
    private string m_assetName = "Cube.prefab";

    private string m_dependAssetName2 = "scene_1/materials.ab";
    private string m_dependAssetName1 = "scene_1/textures.ab";

    private string m_dependName1 = "scene_1/textures.ab";
    private string m_dependName2 = "scene_1/materials.ab";

    // Start is called before the first frame update
    void Start()
    {
        m_singleABLoader = new SingleABLoader(m_dependAssetName1, DependCompelete1);
        StartCoroutine(m_singleABLoader.LoadAssetBundle());
    }

    private void DependCompelete1(string abName)
    {
        m_singleABLoader = new SingleABLoader(m_dependAssetName2, DependCompelete2);
        StartCoroutine(m_singleABLoader.LoadAssetBundle());
    }

    private void DependCompelete2(string abName)
    {
        m_singleABLoader = new SingleABLoader(m_abName, LoadComplete);
        StartCoroutine(m_singleABLoader.LoadAssetBundle());
    }

    private void LoadComplete(string abName)
    {
        UnityEngine.Object tempObj = m_singleABLoader.LoadAsset(m_assetName, false);

        Instantiate(tempObj);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
