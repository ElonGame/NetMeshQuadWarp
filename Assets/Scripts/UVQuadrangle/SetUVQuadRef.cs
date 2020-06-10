using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetUVQuadRef : MonoBehaviour
{
    [SerializeField]
    RenderTexture uvQuadRef= null;

    [SerializeField]
    RenderTexture FinalRendeTex = null;

    [SerializeField]
    Texture2D FinalTex2D = null;
    public Material uvQuadMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        uvQuadMat.SetTexture("_UvQuadRefTex", uvQuadRef);
        uvQuadMat.SetTexture("_FinalRenderTex", FinalTex2D);
    }
}
