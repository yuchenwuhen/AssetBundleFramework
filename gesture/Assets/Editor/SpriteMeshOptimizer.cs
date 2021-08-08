using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.U2D;
using UnityEditor.Sprites;
using UnityEngine;

namespace SpriteMesher
{

    public class SpriteMeshOptimizer : AssetPostprocessor
    {

        //Set this bool with false if we want to stop the script from working
        bool isWorking = true;
        private const byte AlphaTolerance = 128;
        private const bool HoleDetection = true;
        private const float OutlineTolerance = 0.05f;

        //static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        //{
        //    foreach (string str in importedAssets)
        //    {
        //        SpritePhysicsShapeAdjuster.AdjustPhysicsShapes(str);
        //    }
        //}


        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {

            if (isWorking)
            {

                if (!assetPath.Contains("Test")) return;

                var importer = assetImporter as TextureImporter;
                if (importer == null) return;

                if (importer.textureType != TextureImporterType.Sprite && importer.textureType != TextureImporterType.Default) return;

                if (importer.textureType == TextureImporterType.Default && importer.spriteImportMode == SpriteImportMode.None)
                    return;

                importer.textureType = TextureImporterType.Sprite;

                var generateOutline = typeof(SpriteUtility).GetMethod(
            "GenerateOutline",
            BindingFlags.NonPublic | BindingFlags.Static);
                if (generateOutline == null)
                {
                    Debug.LogError("GenerateOutline not found.");
                    return;
                }


                foreach (Sprite spriteItem in sprites)
                {
                    // Get SpriteEditorDataProvider ... Stop processing if failed or destroyed
                    var spriteEditorDataProvider = importer as ISpriteEditorDataProvider;
                    if ((spriteEditorDataProvider == null) || spriteEditorDataProvider.Equals(null))
                    {
                        Debug.LogError("SpriteEditorDataProvider not found.");
                        continue;
                    }
                    spriteEditorDataProvider.InitSpriteEditorDataProvider();

                    // Get TextureDataProvider ... If it fails, stop processing
                    var textureDataProvider = spriteEditorDataProvider.GetDataProvider<ITextureDataProvider>();
                    if (textureDataProvider == null)
                    {
                        Debug.LogError("TextureDataProvider not found.");
                        continue;
                    }

                    // Get texture for sprite ... Stop processing if failed
                    //var texture = textureDataProvider.GetReadableTexture2D();
                    if (texture == null)
                    {
                        Debug.LogError("Texture not found.");
                        continue;
                    }

                    // Get SpritePhysicsOutlineDataProvider ... Stop processing if failed
                    var outlineDataProvider = spriteEditorDataProvider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
                    if (outlineDataProvider == null)
                    {
                        Debug.LogError("OutlineDataProvider not found.");
                        continue;
                    }

                    // Get the actual texture size, calculate the scale
                    // Scale seems to be needed to accommodate textures with maximum size restrictions and NPOT textures ...?
                    int actualWidth, actualHeight;
                    textureDataProvider.GetTextureActualWidthAndHeight(out actualWidth, out actualHeight);
                    var textureWidth = texture.width;
                    var textureHeight = texture.height;
                    var rectScaleX = (float)textureWidth / actualWidth;
                    var rectScaleY = (float)textureHeight / actualHeight;
                    Debug.LogFormat("Rect scale: ({0}, {1})", rectScaleX, rectScaleY);

                    // 计算轮廓
                    var spriteCount = sprites.Length;
                    for (var i = 0; i < spriteCount; i++)
                    {
                        var sprite = sprites[i];
                        if (sprite == null)
                        {
                            Debug.LogWarningFormat("Sprite {0} not found.", i);
                            continue;
                        }
                        Debug.LogFormat("Processing sprite {0}...", sprite.name);
                        var rect = sprite.rect;
                        var halfExtents = rect.size * 0.5f;
                        rect.xMax *= rectScaleX;
                        rect.yMax *= rectScaleY;
                        rect.xMin *= rectScaleX;
                        rect.yMin *= rectScaleY;
                        var args = new object[] { texture, rect, OutlineTolerance, AlphaTolerance, HoleDetection, null };
                        generateOutline.Invoke(null, args);
                        var paths = args[5] as Vector2[][];
                        if (paths == null)
                        {
                            Debug.LogWarning("Paths not found.");
                            continue;
                        }
                        var pathCount = paths.Length;
                        Debug.LogFormat("{0} path(s) found.", pathCount);
                        for (var j = 0; j < pathCount; j++)
                        {
                            var path = paths[j];
                            if (path == null)
                            {
                                continue;
                            }
                            var vertexCount = path.Length;
                            Debug.LogFormat("Path {0} has {1} vertices.", j, vertexCount);
                            for (var k = 0; k < vertexCount; k++)
                            {
                                var p = path[k];
                                path[k].x = Mathf.Clamp(p.x / rectScaleX, -halfExtents.x, halfExtents.x);
                                path[k].y = Mathf.Clamp(p.y / rectScaleY, -halfExtents.y, halfExtents.y);
                            }
                        }
                        var spriteId = sprite.GetSpriteID();
                        outlineDataProvider.SetOutlines(spriteId, paths.ToList());
                        outlineDataProvider.SetTessellationDetail(spriteId, OutlineTolerance);
                        Debug.LogFormat("Processed {0} of {1} sprite(s).", i + 1, spriteCount);
                    }

                    // 应用编辑
                    spriteEditorDataProvider.Apply();

                    // 保存/重新导入
                    Debug.LogFormat("Reimport {0}.", assetPath);
                    //importer.SaveAndReimport();

                    //Vector2[] spritesVertices = new Vector2[spriteItem.vertices.Length];



                    ////we need to transform from sprite space and compensite for pixels per unit
                    //for (int i = 0; i < spriteItem.vertices.Length; i++)
                    //{

                    //    spritesVertices[i] = new Vector2((spriteItem.vertices[i].x * spriteItem.pixelsPerUnit) + spriteItem.rect.size.x / 2,
                    //        (spriteItem.vertices[i].y * spriteItem.pixelsPerUnit) + spriteItem.rect.size.y / 2);
                    //}

                    //Mesh mesh = new Mesh();

                    //mesh.vertices = spritesVertices.toVector3Array();
                    //mesh.triangles = spriteItem.triangles.toIntArray();

                    ////Generate Mesh
                    //AutoWeld(mesh, 2000f, 2000f);

                    //List<Vector2> item = new List<Vector2>();
                    //for(int i = 0;i<spriteItem.texture.width;i++)
                    //{
                    //    for(int j =0;j<spriteItem.texture.height;j++)
                    //    {
                    //        var color = spriteItem.texture.GetPixel(i, j);
                    //        if (color.a > 0.5f)
                    //        {
                    //            item.Add(new Vector2( - spriteItem.texture.width/2 + i/100f, - spriteItem.texture.height/2  + j/100f));
                    //        }
                    //    }
                    //}
                    //var veticles = spriteItem.vertices;
                    //for (int i = 0; i < mesh.vertices.Length; i++)
                    //{
                    //    item.Add(new Vector2(mesh.vertices[i].x, mesh.vertices[i].y));
                    //}
                    //foreach (var item1 in mesh.vertices)
                    //{
                    //    item.Add((Vector2)item1);
                    //}

                    //                IList<Vector2> hull = ConvexHull.ComputeConvexHull(item, true);

                    //                Vector2[] hullItem = new Vector2[hull.Count];
                    //                int index = 0;
                    //                foreach (var hull1 in hull)
                    //                {
                    //                    hullItem[index] = new Vector2(hull1.x, hull1.y);
                    //                    index++;
                    //                }

                    //                spriteItem.OverridePhysicsShape(new List<Vector2[]> {
                    //   hullItem,
                    //});

                    //                Debug.Log(spriteItem.name + " : Sprite mesh optimized");

                    //Vector2[] verticesVector2D = mesh.vertices.toVector2Array();

                    ////Check and make sure no vertices is out of sprite bounds
                    //for (int i = 0; i < verticesVector2D.Length; i++)
                    //{
                    //    if (verticesVector2D[i].x < 0)
                    //    {
                    //        verticesVector2D[i] = new Vector2(0, verticesVector2D[i].y);
                    //    }

                    //    else if (verticesVector2D[i].x > spriteItem.rect.size.x)
                    //    {
                    //        verticesVector2D[i] = new Vector2(spriteItem.rect.size.x, verticesVector2D[i].y);
                    //    }



                    //    if (verticesVector2D[i].y < 0)
                    //    {
                    //        verticesVector2D[i] = new Vector2(verticesVector2D[i].x, 0);
                    //    }
                    //    else if (verticesVector2D[i].y > spriteItem.rect.size.y)
                    //    {
                    //        verticesVector2D[i] = new Vector2(verticesVector2D[i].x, spriteItem.rect.size.y);
                    //    }

                    //}


                    //Add the new mesh to the sprite
                    //spriteItem.OverrideGeometry(verticesVector2D, mesh.triangles.toUshoartArray());

                }
            }
        }


        /*********************************************************************
         * The code below by awesome guy from  this source
         * http://answers.unity3d.com/questions/228841/dynamically-combine-verticies-that-share-the-same.html
         * *******************************************************************/
        public static void AutoWeld(Mesh mesh, float threshold, float bucketStep)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            int[] old2new = new int[oldVertices.Length];
            int newSize = 0;

            // Find AABB
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++)
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
                int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
                int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

                // Check to see if it's already been added
                if (buckets[x, y, z] == null)
                    buckets[x, y, z] = new List<int>(); // Make buckets lazily

                for (int j = 0; j < buckets[x, y, z].Count; j++)
                {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude(to) < threshold)
                    {
                        old2new[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[newSize] = oldVertices[i];
                buckets[x, y, z].Add(newSize);
                old2new[i] = newSize;
                newSize++;

                skip:;
            }

            // Make new triangles
            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length];
            for (int i = 0; i < oldTris.Length; i++)
            {
                newTris[i] = old2new[oldTris[i]];
            }

            Vector3[] finalVertices = new Vector3[newSize];
            for (int i = 0; i < newSize; i++)
                finalVertices[i] = newVertices[i];

            mesh.Clear();
            mesh.vertices = finalVertices;
            mesh.triangles = newTris;
            mesh.RecalculateNormals();
            ;
        }
    }


    public static class HelperArrayExtension
    {
        public static Vector3[] toVector3Array(this Vector2[] v2)
        {
            return System.Array.ConvertAll<Vector2, Vector3>(v2, getV3fromV2);
        }

        public static Vector3 getV3fromV2(Vector2 v3)
        {
            return new Vector3(v3.x, v3.y, 0);
        }


        public static int[] toIntArray(this ushort[] ush)
        {
            return System.Array.ConvertAll<ushort, int>(ush, getInt);
        }

        public static int getInt(ushort u)
        {
            return System.Convert.ToInt32(u);
        }


        public static ushort[] toUshoartArray(this int[] inA)
        {
            return System.Array.ConvertAll<int, ushort>(inA, getushort);
        }

        public static ushort getushort(int u)
        {
            return (ushort)u;
        }


        public static Vector2[] toVector2Array(this Vector3[] v3)
        {
            return System.Array.ConvertAll<Vector3, Vector2>(v3, getV3fromV2);
        }

        public static Vector2 getV3fromV2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.y);
        }
    }


}