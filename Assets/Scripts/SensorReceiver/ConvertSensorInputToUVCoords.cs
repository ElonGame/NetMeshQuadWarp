using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace NetValley
{
    [ExecuteAlways]
    public class ConvertSensorInputToUVCoords : MonoBehaviour
    {
        #region Properties
        [System.Serializable]
        public class PointData
        {
            public float3 vertex;
            public float3 vertexRaw;
            public float2 uv;
            public int    index;

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
        GameObject _debugQuadRef = null;

        [Header("Parameters")]  
        public Bounds FloorArea;
        public Bounds WallLeftArea;
        public Bounds WallRightArea;

        public float OutputWorldSpaceXMin =  0.0f;
        public float OutputWorldSpaceXMax = 10.0f;
        public float OutputWorldSpaceYMin =  0.0f;
        public float OutputWorldSpaceYMax = 10.0f;

        [Header("Debug")]
        public float2 SensorFloor;
        public float2 SensorWallLeft;
        public float2 SensorWallRight;

        public bool   ShowDebugQuadForDrawUV = false;
        public float  DebugQuadPositionZ = 10.0f;

        [Header("Private Variables")]
        [SerializeField, Disable]
        float2 _uvFloor;
        [SerializeField, Disable]
        float2 _uvWallLeft;
        [SerializeField, Disable]
        float2 _uvWallRight;

        [SerializeField, Disable]
        float4 _uvFloorMinMax; // xmin, xmax, ymin, ymax

        [SerializeField, Disable]
        List<PointData> _pointFloorList     = new List<PointData>();

        [SerializeField, Disable]
        List<PointData> _pointWallLeftList  = new List<PointData>();
        
        [SerializeField, Disable]
        List<PointData> _pointWallRightList = new List<PointData>();

        List<PointData> _pointWallLeftOrgList  = new List<PointData>();
        List<PointData> _pointWallRightOrgList = new List<PointData>();

        public List<PointData> PointWallLeftList  => _pointWallLeftList  ?? null;
        public List<PointData> PointWallRightList => _pointWallRightList ?? null;
        public GameObject      StageGo            => _stageGoRef         ?? null;
        #endregion

        #region MonoBehaviour Functions
        void Awake()
        {
            FixTransform();
            SetupToCalculation();
        }

        void Update()
        {
            FixTransform();
            
            if (ShowDebugQuadForDrawUV)
                CalculateDebugUVValue();
        }
        
        void OnDrawGizmos()
        {
            DrawDebugGizmosObject();
        }

        private void OnDestroy()
        {
            if (_pointFloorList != null)
            {
                _pointFloorList.Clear();
                _pointFloorList = null;
            }

            if (_pointWallLeftList != null)
            {
                _pointWallLeftList.Clear();
                _pointWallLeftList = null;
            }

            if (_pointWallRightList != null)
            {
                _pointWallRightList.Clear();
                _pointWallRightList = null;
            }

            if (_pointWallLeftOrgList != null)
            {
                _pointWallLeftOrgList.Clear();
                _pointWallLeftOrgList = null;
            }

            if (_pointWallRightOrgList != null)
            {
                _pointWallRightOrgList.Clear();
                _pointWallRightOrgList = null;
            }
        }
        #endregion

        #region Private Functions
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

            if (_debugQuadRef != null)
            {
                _debugQuadRef.transform.position   = float3((OutputWorldSpaceXMin + OutputWorldSpaceXMax) * 0.5f, (OutputWorldSpaceYMin + OutputWorldSpaceYMax) * 0.5f, DebugQuadPositionZ);
                _debugQuadRef.transform.localScale = float3(abs(OutputWorldSpaceXMax - OutputWorldSpaceXMin), abs(OutputWorldSpaceYMax - OutputWorldSpaceYMin), 1.0f);

                _debugQuadRef.SetActive(ShowDebugQuadForDrawUV);                
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
                SetPointsFromMeshToList(ref _pointWallLeftList,    WallLeftArea, false);
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
                SetPointsFromMeshToList(ref _pointWallRightList,    WallRightArea, false);
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

        void CalculateDebugUVValue()
        {
            // clamp
            SensorFloor     = clamp(SensorFloor,     0.0f, 1.0f);
            SensorWallLeft  = clamp(SensorWallLeft,  0.0f, 1.0f);
            SensorWallRight = clamp(SensorWallRight, 0.0f, 1.0f);
            
            _uvFloor     = GetFloorUV(SensorFloor);
            _uvWallLeft  = GetWallLeftUV(SensorWallLeft);
            _uvWallRight = GetWallRightUV(SensorWallRight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensorInputVSF2XY"></param>
        /// <returns></returns>
        public float2 GetFloorUV(float2 sensorInputVSF2XY, bool? enableWorldSpaceScaling = null)
        {
            sensorInputVSF2XY = clamp(sensorInputVSF2XY, 0.0f, 1.0f);

            var uv = float2(
                lerp(_uvFloorMinMax.x, _uvFloorMinMax.y, sensorInputVSF2XY.x),
                lerp(_uvFloorMinMax.z, _uvFloorMinMax.w, sensorInputVSF2XY.y)
            );
            //Debug.Log("Flooryy uv " + uv);

            return ReturnUV(uv, enableWorldSpaceScaling.HasValue ? enableWorldSpaceScaling.Value : true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensorInputVSF2XY"></param>
        /// <returns></returns>
        public float2 GetWallLeftUV(float2 sensorInputVSF2XY, bool? enableWorldSpaceScaling = null)
        {
            sensorInputVSF2XY = clamp(sensorInputVSF2XY, 0.0f, 1.0f);
            
            var uv = CalculateWallUV(sensorInputVSF2XY, _pointWallLeftList);
            //Debug.Log("LeftWall uv " + uv);

            return ReturnUV(uv, enableWorldSpaceScaling.HasValue ? enableWorldSpaceScaling.Value : true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensorInputVSF2XY"></param>
        /// <returns></returns>
        public float2 GetWallRightUV(float2 sensorInputVSF2XY, bool? enableWorldSpaceScaling = null)
        {
            sensorInputVSF2XY = clamp(sensorInputVSF2XY, 0.0f, 1.0f);
            
            var uv = CalculateWallUV(sensorInputVSF2XY, _pointWallRightList);
            //Debug.Log("RightWall uv " + uv);
            return ReturnUV(uv, enableWorldSpaceScaling.HasValue ? enableWorldSpaceScaling.Value : true);
        }

        float2 ReturnUV(float2 uv, bool enableScaling)
        {
            if (enableScaling)
            {
                //Debug.Log("uv " + float2(
                //    FitAndClip(uv.x, 0.0f, 1.0f, OutputWorldSpaceXMin, OutputWorldSpaceXMax),
                //    FitAndClip(uv.y, 0.0f, 1.0f, OutputWorldSpaceYMin, OutputWorldSpaceYMax)
                //));

                return float2(
                    FitAndClip(uv.x, 0.0f, 1.0f, OutputWorldSpaceXMin, OutputWorldSpaceXMax),
                    FitAndClip(uv.y, 0.0f, 1.0f, OutputWorldSpaceYMin, OutputWorldSpaceYMax)
                );
            }
            else
            {
                return float2(uv.x, uv.y);
            }
        }

        float2 CalculateWallUV(float2 sensorInputVSF2XY, List<PointData> pointList)
        {
            if (pointList != null && pointList.Count >= 4)
            {
                var idx = 0;
                for (var i = 0; i < pointList.Count; i += 2)
                {
                    if (sensorInputVSF2XY.x > pointList[i].vertex.z)
                        idx = i;
                    else
                        break;                    
                }
                
                var v0 = pointList[idx];
                var v1 = pointList[idx + 1];
                var v2 = pointList[idx + 2];
                var v3 = pointList[idx + 3];

                var xAmp = (float)(sensorInputVSF2XY.x - v0.vertex.z) / max(0.001f, abs(v2.vertex.z - v0.vertex.z));
                
                var yOff = lerp(v0.vertex.y, v2.vertex.y, xAmp);
                var yAmp = (float)max(0.0f, (sensorInputVSF2XY.y - yOff)) * (1.0f / (1.0f - yOff));
                
                var v02_uv_xy = lerp(v0.uv.xy, v2.uv.xy, xAmp);
                var v13_uv_xy = lerp(v1.uv.xy, v3.uv.xy, xAmp);

                var v0123_uv_xy = lerp(v02_uv_xy, v13_uv_xy, yAmp);
                return v0123_uv_xy;
            }
            return float2(0, 0);
        }

        /// <summary>
        /// isFloorOrWall true:Floor, false:Wall
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="area"></param>
        /// <param name="isFloorOrWall">true:Floor, false:Wall</param>
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
                        pointList.Add(new PointData(posf3, posf3,mesh.uv[i], i));
                        if (area == WallRightArea) {
                            //Debug.Log("WallRightArea " + posf3 + ". uv " + mesh.uv[i].ToString("F4"));
                        }
                    }
                }
            }
        }

        void SortWallPointsByPositionYZ(ref List<PointData> pointList)
        {
            var sortedPointList = pointList.OrderBy(v => v.vertex.z)
                                           .ThenBy (v => v.vertex.y);
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
        #endregion

        #region Helper Functions

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

        #endregion
    }
}