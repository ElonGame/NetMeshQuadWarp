using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;
public class MeshsQuadrangle : MonoBehaviour
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
    [SerializeField]
    Shader _setQuadrangleDataShader = null;
    [SerializeField]
    RenderTexture SpoutRT = null;
    public GameObject RightWallMesh;
    public GameObject LeftWallMesh;
    public GameObject FloorMesh;
    public Material Mat;
    public Material _resultRendererMat;

    [Header("Parameters")]
    public Bounds FloorArea;
    public Bounds WallLeftArea;
    public Bounds WallRightArea;
    public bool ShowDebugQuadForDrawUV = false;

    [SerializeField, Disable]
    float4 _uvFloorMinMax;          // xmin, xmax, ymin, ymax

    [SerializeField, Disable]
    List<PointData> _pointFloorList = new List<PointData>();

    [SerializeField, Disable]
    List<PointData> _pointWallLeftList = new List<PointData>();

    [SerializeField, Disable]
    List<PointData> _pointWallRightList = new List<PointData>();

    List<PointData> _pointWallLeftOrgList = new List<PointData>();
    List<PointData> _pointWallRightOrgList = new List<PointData>();

    [Header("Private Variables")]
    [SerializeField, Disable]
    float2 _uvFloor;
    [SerializeField, Disable]
    float2 _uvWallLeft;
    [SerializeField, Disable]
    float2 _uvWallRight;

    
    [SerializeField]
    Texture2D _checkboard = null;

    [SerializeField]
    float[] _matrix = new float[9];
    [SerializeField]
    float[] _inverseMatrix = new float[9];

    [SerializeField]
    float[] _matrixTest = new float[9];
    [SerializeField]
    float[] _inverseMatrixTest = new float[9];
    Material _setQuadrangleDataMat = null;
    private MaterialPropertyBlock[] _RwMaterialPropertyBlock;
    private MaterialPropertyBlock[] _LwMaterialPropertyBlock;
    private MaterialPropertyBlock _materialPropertyBlock0;
    private MaterialPropertyBlock _materialPropertyBlock1;

    private void Awake()
    {
        FBOUtility.CreateMaterial(ref _setQuadrangleDataMat, _setQuadrangleDataShader);
        _materialPropertyBlock0 = new MaterialPropertyBlock();
        _materialPropertyBlock1 = new MaterialPropertyBlock();

        _RwMaterialPropertyBlock = new MaterialPropertyBlock[8];
        _LwMaterialPropertyBlock = new MaterialPropertyBlock[7];

        for (int i = 0; i < (18 - 2) / 2; i++) {
            _RwMaterialPropertyBlock[i] = new MaterialPropertyBlock();
        }
        for (int i = 0; i < (16 - 2) / 2; i++)
        {
            _LwMaterialPropertyBlock[i] = new MaterialPropertyBlock();
        }

        SetupToCalculation();

        for (int i = 0; i < _pointWallRightList.Count-2; i += 2)
        //for (int i = 8; i < 10; i += 2)
        {
            BuildMesh(_pointWallRightList, 1,  i, RightWallMesh, Mat, _RwMaterialPropertyBlock[i / 2]);
        }

        for (int i = 0; i < _pointWallLeftList.Count - 2; i += 2)
        {
            BuildMesh(_pointWallLeftList, -1, i, LeftWallMesh, Mat, _LwMaterialPropertyBlock[i / 2]);
        }
    }
    private void OnDestroy()
    {
        FBOUtility.DeleteMaterial(_setQuadrangleDataMat);
    }
    void OnDrawGizmos()
    {
        DrawDebugGizmosObject();
    }
    void BuildMesh(List<PointData> ptData, int dir, int pointIndex, GameObject parentGO, Material mat, MaterialPropertyBlock propblock) {
        GameObject objSpawn;
        objSpawn = new GameObject("Meshs");
        objSpawn.transform.parent = parentGO.transform;
        objSpawn.transform.localPosition = Vector3.zero;
        objSpawn.transform.localRotation = Quaternion.identity;
        objSpawn.transform.localScale = Vector3.one;
        MeshRenderer meshRenderer = objSpawn.AddComponent<MeshRenderer>();

        meshRenderer.sharedMaterial = mat; //_setQuadrangleDataMat;// 
        MeshFilter meshFilter = objSpawn.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];//mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = new Vector2[4];

        for (var i = pointIndex; i < pointIndex+4; i++)
        {
            vertices[i- pointIndex] = ptData[i].vertexRaw;
            uv[i- pointIndex] = ptData[i].uv;
            //var p = vertices[i - pointIndex];
            //Debug.Log("vertex " + i + p.ToString("F4"));
        }
       
        mesh.vertices = vertices;
        mesh.uv = uv;
        int[] tris = new int[6];

        if (dir == 1)
        {
            tris = new int[6]
            {
                0, 2, 3,           // lower left triangle
                3, 1, 0            // upper right triangle
            };
        }
        else {
            tris = new int[6]
            {
                0, 3, 2,           // lower left triangle
                0, 1, 3            // upper right triangle
            };
        }
        mesh.triangles = tris;
        meshFilter.mesh = mesh;

        TranscribeUVFromMeshToWall(
            pointIndex,
            meshRenderer,
            propblock,
            mat,
            ptData[pointIndex + 0].vertexRaw,
            ptData[pointIndex + 1].vertexRaw,
            ptData[pointIndex + 2].vertexRaw,
            ptData[pointIndex + 3].vertexRaw,
            ptData[pointIndex + 0].uv,
            ptData[pointIndex + 1].uv,
            ptData[pointIndex + 2].uv,
            ptData[pointIndex + 3].uv
        );
    }
    float[] _homography = null;
    float[] _homographyVertex = null;
    float[] _invHomographyVertex = null;
    float[] _invHomography = null;

    void TranscribeUVFromMeshToWall(int debugindex, MeshRenderer _meshRenderer, MaterialPropertyBlock _propBlock, Material _mat, Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3, Vector2 _p0, Vector2 _p1, Vector2 _p2, Vector2 _p3)
    {
        _meshRenderer.GetPropertyBlock(_propBlock);

        var p0 = _p0;
        var p1 = _p1;
        var p2 = _p2;
        var p3 = _p3;
        Vector2 vertex0 = new Vector2(_vertex0.y, _vertex0.z);
        Vector2 vertex1 = new Vector2(_vertex1.y, _vertex1.z);
        Vector2 vertex2 = new Vector2(_vertex2.y, _vertex2.z);
        Vector2 vertex3 = new Vector2(_vertex3.y, _vertex3.z);
        Vector3[] source = new[] { new Vector3(_p0.x, _p0.y, 0), new Vector3(_p2.x, _p2.y, 0), new Vector3(_p1.x, _p1.y, 0), new Vector3(_p3.x, _p3.y, 0) };
        Vector3[] destination = new[] { new Vector3(_vertex0.y, _vertex0.z, 0), new Vector3(_vertex2.y, _vertex2.z, 0), new Vector3(_vertex1.y, _vertex1.z, 0), new Vector3(_vertex3.y, _vertex3.z, 0) };

        FindHomography(ref source, ref destination, ref _matrix);
        _inverseMatrix = CalcInverseMatrix(_matrix);
        _propBlock.SetFloatArray("_Homography", _matrix);
        _propBlock.SetFloatArray("_InvHomography", _inverseMatrix);

        _homographyVertex = CalcHomographyMatrix(vertex0, vertex2, vertex1, vertex3);
        _invHomographyVertex = CalcInverseMatrix(_homographyVertex);

        _homography = CalcHomographyMatrix(p0, p2, p1, p3);
        _invHomography = CalcInverseMatrix(_homography);
        _propBlock.SetFloatArray("_InvHomographyUV", _invHomography);
        _propBlock.SetVectorArray("_VertexValues", new Vector4[] {
            new Vector4(vertex0.x, vertex0.y, 0.0f, 1.0f),
            new Vector4(vertex1.x, vertex1.y, 0.0f, 1.0f),
            new Vector4(vertex2.x, vertex2.y, 0.0f, 1.0f),
            new Vector4(vertex3.x, vertex3.y, 0.0f, 1.0f)
        });
        _propBlock.SetVectorArray("_UVValues", new Vector4[] {
            new Vector4(0, 0, 0.0f, 1.0f),
            new Vector4(1, 0, 0.0f, 1.0f),
            new Vector4(0, 1, 0.0f, 1.0f),
            new Vector4(1, 1, 0.0f, 1.0f)
            //new Vector4(p0.x, p0.y, 0.0f, 1.0f),
            //new Vector4(p1.x, p1.y, 0.0f, 1.0f),
            //new Vector4(p2.x, p2.y, 0.0f, 1.0f),
            //new Vector4(p3.x, p3.y, 0.0f, 1.0f)
        });
        _propBlock.SetTexture("_SpoutTex", _checkboard);// SpoutRT); //

        //Graphics.Blit(null, _uvQuadRef, _mat, 0);

        _meshRenderer.SetPropertyBlock(_propBlock);
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
        var floorCol = Color.yellow;
        var floorCenter = transform.position + FloorArea.center;
        DrawString("FloorArea", floorCenter, 0, 0, floorCol);
        Gizmos.color = floorCol;
        Gizmos.DrawWireCube(floorCenter, FloorArea.size);

        if (_pointFloorList != null && _pointFloorList.Count > 0)
        {
            for (var i = 0; i < _pointFloorList.Count; i++)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)_pointFloorList[i].vertex, float3(0.08f));
            }
        }

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
            Gizmos.color = floorCol;
            Gizmos.DrawCube(float3(_uvFloor.x, _uvFloor.y, 0.05f), float3(0.1f));

            Gizmos.color = wallLeftCol;
            Gizmos.DrawCube(float3(_uvWallLeft.x, _uvWallLeft.y, 0.05f), float3(0.1f));

            Gizmos.color = wallRightCol;
            Gizmos.DrawCube(float3(_uvWallRight.x, _uvWallRight.y, 0.05f), float3(0.1f));
        }
    }

    float[] CalcHomographyMatrix(Vector2 p00, Vector2 p01, Vector2 p10, Vector2 p11)
    {
        var x00 = p00.x;
        var y00 = p00.y;
        var x01 = p01.x;
        var y01 = p01.y;
        var x10 = p10.x;
        var y10 = p10.y;
        var x11 = p11.x;
        var y11 = p11.y;

        var a = x10 - x11;
        var b = x01 - x11;
        var c = x00 - x01 - x10 + x11;
        var d = y10 - y11;
        var e = y01 - y11;
        var f = y00 - y01 - y10 + y11;

        var h13 = x00;
        var h23 = y00;
        var h32 = (c * d - a * f) / (b * d - a * e);
        var h31 = (c * e - b * f) / (a * e - b * d);
        var h11 = x10 - x00 + h31 * x10;
        var h12 = x01 - x00 + h32 * x01;
        var h21 = y10 - y00 + h31 * y10;
        var h22 = y01 - y00 + h32 * y01;

        return new float[] { h11, h12, h13, h21, h22, h23, h31, h32, 1f };
    }
    float[] CalcInverseMatrix(float[] mat)
    {
        var i11 = mat[0];
        var i12 = mat[1];
        var i13 = mat[2];
        var i21 = mat[3];
        var i22 = mat[4];
        var i23 = mat[5];
        var i31 = mat[6];
        var i32 = mat[7];
        var i33 = 1f;
        var a = 1f / (
            +(i11 * i22 * i33)
            + (i12 * i23 * i31)
            + (i13 * i21 * i32)
            - (i13 * i22 * i31)
            - (i12 * i21 * i33)
            - (i11 * i23 * i32)
        );

        var o11 = (i22 * i33 - i23 * i32) / a;
        var o12 = (-i12 * i33 + i13 * i32) / a;
        var o13 = (i12 * i23 - i13 * i22) / a;
        var o21 = (-i21 * i33 + i23 * i31) / a;
        var o22 = (i11 * i33 - i13 * i31) / a;
        var o23 = (-i11 * i23 + i13 * i21) / a;
        var o31 = (i21 * i32 - i22 * i31) / a;
        var o32 = (-i11 * i32 + i12 * i31) / a;
        var o33 = (i11 * i22 - i12 * i21) / a;

        return new float[] { o11, o12, o13, o21, o22, o23, o31, o32, o33 };
    }

    void FindHomography(ref Vector3[] src, ref Vector3[] dest, ref float[] homography)
    {
        // originally by arturo castro - 08/01/2010  
        //  
        // create the equation system to be solved  
        //  
        // from: Multiple View Geometry in Computer Vision 2ed  
        //       Hartley R. and Zisserman A.  
        //  
        // x' = xH  
        // where H is the homography: a 3 by 3 matrix  
        // that transformed to inhomogeneous coordinates for each point  
        // gives the following equations for each point:  
        //  
        // x' * (h31*x + h32*y + h33) = h11*x + h12*y + h13  
        // y' * (h31*x + h32*y + h33) = h21*x + h22*y + h23  
        //  
        // as the homography is scale independent we can let h33 be 1 (indeed any of the terms)  
        // so for 4 points we have 8 equations for 8 terms to solve: h11 - h32  
        // after ordering the terms it gives the following matrix  
        // that can be solved with gaussian elimination:  

        float[,] P = new float[,]{
            {-src[0].x, -src[0].y, -1,   0,   0,  0, src[0].x*dest[0].x, src[0].y*dest[0].x, -dest[0].x }, // h11  
            {  0,   0,  0, -src[0].x, -src[0].y, -1, src[0].x*dest[0].y, src[0].y*dest[0].y, -dest[0].y }, // h12  
          
            {-src[1].x, -src[1].y, -1,   0,   0,  0, src[1].x*dest[1].x, src[1].y*dest[1].x, -dest[1].x }, // h13  
            {  0,   0,  0, -src[1].x, -src[1].y, -1, src[1].x*dest[1].y, src[1].y*dest[1].y, -dest[1].y }, // h21  
          
            {-src[2].x, -src[2].y, -1,   0,   0,  0, src[2].x*dest[2].x, src[2].y*dest[2].x, -dest[2].x }, // h22  
            {  0,   0,  0, -src[2].x, -src[2].y, -1, src[2].x*dest[2].y, src[2].y*dest[2].y, -dest[2].y }, // h23  
          
            {-src[3].x, -src[3].y, -1,   0,   0,  0, src[3].x*dest[3].x, src[3].y*dest[3].x, -dest[3].x }, // h31  
            {  0,   0,  0, -src[3].x, -src[3].y, -1, src[3].x*dest[3].y, src[3].y*dest[3].y, -dest[3].y }, // h32  
    	    };

        GaussianElimination(ref P, 9);

        // gaussian elimination gives the results of the equation system  
        // in the last column of the original matrix.  
        // opengl needs the transposed 4x4 matrix:  
        float[] aux_H ={
                P[0,8], P[3,8], 0, P[6,8],    // h11  h21  0  h31  
	            P[1,8], P[4,8], 0, P[7,8],    // h12  h22  0  h32  
	            0     , 0     , 0, 0     ,    // 0    0    0  0  
	            P[2,8], P[5,8], 0, 1          // h13  h23  0  h33  
            };

        float[] aux_H33transpose ={
                P[0,8], P[3,8], P[6,8],    // h11  h21  h31  
	            P[1,8], P[4,8], P[7,8],    // h12  h22  h32  
	            P[2,8], P[5,8], 1          // h13  h23  h33  
            };

        float[] aux_H33 ={
                P[0,8], P[1,8], P[2,8],    // h11  h12  h13  
	            P[3,8], P[4,8], P[5,8],    // h21  h22  h23  
	            P[6,8], P[7,8], 1          // h31  h32  h33  
            };

        for (int i = 0; i < 9; i++) homography[i] = aux_H33[i];
        //for (int i = 0; i < 16; i++) homography[i] = aux_H[i];

    }

    void GaussianElimination(ref float[,] A, int n)
    {
        // originally by arturo castro - 08/01/2010  
        //  
        // ported to c from pseudocode in  
        // http://en.wikipedia.org/wiki/Gaussian_elimination  

        int i = 0;
        int j = 0;
        int m = n - 1;
        while (i < m && j < n)
        {
            // Find pivot in column j, starting in row i:  
            int maxi = i;
            for (int k = i + 1; k < m; k++)
            {
                if (Mathf.Abs(A[k, j]) > Mathf.Abs(A[maxi, j]))
                {
                    maxi = k;
                }
            }
            if (A[maxi, j] != 0)
            {
                //swap rows i and maxi, but do not change the value of i  
                if (i != maxi)
                    for (int k = 0; k < n; k++)
                    {
                        float aux = A[i, k];
                        A[i, k] = A[maxi, k];
                        A[maxi, k] = aux;
                    }
                //Now A[i,j] will contain the old value of A[maxi,j].  
                //divide each entry in row i by A[i,j]  
                float A_ij = A[i, j];
                for (int k = 0; k < n; k++)
                {
                    A[i, k] /= A_ij;
                }
                //Now A[i,j] will have the value 1.  
                for (int u = i + 1; u < m; u++)
                {
                    //subtract A[u,j] * row i from row u  
                    float A_uj = A[u, j];
                    for (int k = 0; k < n; k++)
                    {
                        A[u, k] -= A_uj * A[i, k];
                    }
                    //Now A[u,j] will be 0, since A[u,j] - A[i,j] * A[u,j] = A[u,j] - 1 * A[u,j] = 0.  
                }

                i++;
            }
            j++;
        }

        //back substitution  
        for (int k = m - 2; k >= 0; k--)
        {
            for (int l = k + 1; l < n - 1; l++)
            {
                A[k, m] -= A[k, l] * A[l, m];
                //A[i*n+j]=0;  
            }
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
