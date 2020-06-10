using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FBOUtility
{
    public static void CreateBuffer(ref RenderTexture rt, int w, int h, RenderTextureFormat format, FilterMode filter)
    {
        rt = new RenderTexture(w, h, 0, format, RenderTextureReadWrite.Linear);
        rt.filterMode = filter;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.Create();
    }

    public static void CreateBuffer(ref RenderTexture[] rt, int w, int h, RenderTextureFormat format, FilterMode filter)
    {
        rt = new RenderTexture[2];
        for (var i = 0; i < rt.Length; i++)
        {
            rt[i] = new RenderTexture(w, h, 0, format, RenderTextureReadWrite.Linear);
            rt[i].filterMode = filter;
            rt[i].wrapMode = TextureWrapMode.Clamp;
            rt[i].Create();
        }
    }

    public static void DeleteBuffer(RenderTexture rt)
    {
        if (rt != null)
        {
            if (Application.isEditor)
                RenderTexture.DestroyImmediate(rt);
            else
                RenderTexture.Destroy(rt);
            rt = null;
        }
    }

    public static void DeleteBuffer(RenderTexture[] rt)
    {
        if (rt != null)
        {
            for (var i = 0; i < rt.Length; i++)
            {
                if (rt[i] != null)
                {
                    if (Application.isEditor)
                        RenderTexture.DestroyImmediate(rt[i]);
                    else
                        RenderTexture.Destroy(rt[i]);
                    rt[i] = null;
                }
            }
        }
    }

    public static void ClearBuffer(RenderTexture rt, Color? clearColor = null)
    {
        Color c = clearColor.HasValue ? clearColor.Value : new Color(0, 0, 0, 0);

        RenderTexture temp = RenderTexture.active;
        Graphics.SetRenderTarget(rt);
        GL.Clear(false, true, c);
        Graphics.SetRenderTarget(temp);
    }

    public static void Swap(RenderTexture[] rt)
    {
        RenderTexture temp = rt[0];
        rt[0] = rt[1];
        rt[1] = temp;
    }

    public static void CreateMaterial(ref Material material, Shader shader)
    {
        if (material == null)
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
        }
    }

    public static void DeleteMaterial(Material material)
    {
        if (material != null)
        {
            if (Application.isEditor)
                Material.DestroyImmediate(material);
            else
                Material.Destroy(material);
            material = null;
        }
    }
}
