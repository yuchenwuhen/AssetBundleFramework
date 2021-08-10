using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int width;
    public int height;

    // Start is called before the first frame update
    void Start()
    {
        TextAsset text = Resources.Load<TextAsset>("组 2_0"); ;

        JsonData sceneData = JsonMapper.ToObject(text.text);

        var objName = (string)(sceneData["name"]);
        JsonData rect = JsonMapper.ToObject((string)(sceneData["rect"]));
        width = (int)(rect["width"]);
        height = (int)(rect["height"]);
        GameObject obj = new GameObject("objName");
        obj.transform.position = Vector3.zero; 
        if (sceneData["children"] != null)
        {
            JsonData children = JsonMapper.ToObject((string)(sceneData["children"]));
            for (int i = 0; i < children.Count; i++)
            {
                LoadSceneData(children[i], obj.transform);
            }
        }
    }

    void LoadSceneData(JsonData childrenData,Transform parent)
    {
        var objName = (string)(childrenData["name"]);
        var objType = (string)(childrenData["type"]);
        GameObject obj = new GameObject("objName");
        if (objType == "Image")
        {
            //获取Sprite位置
            var sprite = Resources.Load<Sprite>("组 2_0/" + objName);
            obj.AddComponent<SpriteRenderer>().sprite = sprite;
        }

        JsonData rect = JsonMapper.ToObject((string)(sceneData["rect"]));
        width = (int)(rect["width"]);
        height = (int)(rect["height"]); 

        obj.transform.SetParent(parent);
        obj.transform.position = 
    }
}
