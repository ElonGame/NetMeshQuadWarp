using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デバッグ 
/// </summary>
namespace Utilities.Debug
{
    /// <summary>
    /// GL描画
    /// </summary>
    public class GLDraw
    {
        /// <summary>
        /// 円の解像度
        /// </summary>
        const int CIRCLE_RESOLUTION = 32;

        /// <summary>
        /// 円グラフの円の解像度
        /// </summary>
        const int CIRCULAR_GRAPH_CIRCLE_RESOLUTION = 128;
        

        public static void Line(Vector3 start, Vector3 end, Color? color = null)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(start.x, start.y, start.z);
            GL.Vertex3(end.x,   end.y,   end.z  );
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// ワイヤーフレーム 円 を描画
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="radius">半径</param>
        /// <param name="color">色</param>
        public static void WireCircleXZ(Vector3 pos, float? radius = null, Color? color = null, int? resolution = null, float? rotation = null)
        {
            var res = resolution.HasValue ? resolution.Value : CIRCLE_RESOLUTION;
            var rot = rotation.HasValue ? rotation.Value : 0.0f;
            var rad = radius ?? 1.0f;
            var dAng = (Mathf.PI * 2.0f) / (float)res;

            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            for (var i = 0; i < res + 1; ++i)
            {
                float ang = dAng * i + rot;
                GL.Vertex3
                (
                    pos.x + rad * Mathf.Cos(ang),
                    pos.y,
                    pos.z + rad * Mathf.Sin(ang)
                );
            }
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// ワイヤーフレーム 円 を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void WireCircleXY(Vector3 pos, float? radius = null, Color? color = null, int? resolution = null, float? rotation = null)
        {
            var res = resolution.HasValue ? resolution.Value : CIRCLE_RESOLUTION;
            var rot = rotation.HasValue ? rotation.Value : 0.0f;
            var rad = radius ?? 1.0f;
            var dAng = (Mathf.PI * 2.0f) / (float)res;

            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            for (var i = 0; i < res + 1; ++i)
            {
                float ang = dAng * i + rot;
                GL.Vertex3
                (
                    pos.x + rad * Mathf.Cos(ang),
                    pos.y + rad * Mathf.Sin(ang),
                    pos.z
                );
            }
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// ワイヤーフレーム 円 を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void WireCircleYZ(Vector3 pos, float? radius = null, Color? color = null, int? resolution = null, float? rotation = null)
        {
            var res = resolution.HasValue ? resolution.Value : CIRCLE_RESOLUTION;
            var rot = rotation.HasValue ? rotation.Value : 0.0f;
            var rad = radius ?? 1.0f;
            var dAng = (Mathf.PI * 2.0f) / (float)res;

            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            for (var i = 0; i < res + 1; ++i)
            {
                float ang = dAng * i;
                GL.Vertex3
                (
                    pos.x,
                    pos.y + rad * Mathf.Cos(ang),
                    pos.z + rad * Mathf.Sin(ang)
                );
            }
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// ワイヤーフレーム 矩形 を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public static void WireRectXZ(Vector3 pos, float? width = null, float? height = null, Color? color = null)
        {
            var w = width ?? 1.0f;
            var h = height ?? 1.0f;

            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(pos.x - w * 0.5f, pos.y, pos.z - h * 0.5f);
            GL.Vertex3(pos.x + w * 0.5f, pos.y, pos.z - h * 0.5f);
            GL.Vertex3(pos.x + w * 0.5f, pos.y, pos.z + h * 0.5f);
            GL.Vertex3(pos.x - w * 0.5f, pos.y, pos.z + h * 0.5f);
            GL.Vertex3(pos.x - w * 0.5f, pos.y, pos.z - h * 0.5f);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// ワイヤーフレーム 矩形 を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public static void WireRectXY(Vector3 pos, float? width = null, float? height = null, Color? color = null)
        {
            var w = width ?? 1.0f;
            var h = height ?? 1.0f;

            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(pos.x - w * 0.5f, pos.y - h * 0.5f, pos.z);
            GL.Vertex3(pos.x + w * 0.5f, pos.y - h * 0.5f, pos.z);
            GL.Vertex3(pos.x + w * 0.5f, pos.y + h * 0.5f, pos.z);
            GL.Vertex3(pos.x - w * 0.5f, pos.y + h * 0.5f, pos.z);
            GL.Vertex3(pos.x - w * 0.5f, pos.y - h * 0.5f, pos.z);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// ワイヤーフレーム 矩形 を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public static void WireRectYZ(Vector3 pos, float? width = null, float? height = null, Color? color = null)
        {
            var w = width ?? 1.0f;
            var h = height ?? 1.0f;

            GL.PushMatrix();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(pos.x, pos.y - w * 0.5f, pos.z - h * 0.5f);
            GL.Vertex3(pos.x, pos.y + w * 0.5f, pos.z - h * 0.5f);
            GL.Vertex3(pos.x, pos.y + w * 0.5f, pos.z + h * 0.5f);
            GL.Vertex3(pos.x, pos.y - w * 0.5f, pos.z + h * 0.5f);
            GL.Vertex3(pos.x, pos.y - w * 0.5f, pos.z - h * 0.5f);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void CircleXZ(Vector3 pos, float? radius = null, Color? color = null)
        {
            var rad = radius ?? 1.0f;
            var dAng = (Mathf.PI * 2.0f) / (float)CIRCLE_RESOLUTION;

            GL.PushMatrix();
            GL.Begin(GL.TRIANGLES);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            for (var i = 0; i < CIRCLE_RESOLUTION + 1; ++i)
            {
                float ang0 = dAng * i;
                float ang1 = dAng * (i + 1);
                GL.Vertex3
                (
                    pos.x + rad * Mathf.Cos(ang0),
                    pos.y + 0.0f,
                    pos.z + rad * Mathf.Sin(ang0)
                );
                GL.Vertex3
                (
                   pos.x,
                   pos.y,
                   pos.z
                );
                GL.Vertex3
                (
                    pos.x + rad * Mathf.Cos(ang1),
                    pos.y + 0.0f,
                    pos.z + rad * Mathf.Sin(ang1)
                );
            }
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void CircleXY(Vector3 pos, float? radius = null, Color? color = null)
        {
            var rad = radius ?? 1.0f;
            var dAng = (Mathf.PI * 2.0f) / (float)CIRCLE_RESOLUTION;

            GL.PushMatrix();
            GL.Begin(GL.TRIANGLES);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            for (var i = 0; i < CIRCLE_RESOLUTION + 1; ++i)
            {
                float ang0 = dAng * i;
                float ang1 = dAng * (i + 1);
                GL.Vertex3
                (
                    pos.x + rad * Mathf.Cos(ang0),
                    pos.y + rad * Mathf.Sin(ang0),
                    pos.z
                );
                GL.Vertex3
                (
                    pos.x,
                    pos.y,
                    pos.z
                );
                GL.Vertex3
                (
                   pos.x + rad * Mathf.Cos(ang1),
                   pos.y + rad * Mathf.Sin(ang1),
                   pos.z
                );
            }
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void CircleYZ(Vector3 pos, float? radius = null, Color? color = null)
        {
            var rad = radius ?? 1.0f;
            var dAng = (Mathf.PI * 2.0f) / (float)CIRCLE_RESOLUTION;

            GL.PushMatrix();
            GL.Begin(GL.TRIANGLES);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            for (var i = 0; i < CIRCLE_RESOLUTION + 1; ++i)
            {
                float ang0 = dAng * i;
                float ang1 = dAng * (i + 1);
                GL.Vertex3
                (
                    pos.x,
                    pos.y + rad * Mathf.Cos(ang0),
                    pos.z + rad * Mathf.Sin(ang0)
                );
                GL.Vertex3
                (
                    pos.x,
                    pos.y,
                    pos.z
                );
                GL.Vertex3
                (
                   pos.x,
                   pos.y + rad * Mathf.Cos(ang1),
                   pos.z + rad * Mathf.Sin(ang1)
                );
            }
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 矩形を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public static void RectXZ(Vector3 pos, float? width = null, float? height = null, Color? color = null)
        {
            var w = width ?? 1.0f;
            var h = height ?? 1.0f;

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(pos.x - w * 0.5f, pos.y, pos.z - h * 0.5f);
            GL.Vertex3(pos.x - w * 0.5f, pos.y, pos.z + h * 0.5f);
            GL.Vertex3(pos.x + w * 0.5f, pos.y, pos.z + h * 0.5f);
            GL.Vertex3(pos.x + w * 0.5f, pos.y, pos.z - h * 0.5f);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 矩形を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public static void RectXY(Vector3 pos, float? width = null, float? height = null, Color? color = null)
        {
            var w = width ?? 1.0f;
            var h = height ?? 1.0f;

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(pos.x - w * 0.5f, pos.y - h * 0.5f, pos.z);
            GL.Vertex3(pos.x - w * 0.5f, pos.y + h * 0.5f, pos.z);
            GL.Vertex3(pos.x + w * 0.5f, pos.y + h * 0.5f, pos.z);
            GL.Vertex3(pos.x + w * 0.5f, pos.y - h * 0.5f, pos.z);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 矩形を描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public static void RectYZ(Vector3 pos, float? width = null, float? height = null, Color? color = null)
        {
            var w = width ?? 1.0f;
            var h = height ?? 1.0f;

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Vertex3(pos.x, pos.y - w * 0.5f, pos.z - h * 0.5f);
            GL.Vertex3(pos.x, pos.y - w * 0.5f, pos.z + h * 0.5f);
            GL.Vertex3(pos.x, pos.y + w * 0.5f, pos.z + h * 0.5f);
            GL.Vertex3(pos.x, pos.y + w * 0.5f, pos.z - h * 0.5f);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// 円グラフを描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="angle"></param>
        /// <param name="outerRadius"></param>
        /// <param name="innerRadius"></param>
        /// <param name="color"></param>
        public static void CircularGraphXZ(Vector3 pos, float value, float? outerRadius = null, float? innerRadius = null, Color? color = null)
        {
            var or = outerRadius ?? 1.0f;
            var ir = innerRadius ?? 0.0f;

            var dAng = (Mathf.PI * 2.0f) / (float)CIRCULAR_GRAPH_CIRCLE_RESOLUTION;

            var num = Mathf.FloorToInt((float)CIRCULAR_GRAPH_CIRCLE_RESOLUTION * Mathf.Clamp01(value));

            GL.PushMatrix();
           
            for (var i = 0; i < num; ++i)
            {

                float ang0 = Mathf.PI * 0.5f - dAng * i;
                float ang1 = Mathf.PI * 0.5f - dAng * (i + 1);

                GL.Begin(GL.QUADS);
                GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
                // 0
                GL.Vertex3
                (
                    pos.x + ir * Mathf.Cos(ang0),
                    pos.y,
                    pos.z + ir * Mathf.Sin(ang0)
                );
                // 1
                GL.Vertex3
                (
                    pos.x + or * Mathf.Cos(ang0),
                    pos.y,
                    pos.z + or * Mathf.Sin(ang0)
                );
                // 2
                GL.Vertex3
                (
                    pos.x + or * Mathf.Cos(ang1),
                    pos.y,
                    pos.z + or * Mathf.Sin(ang1)
                );
                // 3
                GL.Vertex3
                (
                    pos.x + ir * Mathf.Cos(ang1),
                    pos.y,
                    pos.z + ir * Mathf.Sin(ang1)
                );
                GL.End();
            }
           
            GL.PopMatrix();
        }

        /// <summary>
        /// 円グラフを描画
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="angle"></param>
        /// <param name="outerRadius"></param>
        /// <param name="innerRadius"></param>
        /// <param name="color"></param>
        public static void CircularGraphXY(Vector3 pos, float value, float? outerRadius = null, float? innerRadius = null, Color? color = null)
        {
            var or = outerRadius ?? 1.0f;
            var ir = innerRadius ?? 0.0f;

            var dAng = (Mathf.PI * 2.0f) / (float)CIRCULAR_GRAPH_CIRCLE_RESOLUTION;

            var num = Mathf.FloorToInt((float)CIRCULAR_GRAPH_CIRCLE_RESOLUTION * Mathf.Clamp01(value));

            GL.PushMatrix();

            for (var i = 0; i < num; ++i)
            {

                float ang0 = Mathf.PI * 0.5f - dAng * i;
                float ang1 = Mathf.PI * 0.5f - dAng * (i + 1);

                GL.Begin(GL.QUADS);
                GL.Color(color.HasValue ? color.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f));
                // 0
                GL.Vertex3
                (
                    pos.x + ir * Mathf.Cos(ang0),
                    pos.y + ir * Mathf.Sin(ang0),
                    pos.z
                );
                // 1
                GL.Vertex3
                (
                    pos.x + or * Mathf.Cos(ang0),
                    pos.y + or * Mathf.Sin(ang0),
                    pos.z
                );
                // 2
                GL.Vertex3
                (
                    pos.x + or * Mathf.Cos(ang1),
                    pos.y + or * Mathf.Sin(ang1),
                    pos.z 
                );
                // 3
                GL.Vertex3
                (
                    pos.x + ir * Mathf.Cos(ang1),
                    pos.y + ir * Mathf.Sin(ang1),
                    pos.z
                );
                GL.End();
            }

            GL.PopMatrix();
        }
    }
}