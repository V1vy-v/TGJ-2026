using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostProcessingScanOrigin : MonoBehaviour
{
    public Material material;

    void Start()
    {

    }

    void LateUpdate()
    {
        // 材质由 ScanRendererFeature 指定，这里只是可选的“跟随物体”写法。
        if (material != null)
            material.SetVector("_Origin", transform.position);
    }
}
