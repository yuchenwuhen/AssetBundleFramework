using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GestureRecognizer;
using UnityEngine.UI;

public class GestureCapturePoints : MonoBehaviour {

	[Tooltip("Disable or enable gesture recognition")]
	public bool isEnabled = true;

	[Tooltip("Overwrite the XML file in persistent data path")]
	public bool forceCopy = false;

	[Tooltip("Use the faster algorithm, however default (slower) algorithm has a better scoring system")]
	public bool UseProtractor = false;

	[Tooltip("The name of the gesture library to load. Do NOT include '.xml'")]
	public string libraryToLoad = "shapes";

	[Tooltip("A new point will be placed if it is this further than the last point.")]
	public float distanceBetweenPoints = 10f;

	[Tooltip("Minimum amount of points required to recognize a gesture.")]
	public int minimumPointsToRecognize = 10;

	[Tooltip("Material for the line renderer.")]
	public Material lineMaterial;

	[Tooltip("Start thickness of the gesture.")]
	public float startThickness = 0.25f;

	[Tooltip("End thickness of the gesture.")]
	public float endThickness = 0.05f;

    [Tooltip("Start color of the gesture.")]
    public Color startColor = new Color(0, 0.67f, 1f);

	[Tooltip("End color of the gesture.")]
	public Color endColor = new Color(0.48f, 0.83f, 1f);

    [Tooltip("The RectTransform that limits the gesture")]
    public RectTransform drawArea;

    [Tooltip("The InputField that will hold the new gesture name")]
    public InputField newGestureName;

    [Tooltip("Messages will show up here")]
    public Text messageArea;

	// Current platform.
	RuntimePlatform platform;

	// Line renderer component.
	LineRenderer gestureRenderer;

	// The position of the point on the screen.
	Vector3 virtualKeyPosition = Vector2.zero;

	// A new point.
	Vector2 point;

	// List of points that form the gesture.
	List<Vector2> points = new List<Vector2>();

	// Vertex count of the line renderer.
	int vertexCount = 0;

	// Loaded gesture library.
	GestureLibrary gl;

	// Recognized gesture.
	Gesture gesture;

	// Result.
	Result result;

    public string spriteName = "1";

	// Get the platform and apply attributes to line renderer.
	void Awake() {
		platform = Application.platform;
		gestureRenderer = gameObject.AddComponent<LineRenderer>();
		gestureRenderer.SetVertexCount(0);
		gestureRenderer.material = lineMaterial;
		gestureRenderer.SetColors(startColor, endColor);
		gestureRenderer.SetWidth(startThickness, endThickness);
	}

    Sprite spriteItem;
	// Load the library.
	void Start() {
		gl = new GestureLibrary(libraryToLoad, forceCopy);
        spriteItem = Resources.Load<Sprite>(spriteName);

    }


    void Update() {

        if (Input.GetKeyDown(KeyCode.Space))
        {

            Vector2[] spritesVertices = new Vector2[spriteItem.vertices.Length];
            for (int i = 0; i < spriteItem.vertices.Length; i++)
            {

                spritesVertices[i] = new Vector2((spriteItem.vertices[i].x * spriteItem.pixelsPerUnit) + spriteItem.rect.size.x / 2,
                    (spriteItem.vertices[i].y * spriteItem.pixelsPerUnit) + spriteItem.rect.size.y / 2);
            }

            Mesh mesh = new Mesh();

            mesh.vertices = spritesVertices.toVector3Array();
            mesh.triangles = spriteItem.triangles.toIntArray();

            //Generate Mesh
            AutoWeld(mesh, 500f, 500f);

            List<Vector2> item = new List<Vector2>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                item.Add(new Vector2(mesh.vertices[i].x, mesh.vertices[i].y));
            }

            IList<Vector2> hull = ConvexHull.ComputeConvexHull(item, true);
            var _points = new List<Vector2>();
            Vector2[] hullItem = new Vector2[hull.Count];
            int index = 0;
            foreach (var hull1 in hull)
            {
                hullItem[index] = new Vector2(hull1.x, hull1.y - Screen.height/2f);
                _points.Add(hullItem[index]);
                index++;
            }

            if (_points.Count > 2)
            {
                gesture = new Gesture(_points);
                result = gesture.Recognize(gl, UseProtractor);
                SetMessage("Gesture is recognized as <color=#ff0000>'" + result.Name + "'</color> with a score of " + result.Score);
            }

            //if (_points.Count > 2)
            //{
            //    var _points2 = new List<Vector2>();
            //    for (int i = _points.Count-1; i >= 0; i--)
            //    {
            //        _points2.Add(_points[i]);
            //    }
            //    gesture = new Gesture(_points2);
            //    result = gesture.Recognize(gl, UseProtractor);
            //    SetMessage("222Gesture is recognized as <color=#ff0000>'" + result.Name + "'</color> with a score of " + result.Score);
            //}
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            List<Vector2> _addPoints = new List<Vector2>();
            //添加正方形
            for (int i = 0; i < 16; i++)
            {
                _addPoints.Add(new Vector2(100 + i * 10, -100f));
            }
            for (int i = 1; i < 16; i++)
            {
                _addPoints.Add(new Vector2(250f, -100f - i*6));
            }
            for (int i = 1; i < 16; i++)
            {
                _addPoints.Add(new Vector2(250f-i*10, -190f));
            }
            for (int i = 1; i < 15; i++)
            {
                _addPoints.Add(new Vector2(100f, -190f+i*6f));
            }

            AddGesture(_addPoints, "fang");

            _addPoints.Clear();

            //添加圆形
            for (int i = 0; i < 360; i+=5)
            {
                _addPoints.Add(new Vector2(100+50 * Mathf.Cos(i * Mathf.Deg2Rad), -100 + 50 * Mathf.Sin(i * Mathf.Deg2Rad)));
            }

            AddGesture(_addPoints, "yuan");
        }

		// Track user input if GestureRecognition is enabled.
		if (isEnabled) {

			// If it is a touch device, get the touch position
			// if it is not, get the mouse position
			if (Utility.IsTouchDevice()) {
				if (Input.touchCount > 0) {
					virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
				}
			} else {
				if (Input.GetMouseButton(0)) {
					virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
				}
			}

			if (RectTransformUtility.RectangleContainsScreenPoint(drawArea, virtualKeyPosition, Camera.main)) {

				if (Input.GetMouseButtonDown(0)) {
					ClearGesture();
				}

				// It is not necessary to track the touch from this point on,
				// because it is already registered, and GetMouseButton event 
				// also fires on touch devices
				if (Input.GetMouseButton(0)) {

					point = new Vector2(virtualKeyPosition.x, -virtualKeyPosition.y);

					// Register this point only if the point list is empty or current point
					// is far enough than the last point. This ensures that the gesture looks
					// good on the screen. Moreover, it is good to not overpopulate the screen
					// with so much points.
					if (points.Count == 0 ||
						(points.Count > 0 && Vector2.Distance(point, points[points.Count - 1]) > distanceBetweenPoints)) {
						points.Add(point);

						gestureRenderer.SetVertexCount(++vertexCount);
						gestureRenderer.SetPosition(vertexCount - 1, Utility.WorldCoordinateForGesturePoint(virtualKeyPosition));
					}

				}

				// Capture the gesture, recognize it, fire the recognition event,
				// and clear the gesture from the screen.
				if (Input.GetMouseButtonUp(0)) {

					if (points.Count > minimumPointsToRecognize) {
						gesture = new Gesture(points);
						result = gesture.Recognize(gl, UseProtractor);
                        SetMessage("Gesture is recognized as <color=#ff0000>'" + result.Name + "'</color> with a score of " + result.Score);
					}

				} 
			}
		}

    }

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


/// <summary>
/// Adds a gesture to the library
/// </summary>
public void AddGesture() {
        Gesture newGesture = new Gesture(points, newGestureName.text);
        gl.AddGesture(newGesture);
        SetMessage(newGestureName.text + " has been added to the library");
    }

    public void AddGesture(List<Vector2> addPoints,string newName)
    {
        Gesture newGesture = new Gesture(addPoints, newName);
        gl.AddGesture(newGesture);
        SetMessage(newGestureName.text + " has been added to the library");
    }


    /// <summary>
    /// Shows a message at the bottom of the screen
    /// </summary>
    /// <param name="text"></param>
    public void SetMessage(string text) {
        messageArea.text = text;
    }


	/// <summary>
	/// Remove the gesture from the screen.
	/// </summary>
	void ClearGesture() {
		points.Clear();
		gestureRenderer.SetVertexCount(0);
		vertexCount = 0;
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
