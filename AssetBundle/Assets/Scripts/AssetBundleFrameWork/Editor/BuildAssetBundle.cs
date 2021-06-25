/***
 *
 *   Title: "AssetBundle简单框架"项目
 *
 *   Description:
 *          功能： 对标记的资源进行打包输出
 *
 *   Author: Liuguozhu
 *
 *   Date: 2017.10
 *
 *   Modify：  
 *
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;   //引入Unity编辑器，命名空间
using System.IO;     //引入的C#IO,命名空间

namespace ABFW
{
    public class BuildAssetBundle {

        /// <summary>
        /// 打包生成所有的AssetBundles(包)
        /// </summary>
        [MenuItem("Tools/AssetBundle/BuildAllAssetBundles")]
        public static void BuildAllAB()
        {
            //打包AB输出路径
            string strABOutPathDIR = string.Empty;

            //获取"StreamingAssets"数值
            strABOutPathDIR = PathTool.GetWWWPath();

        //判断生成输出目录文件夹
            if (!Directory.Exists(strABOutPathDIR))
            {
                Directory.CreateDirectory(strABOutPathDIR);
            }
            //打包生成
            BuildPipeline.BuildAssetBundles(strABOutPathDIR, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }

    }//Class_end
}
