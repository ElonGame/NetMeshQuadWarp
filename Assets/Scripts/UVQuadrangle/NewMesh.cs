using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class NewMesh : MonoBehaviour
{
    [System.Serializable]
    public class PointData
    {
        public float3 vertex;
        public float3 vertexRaw;
        public float2 uv;
        public int index;

        public PointData(float3 vertex, float3 vertexRaw, float2 uv, int index)
        {
            this.vertex = vertex;
            this.vertexRaw = vertex;
            this.uv = uv;
            this.index = index;
        }
    }
    [Header("References")]
    [SerializeField]
    GameObject _stageGoRef = null;

    [Header("Parameters")]
    public Bounds FloorArea;
    public Bounds WallLeftArea;
    public Bounds WallRightArea;

    [SerializeField, Disable]
    float4 _uvFloorMinMax; // xmin, xmax, ymin, ymax

    [SerializeField, Disable]
    List<PointData> _pointFloorList = new List<PointData>();

    [SerializeField, Disable]
    List<PointData> _pointWallLeftList = new List<PointData>();

    [SerializeField, Disable]
    List<PointData> _pointWallRightList = new List<PointData>();

    List<PointData> _pointWallLeftOrgList = new List<PointData>();
    List<PointData> _pointWallRightOrgList = new List<PointData>();

    public GameObject RightWallMesh;
    public GameObject LeftWallMesh;
    public GameObject FloorMesh;
    public Material Mat;
    public bool ShowDebugQuadForDrawUV = false;

    [Header("Private Variables")]
    [SerializeField, Disable]
    float2 _uvFloor;
    [SerializeField, Disable]
    float2 _uvWallLeft;
    [SerializeField, Disable]
    float2 _uvWallRight;

    Vector2[] newUV;
    private void Awake()
    {
        SetupToCalculation();
        Vector3[] rightVertices = new Vector3[26];//18 + 8
        Vector2[] rightUV = new Vector2[26];
        int[] rightTris = new int[96];// 8 * 4 * 3

        Vector3[] leftVertices = new Vector3[23];
        Vector2[] leftUV = new Vector2[23];
        int[] leftTris = new int[84];

        RebuildMesh(LeftWallMesh, _pointWallLeftList, -1, leftVertices, leftUV, leftTris);
        RebuildMesh(RightWallMesh, _pointWallRightList, 1, rightVertices, rightUV, rightTris);
        //BuildFloor(FloorMesh, _pointFloorList);
    }

    void BuildFloor(GameObject TargetMesh, List<PointData> PtDataList) {
        MeshRenderer meshRenderer = TargetMesh.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Mat;
        MeshFilter meshFilter = TargetMesh.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[30];
        Vector2[] uv = new Vector2[30];
        mesh.vertices = vertices;
        mesh.uv = uv;
        meshFilter.mesh = mesh;
        //Triangulator tr = new Triangulator(vertices);
        //int[] indices = tr.Triangulate();
    }
    void RebuildMesh(GameObject TargetMesh, List<PointData> PtDataList, int dir, Vector3[] vertices, Vector2[] uv, int[] tris)
    {
        MeshRenderer meshRenderer = TargetMesh.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Mat;
        MeshFilter meshFilter = TargetMesh.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        //Vector3[] vertices = new Vector3[26];//18 + 8
        //Vector2[] uv = new Vector2[26];
        //int[] tris = new int[96];// 8 * 4 * 3
        int n = 0;
        int m = 0;
        for (var i = 0; i < PtDataList.Count; i+=2){
            
            vertices[i + 0 + n] = PtDataList[i + 0].vertexRaw;
            vertices[i + 1 + n] = PtDataList[i + 1].vertexRaw;
            uv[i + 0 + n] = PtDataList[i + 0].uv;
            uv[i + 1 + n] = PtDataList[i + 1].uv;

            if (i < PtDataList.Count - 2) {
                vertices[i + 2 + n] = (PtDataList[i + 0].vertexRaw + PtDataList[i + 1].vertexRaw + PtDataList[i + 2].vertexRaw + PtDataList[i + 3].vertexRaw) / 4;
                uv[i + 2 + n] = (PtDataList[i + 0].uv + PtDataList[i + 1].uv + PtDataList[i + 2].uv + PtDataList[i + 3].uv) / 4;
            }
            if (i > 0 && dir == 1) {
                int k = (i - 2) * 6;
                tris[k + 0] = 0 + (i - 2 + m);
                tris[k + 1] = 3 + (i - 2 + m);
                tris[k + 2] = 2 + (i - 2 + m);

                tris[k + 3] = 2 + (i - 2 + m);
                tris[k + 4] = 3 + (i - 2 + m);
                tris[k + 5] = 4 + (i - 2 + m);

                tris[k + 6] = 4 + (i - 2 + m);
                tris[k + 7] = 1 + (i - 2 + m);
                tris[k + 8] = 2 + (i - 2 + m);

                tris[k + 9] =  2 + (i - 2 + m);
                tris[k + 10] = 1 + (i - 2 + m);
                tris[k + 11] = 0 + (i - 2 + m);
                m += 1;
            }

            if (i> 0 && dir == -1) {
                int k = (i - 2) * 6;
                tris[k + 0] = 0 + (i - 2 + m);
                tris[k + 1] = 2 + (i - 2 + m);
                tris[k + 2] = 3 + (i - 2 + m);

                tris[k + 3] = 3 + (i - 2 + m);
                tris[k + 4] = 2 + (i - 2 + m);
                tris[k + 5] = 4 + (i - 2 + m);

                tris[k + 6] = 4 + (i - 2 + m);
                tris[k + 7] = 2 + (i - 2 + m);
                tris[k + 8] = 1 + (i - 2 + m);

                tris[k + 9] = 1 + (i - 2 + m);
                tris[k + 10] = 2 + (i - 2 + m);
                tris[k + 11] = 0 + (i - 2 + m);
                m += 1;
            }
            n += 1;
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        //var normals = new List<Vector3>();
        //for (int i=0;i < PtDataList.Count+ (PtDataList.Count - 2)/2; i++) {
        //    normals.Add(normalDir);
        //}
        //mesh.normals = normals.ToArray();

        mesh.triangles = tris;
        meshFilter.mesh = mesh;
    }
    void BuildMesh() {
        MeshRenderer meshRenderer = RightWallMesh.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Mat; 
        MeshFilter meshFilter = RightWallMesh.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[18];//mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = new Vector2[18];
        //int[] tris = new int[48];

        for (var i = 0; i < _pointWallRightList.Count; i++)
        {
            vertices[i] = _pointWallRightList[i].vertexRaw;
            uv[i] = _pointWallRightList[i].uv;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        int[] tris = new int[48]
        {
            0, 2, 3,            // lower left triangle
            3, 1, 0,            // upper right triangle
            
            2, 4, 5,
            5, 3, 2,

            4, 6, 7,
            7, 5, 4,

            6, 8, 9,
            9, 7, 6,

            8, 10, 11,
            11, 9, 8,

            10, 12, 13,
            13, 11, 10,

            12, 14, 15,
            15, 13, 12,

            14, 16, 17,
            17, 15, 14
        };
        mesh.triangles = tris;
        meshFilter.mesh = mesh;
    }

    void OnDrawGizmos()
    {
        DrawDebugGizmosObject();
    }

    void FixTransform()
    {
        transform.localEulerAngles = float3(0);
        transform.localScale = float3(1);

        if (_stageGoRef != null)
        {
            _stageGoRef.transform.localPosition = float3(0);
            _stageGoRef.transform.localEulerAngles = float3(0, 90, 0);
            _stageGoRef.transform.localScale = float3(1);
        }
    }
    void SetupToCalculation()
    {
        if (_stageGoRef != null)
        {
            // --- Floor ---
            SetPointsFromMeshToList(ref _pointFloorList, FloorArea, true);

            if (_pointFloorList == null || _pointFloorList.Count == 0)
            {
                Debug.LogError("Maybe It's not set 【FloorArea】 bounds is this script.");
                return;
            }

            if (_pointFloorList != null && _pointFloorList.Count > 0)
            {
                var uv_xmin = _pointFloorList.Min(v => v.uv.x);
                var uv_xmax = _pointFloorList.Max(v => v.uv.x);
                var uv_ymin = _pointFloorList.Min(v => v.uv.y);
                var uv_ymax = _pointFloorList.Max(v => v.uv.y);

                _uvFloorMinMax = float4(uv_xmin, uv_xmax, uv_ymin, uv_ymax);
            }

            // --- Wall Left ---
            SetPointsFromMeshToList(ref _pointWallLeftList, WallLeftArea, false);
            SetPointsFromMeshToList(ref _pointWallLeftOrgList, WallLeftArea, false);

            if (_pointWallLeftList == null || _pointWallLeftList.Count == 0)
            {
                Debug.LogError("Maybe It's not set 【WallLeftArea】 bounds is this script.");
                return;
            }
            // sort points
            SortWallPointsByPositionYZ(ref _pointWallLeftList);
            // normalize points
            NormalizePointsPositionYZ(ref _pointWallLeftList);

            // --- Wall Right ---
            SetPointsFromMeshToList(ref _pointWallRightList, WallRightArea, false);
            SetPointsFromMeshToList(ref _pointWallRightOrgList, WallRightArea, false);

            if (_pointWallRightList == null || _pointWallRightList.Count == 0)
            {
                Debug.LogError("Maybe It's not set 【WallRightArea】 bounds is this script.");
                return;
            }
            // sort points
            SortWallPointsByPositionYZ(ref _pointWallRightList);
            // normalize points
            NormalizePointsPositionYZ(ref _pointWallRightList);
        }
    }

    void SetPointsFromMeshToList(ref List<PointData> pointList, Bounds area, bool isFloorOrWall)
    {
        var mesh = _stageGoRef.GetComponent<MeshFilter>().sharedMesh;

        pointList.Clear();

        for (var i = 0; i < mesh.vertexCount; i++)
        {
            var posvec4 = _stageGoRef.transform.localToWorldMatrix * mesh.vertices[i];
            var posf3 = float3(posvec4.x, posvec4.y, posvec4.z);

            var xmin = area.min.x;
            var xmax = area.max.x;
            var ymin = area.min.y;
            var ymax = area.max.y;
            var zmin = area.min.z;
            var zmax = area.max.z;

            bool isInBounds = IntersectAABB(posf3, xmin, xmax, ymin, ymax, zmin, zmax);
            if (isInBounds)
            {
                // 壁の場合
                if (isFloorOrWall == false)
                    // 法線が上向き（床だったら）処理しない
                    if (mesh.normals[i].y > 0.5f)
                        break;

                bool isNearPointInList = false;
                for (var k = 0; k < pointList.Count; k++)
                {
                    var dist = Vector3.Distance(pointList[k].vertex, posf3);
                    if (dist < 0.01f)
                    {
                        isNearPointInList = true;
                        break;
                    }
                }
                if (isNearPointInList == false)
                {
                    pointList.Add(new PointData(posf3, posf3, mesh.uv[i], i));
                    //mesh.triangles
                    if (area == WallRightArea)
                    {
                        //Debug.Log("WallRightArea " + posf3 + ". uv " + mesh.uv[i].ToString("F4"));
                    }
                }
            }
        }
    }

    void SortWallPointsByPositionYZ(ref List<PointData> pointList)
    {
        var sortedPointList = pointList.OrderBy(v => v.vertex.z)
                                       .ThenBy(v => v.vertex.y);
        pointList = sortedPointList.ToList<PointData>();
    }

    void NormalizePointsPositionYZ(ref List<PointData> pointList)
    {
        var xAve = pointList.Average(v => v.vertex.x);
        //var xAve = -1.0f;
        var yMin = pointList.Min(v => v.vertex.y);
        var yMax = pointList.Max(v => v.vertex.y);
        var zMin = pointList.Min(v => v.vertex.z);
        var zMax = pointList.Max(v => v.vertex.z);

        // y, z 正規化
        for (var i = 0; i < pointList.Count; i++)
        {
            pointList[i].vertex.x = xAve;
            pointList[i].vertex.y = (pointList[i].vertex.y - yMin) / abs(yMax - yMin);
            pointList[i].vertex.z = (pointList[i].vertex.z - zMin) / abs(zMax - zMin);
        }
    }

    void DrawDebugGizmosObject()
    {
        // --- Wall Left ---
        var wallLeftCol = Color.cyan;
        var wallLeftCenter = transform.position + WallLeftArea.center;
        DrawString("WallLeftArea", wallLeftCenter, 0, 0, wallLeftCol);
        Gizmos.color = wallLeftCol;
        Gizmos.DrawWireCube(wallLeftCenter, WallLeftArea.size);

        if (_pointWallLeftOrgList != null && _pointWallLeftOrgList.Count > 0)
        {
            for (var i = 0; i < _pointWallLeftOrgList.Count; i++)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)_pointWallLeftOrgList[i].vertex, float3(0.1f));
            }
        }

        // --- Wall Right ---
        var wallRightCol = Color.magenta;
        var wallRightCenter = transform.position + WallRightArea.center;
        DrawString("WallRightArea", wallRightCenter, 0, 0, wallRightCol);
        Gizmos.color = wallRightCol;
        Gizmos.DrawWireCube(wallRightCenter, WallRightArea.size);

        if (_pointWallRightOrgList != null && _pointWallRightOrgList.Count > 0)
        {
            for (var i = 0; i < _pointWallRightOrgList.Count; i++)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)_pointWallRightOrgList[i].vertex, float3(0.1f));
            }
        }

        if (ShowDebugQuadForDrawUV)
        {
            // UV Value
            //Gizmos.color = floorCol;
            //Gizmos.DrawCube(float3(_uvFloor.x, _uvFloor.y, 0.05f), float3(0.1f));

            Gizmos.color = wallLeftCol;
            Gizmos.DrawCube(float3(_uvWallLeft.x, _uvWallLeft.y, 0.05f), float3(0.1f));

            Gizmos.color = wallRightCol;
            Gizmos.DrawCube(float3(_uvWallRight.x, _uvWallRight.y, 0.05f), float3(0.1f));
        }
    }
    #region Helper Functions

    static public void DrawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if (view == null || view.camera == null)
            return;

        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();

#endif

    }

    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }
    private void OnDestroy()
    { 
    
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {
#if UNITY_EDITOR
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
#endif
        return Vector3.zero;
    }


    public static float Fit(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    public static float FitAndClip(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return Mathf.Max(Mathf.Min((x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin, outMax), outMin);
    }

    bool IntersectAABB(float3 position, float xmin, float xmax, float ymin, float ymax, float zmin, float zmax)
    {
        if (position.x < xmin) return false;
        if (position.x > xmax) return false;
        if (position.y < ymin) return false;
        if (position.y > ymax) return false;
        if (position.z < zmin) return false;
        if (position.z > zmax) return false;

        return true;
    }
    #endregion
}