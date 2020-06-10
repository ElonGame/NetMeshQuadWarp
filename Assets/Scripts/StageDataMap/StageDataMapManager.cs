using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace NetValley
{
    public class StageDataMapManager : MonoBehaviour
    {
        [Header("Parameters")]
        public int BufferWidth  = 1024;
        public int BufferHeight = 1024;

        [Header("References")]
        [SerializeField]
        ConvertSensorInputToUVCoords _script = null;

        [Header("Resources")]
        [SerializeField]
        Shader _setQuadrangleDataShader = null;
        [SerializeField]
        Shader _setTriangleDataShader = null;
        [SerializeField]
        Shader _calcGradientShader = null;
        [SerializeField]
        Shader _resultRendererMapShader = null;
        [SerializeField]
        Material _resultRendererMapMatTest = null;
        [SerializeField]
        float[] _matrix = new float[9];
        [SerializeField]
        float[] _inverseMatrix = new float[9];

        [Header("Private Properties")]
        [SerializeField, Disable]
        RenderTexture _gravityDirectionMap       = null;
        [SerializeField, Disable]                
        RenderTexture _worldPositionMap          = null;
        [SerializeField, Disable]
        RenderTexture _worldPositionYGradientMap = null;
        [SerializeField, Disable]
        RenderTexture _uvMap                     = null;
        [SerializeField, Disable]                
        RenderTexture _normalMap                 = null;
        [SerializeField, Disable]                
        RenderTexture _emitterUVMap              = null;
        [SerializeField, Disable]
        RenderTexture _ceilingMap                = null;
        [SerializeField, Disable]
        RenderTexture _gravityInteractionMap     = null;
        [SerializeField]
        RenderTexture _mainMap                   = null;
        [SerializeField]
        RenderTexture _uvQuadRef                 = null;
        [SerializeField]
        RenderTexture _gravitydirmap = null;
        [SerializeField]
        Texture2D _checkboard                    = null;
        [SerializeField]
        Material _resultRendererMapMaterial      = null;

        bool _hasCreatedGravityMap                     = false;
        bool _hasCreatedWorldPositionAndNormalAndUVMap = false;
        bool _hasCreatedEmitterUVMap                   = false;
        //bool _hasCreatedceilMap                        = false;
        bool _hasCreatedInteractionMap                 = false;
        bool _hasCreatedResultRendMap                 = false;


        Material _setQuadrangleDataMat = null;
        Material _setTriangleDataMat   = null;
        Material _calcGradientMat      = null;
        Material _resultRendererMapMat = null;
        

        public RenderTexture gravityDirectionMap    => _gravityDirectionMap;
        public RenderTexture worldPositionMap       => _worldPositionMap;
        public RenderTexture worldPositionYGradient => _worldPositionYGradientMap;
        public RenderTexture uvMap                  => _uvMap;
        public RenderTexture normalMap              => _normalMap;
        public RenderTexture emitterMap             => _emitterUVMap;
        public RenderTexture ceilMap                => _ceilingMap;
        public RenderTexture gravityInteractionMap => _gravityInteractionMap;


        void Start()
        {
            FBOUtility.CreateBuffer(ref _gravityDirectionMap,       BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            FBOUtility.CreateBuffer(ref _worldPositionMap,          BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            FBOUtility.CreateBuffer(ref _worldPositionYGradientMap, BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            FBOUtility.CreateBuffer(ref _uvMap,                     BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            FBOUtility.CreateBuffer(ref _normalMap,                 BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            FBOUtility.CreateBuffer(ref _emitterUVMap,              BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Point   );
            FBOUtility.CreateBuffer(ref _ceilingMap,                BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Point);
            FBOUtility.CreateBuffer(ref _gravityInteractionMap,     BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);
            //FBOUtility.CreateBuffer(ref _resultRendererMap,         BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);

            FBOUtility.CreateMaterial(ref _setQuadrangleDataMat, _setQuadrangleDataShader);
            FBOUtility.CreateMaterial(ref _setTriangleDataMat,   _setTriangleDataShader  );
            FBOUtility.CreateMaterial(ref _calcGradientMat,      _calcGradientShader     );
            FBOUtility.CreateMaterial(ref _resultRendererMapMat, _resultRendererMapShader);

            // GetComponent<Renderer>().sharedMaterial.mainTexture = _gravityDirectionMap;
        }

        void Update()
        {
            CreateGravityDirectionMap();
            CreatePositionMap();
            CreateEmitterMap();
            CreateInteractionMap();
            CreateUvFixMap();
            //createUVquadMap();
        }

        private void OnDestroy()
        {
            FBOUtility.DeleteBuffer(_gravityDirectionMap);
            FBOUtility.DeleteBuffer(_worldPositionMap);
            FBOUtility.DeleteBuffer(_worldPositionYGradientMap);
            FBOUtility.DeleteBuffer(_uvMap);
            FBOUtility.DeleteBuffer(_normalMap);
            FBOUtility.DeleteBuffer(_emitterUVMap);
            FBOUtility.DeleteBuffer(_gravityInteractionMap);

            FBOUtility.DeleteMaterial(_setQuadrangleDataMat);
            FBOUtility.DeleteMaterial(_setTriangleDataMat);
            FBOUtility.DeleteMaterial(_calcGradientMat);
            FBOUtility.DeleteMaterial(_resultRendererMapMat);
        }

        void CreatePositionMap()
        {
            if (_hasCreatedWorldPositionAndNormalAndUVMap == true)
                return;

            FBOUtility.CreateBuffer  (ref _worldPositionMap, BufferWidth, BufferHeight, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);

            if (_script.StageGo != null)
            {
                var mesh = _script.StageGo.transform.GetComponent<MeshFilter>().sharedMesh;

                FBOUtility.ClearBuffer(_worldPositionMap);
                FBOUtility.ClearBuffer(_uvMap);
                FBOUtility.ClearBuffer(_normalMap);

                for (var sm = 0; sm < mesh.subMeshCount; sm++)
                {
                    var vertices = mesh.vertices;
                    var uvs = mesh.uv;
                    var normals = mesh.normals;
                    var indices = mesh.GetIndices(sm);
                    
                    for (var i = 0; i < indices.Length; i += 3)
                    {
                        var idx0 = indices[i + 0];
                        var idx1 = indices[i + 1];
                        var idx2 = indices[i + 2];

                        _setTriangleDataMat.SetVector("_UV0", new Vector3(uvs[idx0].x, uvs[idx0].y, 0.0f));
                        _setTriangleDataMat.SetVector("_UV1", new Vector3(uvs[idx1].x, uvs[idx1].y, 0.0f));
                        _setTriangleDataMat.SetVector("_UV2", new Vector3(uvs[idx2].x, uvs[idx2].y, 0.0f));

                        // --- Position ---
                        // flip x and z
                        _setTriangleDataMat.SetVector("_Color0", new Vector4(vertices[idx0].z, vertices[idx0].y, vertices[idx0].x, 1.0f));
                        _setTriangleDataMat.SetVector("_Color1", new Vector4(vertices[idx1].z, vertices[idx1].y, vertices[idx1].x, 1.0f));
                        _setTriangleDataMat.SetVector("_Color2", new Vector4(vertices[idx2].z, vertices[idx2].y, vertices[idx2].x, 1.0f));
                        Graphics.Blit(null, _worldPositionMap, _setTriangleDataMat, 0);

                        // --- UV ---
                        _setTriangleDataMat.SetVector("_Color0", new Vector4(uvs[idx0].x, uvs[idx0].y, 0.0f, 1.0f));
                        _setTriangleDataMat.SetVector("_Color1", new Vector4(uvs[idx1].x, uvs[idx1].y, 0.0f, 1.0f));
                        _setTriangleDataMat.SetVector("_Color2", new Vector4(uvs[idx2].x, uvs[idx2].y, 0.0f, 1.0f));
                        Graphics.Blit(null, _uvMap, _setTriangleDataMat, 0);

                        // --- Normal ---
                        _setTriangleDataMat.SetVector("_Color0", new Vector4(normals[idx0].x, normals[idx0].y, normals[idx0].z, 1.0f));
                        _setTriangleDataMat.SetVector("_Color1", new Vector4(normals[idx1].x, normals[idx1].y, normals[idx1].z, 1.0f));
                        _setTriangleDataMat.SetVector("_Color2", new Vector4(normals[idx2].x, normals[idx2].y, normals[idx2].z, 1.0f));
                        Graphics.Blit(null, _normalMap, _setTriangleDataMat, 0);
                    }
                }

                Graphics.Blit(_worldPositionMap, _worldPositionYGradientMap, _calcGradientMat, 0);

                _hasCreatedWorldPositionAndNormalAndUVMap = true;
            }
        }
        Matrix4x4 CalcHomography(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var sx = p0.x - p1.x + p2.x - p3.x;
            var sy = p0.y - p1.y + p2.y - p3.y;

            var dx1 = p1.x - p2.x;
            var dx2 = p3.x - p2.x;
            var dy1 = p1.y - p2.y;
            var dy2 = p3.y - p2.y;

            var z = (dy1 * dx2) - (dx1 * dy2);
            var g = ((sx * dy1) - (sy * dx1)) / z;
            var h = ((sy * dx2) - (sx * dy2)) / z;

            var system = new[]{
            p3.x * g - p0.x + p3.x,
            p1.x * h - p0.x + p1.x,
            p0.x,
            p3.y * g - p0.y + p3.y,
            p1.y * h - p0.y + p1.y,
            p0.y,
            g,
            h,
        };

            var mtx = Matrix4x4.identity;
            mtx.m00 = system[0]; mtx.m01 = system[1]; mtx.m02 = system[2];
            mtx.m10 = system[3]; mtx.m11 = system[4]; mtx.m12 = system[5];
            mtx.m20 = system[6]; mtx.m21 = system[7]; mtx.m22 = 1f;

            return mtx;
        }
        void createUVquadMap() {
            if (_script != null && _script.PointWallLeftList != null && _script.PointWallRightList != null)
            {
                RenderTexture store = RenderTexture.active;
                Graphics.SetRenderTarget(_uvQuadRef);
                GL.Clear(false, true, Color.black);
                Graphics.SetRenderTarget(store);
                _resultRendererMapMat.SetTexture("_CheckBoardTex", _checkboard);

                for (var i = 6; i < 8; i += 2)
                {
                    var homography = CalcHomography(_script.PointWallRightList[i + 0].uv, _script.PointWallRightList[i + 2].uv, _script.PointWallRightList[i + 3].uv, _script.PointWallRightList[i + 1].uv).inverse;
                        _resultRendererMapMat.SetMatrix("_HomographyMat", homography);
                        _resultRendererMapMat.SetVectorArray("_VertexValues", new Vector4[] {
                        new Vector4(_script.PointWallRightList[i + 0].uv.x, _script.PointWallRightList[i + 0].uv.y, 0.0f, 1.0f),
                        new Vector4(_script.PointWallRightList[i + 1].uv.x, _script.PointWallRightList[i + 1].uv.y, 0.0f, 1.0f),
                        new Vector4(_script.PointWallRightList[i + 2].uv.x, _script.PointWallRightList[i + 2].uv.y, 0.0f, 1.0f),
                        new Vector4(_script.PointWallRightList[i + 3].uv.x, _script.PointWallRightList[i + 3].uv.y, 0.0f, 1.0f)
                    });
                    Graphics.Blit(null, _uvQuadRef, _resultRendererMapMat, 0);
                }
            }
                
        }
        void CreateUvFixMap()
        {

            //if (_hasCreatedResultRendMap == true)
            //    return;
            if (_script != null && _script.PointWallLeftList != null && _script.PointWallRightList != null)
            {
                RenderTexture store = RenderTexture.active;
                Graphics.SetRenderTarget(_uvQuadRef);
                GL.Clear(false, true, Color.black);
                Graphics.SetRenderTarget(store);
                _resultRendererMapMaterial.SetTexture("_CheckBoardTex", _checkboard);
                //for (var i = 0; i < _script.PointWallLeftList.Count - 2; i += 2)
                //{
                //    // Debug.Log("asd");

                //    TranscribeUVDirectionFromMeshToWallResultMap(
                //        _script.PointWallLeftList[i + 0].vertex,
                //        _script.PointWallLeftList[i + 1].vertex,
                //        _script.PointWallLeftList[i + 2].vertex,
                //        _script.PointWallLeftList[i + 3].vertex,
                //        _script.PointWallLeftList[i + 0].uv,
                //        _script.PointWallLeftList[i + 1].uv,
                //        _script.PointWallLeftList[i + 2].uv,
                //        _script.PointWallLeftList[i + 3].uv
                //    );
                //}

                for (var i = 8; i < 10; i += 2)
                {
                    TranscribeUVFromMeshToWall(
                        _script.PointWallRightList[i + 0].vertexRaw,
                        _script.PointWallRightList[i + 1].vertexRaw,
                        _script.PointWallRightList[i + 2].vertexRaw,
                        _script.PointWallRightList[i + 3].vertexRaw,
                        _script.PointWallRightList[i + 0].uv,
                        _script.PointWallRightList[i + 1].uv,
                        _script.PointWallRightList[i + 2].uv,
                        _script.PointWallRightList[i + 3].uv
                   );
                    //TranscribeUVDirectionFromMeshToWallResultMap(

                    //    _script.PointWallRightList[i + 0].vertex,
                    //    _script.PointWallRightList[i + 1].vertex,
                    //    _script.PointWallRightList[i + 2].vertex,
                    //    _script.PointWallRightList[i + 3].vertex,
                    //    _script.PointWallRightList[i + 0].uv,
                    //    _script.PointWallRightList[i + 1].uv,
                    //    _script.PointWallRightList[i + 2].uv,
                    //    _script.PointWallRightList[i + 3].uv
                    //);
                }
                _hasCreatedResultRendMap = true;
            }
        }
        void CreateGravityDirectionMap()
        {
            if (_hasCreatedGravityMap == true)
                return;

            if (_script != null && _script.PointWallLeftList != null && _script.PointWallRightList != null)
            {
                //Debug.Log("adasda");

                RenderTexture store = RenderTexture.active;
                Graphics.SetRenderTarget(_gravityDirectionMap);
                GL.Clear(false, true, Color.black);
                Graphics.SetRenderTarget(store);

                for (var i = 0; i < _script.PointWallLeftList.Count - 2; i += 2)
                {
                    TranscribeUVDirectionFromMeshToMap(
                        _script.PointWallLeftList[i + 0].uv,
                        _script.PointWallLeftList[i + 1].uv,
                        _script.PointWallLeftList[i + 2].uv,
                        _script.PointWallLeftList[i + 3].uv
                    );
                }//1032 reverse if counterclockwise

                for (var i = 0; i < _script.PointWallRightList.Count - 2; i += 2)
                {
                    TranscribeUVDirectionFromMeshToMap(
                        _script.PointWallRightList[i + 0].uv,
                        _script.PointWallRightList[i + 1].uv,
                        _script.PointWallRightList[i + 2].uv,
                        _script.PointWallRightList[i + 3].uv
                    );
                }//1032 reverse if clockwise

                for (var i = 0; i < (_script.PointWallLeftList.Count) - 2; i += 2)
                {
                    TranscribeUVDirectionFromMeshToMap(
                        _script.PointWallLeftList[i + 0].uv,
                        _script.PointWallRightList[i + 0].uv,
                        _script.PointWallLeftList[i + 2].uv,
                        _script.PointWallRightList[i + 2].uv
                    );
                }
                _hasCreatedGravityMap = true;
            }
        }

        void CreateInteractionMap() {
            if (_hasCreatedInteractionMap == true)
                return;

            if (_script != null && _script.PointWallLeftList != null && _script.PointWallRightList != null)
            {
                RenderTexture store = RenderTexture.active;
                Graphics.SetRenderTarget(_gravityInteractionMap);
                GL.Clear(false, true, Color.black);
                Graphics.SetRenderTarget(store);

                for (var i = 0; i < _script.PointWallLeftList.Count - 2; i += 2)
                {
                    TranscribeUVDirectionFromMeshToInteractionMap(
                        _script.PointWallLeftList[i + 1].uv,
                        _script.PointWallLeftList[i + 0].uv,
                        _script.PointWallLeftList[i + 3].uv,
                        _script.PointWallLeftList[i + 2].uv
                    );
                }//1032 reverse if counterclockwise

                for (var i = 0; i < _script.PointWallRightList.Count - 2; i += 2)
                {
                    TranscribeUVDirectionFromMeshToInteractionMap(
                        _script.PointWallRightList[i + 0].uv,
                        _script.PointWallRightList[i + 1].uv,
                        _script.PointWallRightList[i + 2].uv,
                        _script.PointWallRightList[i + 3].uv
                    );
                }//1032 reverse if clockwise

                for (var i = 0; i < (_script.PointWallLeftList.Count) - 2; i += 2)
                {
                    TranscribeUVDirectionFromMeshToInteractionMap(
                        _script.PointWallLeftList[i + 0].uv,
                        _script.PointWallRightList[i + 0].uv,
                        _script.PointWallLeftList[i + 2].uv,
                        _script.PointWallRightList[i + 2].uv
                    );
                }
                _hasCreatedInteractionMap = true;
            }
        }

        void CreateEmitterMap()
        {
            if (_hasCreatedEmitterUVMap == true)
                return;

            if (_script != null && _script.PointWallLeftList != null && _script.PointWallRightList != null)
            {
                Texture2D tex = new Texture2D(BufferWidth, BufferHeight, TextureFormat.RGBAFloat, false);
                tex.hideFlags  = HideFlags.DontSave;
                tex.filterMode = FilterMode.Point;
                
                var wNum = BufferWidth;
                var hNum = BufferHeight;
                var dw = 1.0f / wNum;
                var dh = 1.0f / hNum;
                for (var h = 0; h < hNum; h++)
                {
                    for (var w = 0; w < wNum; w++)
                    {
                        var uv = float2(0, 0);
                        if (w < wNum * 0.5f)//left wall
                            uv = _script.GetWallLeftUV(float2(dh * h, 1.0f - (dw * 2.0f * w)), false);//w [0,wNum/2]
                        else
                            uv = _script.GetWallRightUV(float2(dh * h, 0.0f + (dw * 2.0f * w - 1.0f)), false);
                        tex.SetPixel(w, h, new Color(uv.x, uv.y, 0.0f, 1.0f));
                    }
                }
                tex.Apply();

                Graphics.Blit(tex, _emitterUVMap);

                if (Application.isEditor)
                    DestroyImmediate(tex);
                else
                    Destroy(tex);
                tex = null;
            }

            _hasCreatedEmitterUVMap = true;
        }

        //[SerializeField, Disable]
        float[] _homography = null;
        float[] _homographyVertex = null;
        float[] _invHomographyVertex = null;
        //[SerializeField, Disable]
        float[] _invHomography = null;

        void TranscribeVertexFromMeshToWallResultMap(Vector3 _p0, Vector3 _p1, Vector3 _p2, Vector3 _p3)
        {
            var p0 = _p0;
            var p1 = _p1;
            var p2 = _p2;
            var p3 = _p3;
            //Debug.Log("p0 " + p0);
            //Debug.Log("p3 " + p3);
            _homography = CalcHomographyMatrix(p1, p3, p2, p0);
            _invHomography = CalcInverseMatrix(_homography);
            _resultRendererMapMat.SetFloatArray("_InvHomography", _invHomography);
            _resultRendererMapMat.SetVectorArray("_VertexValues", new Vector4[] {
                new Vector4(p1.x, p1.y, 0.0f, 1.0f),
                new Vector4(p0.x, p0.y, 0.0f, 1.0f),
                new Vector4(p3.x, p3.y, 0.0f, 1.0f),
                new Vector4(p2.x, p2.y, 0.0f, 1.0f)
            });
            //Graphics.Blit(_mainMap, _resultMap, _resultRendererMapMat, 0);
            Graphics.Blit(null, _uvQuadRef, _resultRendererMapMat, 0);
        }

        void TranscribeUVDirectionFromMeshToWallResultMap(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3, Vector2 _p0, Vector2 _p1, Vector2 _p2, Vector2 _p3)
        {
            var p0 = _p0;
            var p1 = _p1;
            var p2 = _p2;
            var p3 = _p3;
            Vector2 vertex0 = new Vector2(_vertex0.y, _vertex0.z);
            Vector2 vertex1 = new Vector2(_vertex1.y, _vertex1.z);
            Vector2 vertex2 = new Vector2(_vertex2.y, _vertex2.z);
            Vector2 vertex3 = new Vector2(_vertex3.y, _vertex3.z);
            Debug.Log("vertex0 " + _vertex0.ToString("F4"));
            Debug.Log("vertex1 " + _vertex1.ToString("F4"));
            Debug.Log("vertex2 " + _vertex2.ToString("F4"));
            Debug.Log("vertex3 " + _vertex3.ToString("F4"));
            Debug.Log("p0 " + p0.ToString("F4"));
            Debug.Log("p1 " + p1.ToString("F4"));
            Debug.Log("p2 " + p2.ToString("F4"));
            Debug.Log("p3 " + p3.ToString("F4"));
            _homography = CalcHomographyMatrix(p0, p2, p1, p3);
            _homographyVertex = CalcHomographyMatrix(vertex0, vertex2, vertex1, vertex3);
            _invHomography = CalcInverseMatrix(_homography);
            _resultRendererMapMat.SetFloatArray("_Homography", _homography);
            _resultRendererMapMat.SetFloatArray("_InvHomography", _invHomography);
            _resultRendererMapMat.SetVectorArray("_VertexValues", new Vector4[] {
                new Vector4(vertex0.x, vertex0.y, 0.0f, 1.0f),
                new Vector4(vertex1.x, vertex1.y, 0.0f, 1.0f),
                new Vector4(vertex2.x, vertex2.y, 0.0f, 1.0f),
                new Vector4(vertex3.x, vertex3.y, 0.0f, 1.0f)
            });
            _resultRendererMapMat.SetVectorArray("_UVValues", new Vector4[] {
                new Vector4(p0.x, p0.y, 0.0f, 1.0f),
                new Vector4(p1.x, p1.y, 0.0f, 1.0f),
                new Vector4(p2.x, p2.y, 0.0f, 1.0f),
                new Vector4(p3.x, p3.y, 0.0f, 1.0f)
            });
            _resultRendererMapMat.SetTexture("_ParticleTex", _mainMap);
            Graphics.Blit(null, _uvQuadRef, _resultRendererMapMat, 0);
        }

        void TranscribeUVFromMeshToWall(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3, Vector2 _p0, Vector2 _p1, Vector2 _p2, Vector2 _p3) {
            var p0 = _p0;
            var p1 = _p1;
            var p2 = _p2;
            var p3 = _p3;
            Vector2 vertex0 = new Vector2(_vertex0.y, _vertex0.z);
            Vector2 vertex1 = new Vector2(_vertex1.y, _vertex1.z);
            Vector2 vertex2 = new Vector2(_vertex2.y, _vertex2.z);
            Vector2 vertex3 = new Vector2(_vertex3.y, _vertex3.z);
            //Debug.Log("vertex0 " + _vertex0.ToString("F4"));
            //Debug.Log("vertex1 " + _vertex1.ToString("F4"));
            //Debug.Log("vertex2 " + _vertex2.ToString("F4"));
            //Debug.Log("vertex3 " + _vertex3.ToString("F4"));
            //Debug.Log("p0 " + p0.ToString("F4"));
            //Debug.Log("p1 " + p1.ToString("F4"));
            //Debug.Log("p2 " + p2.ToString("F4"));
            //Debug.Log("p3 " + p3.ToString("F4"));
            Vector3[] source = new[] { new Vector3(_p0.x, _p0.y, 0), new Vector3(_p2.x, _p2.y, 0), new Vector3(_p1.x, _p1.y, 0), new Vector3(_p3.x, _p3.y, 0) };
            Vector3[] destination = new[] { new Vector3(_vertex0.y, _vertex0.z, 0), new Vector3(_vertex2.y, _vertex2.z, 0), new Vector3(_vertex1.y, _vertex1.z, 0), new Vector3(_vertex3.y, _vertex3.z, 0)};

            FindHomography(ref source, ref destination, ref _matrix);
            _inverseMatrix = CalcInverseMatrix(_matrix);
            _homography = CalcHomographyMatrix(p0, p2, p1, p3);
            _invHomography = CalcInverseMatrix(_homography);
            //float[] _homographyVertex = null;
            _homographyVertex = CalcHomographyMatrix(vertex0, vertex2, vertex1, vertex3);
            _invHomographyVertex = CalcInverseMatrix(_homographyVertex);

            _resultRendererMapMaterial.SetFloatArray("_Homography", _matrix);//_homographyVertex);//
            _resultRendererMapMaterial.SetFloatArray("_InvHomography", _inverseMatrix); //_invHomographyVertex);// 
            _resultRendererMapMaterial.SetFloatArray("_InvHomographyUV", _invHomography);

            _resultRendererMapMaterial.SetVectorArray("_VertexValues", new Vector4[] {
                new Vector4(vertex0.x, vertex0.y, 0.0f, 1.0f),
                new Vector4(vertex1.x, vertex1.y, 0.0f, 1.0f),
                new Vector4(vertex2.x, vertex2.y, 0.0f, 1.0f),
                new Vector4(vertex3.x, vertex3.y, 0.0f, 1.0f)
            });
            _resultRendererMapMaterial.SetVectorArray("_UVValues", new Vector4[] {
                new Vector4(p0.x, p0.y, 0.0f, 1.0f),
                new Vector4(p1.x, p1.y, 0.0f, 1.0f),
                new Vector4(p2.x, p2.y, 0.0f, 1.0f),
                new Vector4(p3.x, p3.y, 0.0f, 1.0f)
            });
            _resultRendererMapMaterial.SetTexture("_ParticleTex", _mainMap);
            Graphics.Blit(null, _uvQuadRef, _resultRendererMapMaterial, 0);
        }
        void TranscribeUVDirectionFromMeshToMap(Vector2 _p0, Vector2 _p1, Vector2 _p2, Vector2 _p3)
        {
            var p0 = _p0;
            var p1 = _p1;
            var p2 = _p2;
            var p3 = _p3;
            var n0 = normalize(p0 - p1);
            var n1 = normalize(p0 - p1);
            var n2 = normalize(p2 - p3);
            var n3 = normalize(p2 - p3);

            _homography = CalcHomographyMatrix(p0, p1, p2, p3);
            _invHomography = CalcInverseMatrix(_homography);
            _setQuadrangleDataMat.SetTexture("_CheckBoardTex", _checkboard);
            _setQuadrangleDataMat.SetFloatArray("_InvHomography", _invHomography);
            _setQuadrangleDataMat.SetVectorArray("_VertexValues", new Vector4[] {
                new Vector4(n0.x, n0.y, 0.0f, 1.0f),
                new Vector4(n2.x, n2.y, 0.0f, 1.0f),
                new Vector4(n1.x, n1.y, 0.0f, 1.0f),
                new Vector4(n3.x, n3.y, 0.0f, 1.0f)
            });
            Graphics.Blit(null, _gravityDirectionMap, _setQuadrangleDataMat, 0);
            Graphics.Blit(null, _gravitydirmap, _setQuadrangleDataMat, 0);
        }
        void TranscribeUVDirectionFromMeshToInteractionMap(Vector2 _p0, Vector2 _p1, Vector2 _p2, Vector2 _p3)
        {
            var p0 = _p0;
            var p1 = _p1;
            var p2 = _p2;
            var p3 = _p3;
            var n0 = normalize(p0 - p1);
            var n1 = normalize(p0 - p1);
            var n2 = normalize(p2 - p3);
            var n3 = normalize(p2 - p3);

            _homography = CalcHomographyMatrix(p0, p1, p2, p3);
            _invHomography = CalcInverseMatrix(_homography);
            _setQuadrangleDataMat.SetFloatArray("_InvHomography", _invHomography);
            _setQuadrangleDataMat.SetVectorArray("_VertexValues", new Vector4[] {
                new Vector4(n0.x, n0.y, 0.0f, 1.0f),
                new Vector4(n2.x, n2.y, 0.0f, 1.0f),
                new Vector4(n1.x, n1.y, 0.0f, 1.0f),
                new Vector4(n3.x, n3.y, 0.0f, 1.0f)
            });
            Graphics.Blit(null, _gravityInteractionMap, _setQuadrangleDataMat, 0);
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

       
    }
}
