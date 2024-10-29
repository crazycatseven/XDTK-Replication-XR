using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCollisionDetector : MonoBehaviour
{

    public LineRenderer rayLineRenderer;
    public Color hitColor = Color.red;

    public GameObject currentHitObject;

    private Gradient defaultGradient;

    public LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        rayLineRenderer = GetComponent<LineRenderer>();
        defaultGradient = rayLineRenderer.colorGradient;


    }

    // Update is called once per frame
    void Update()
    {
        // 获得lineRenderer的开始和结束位置
        Vector3 startPosition = rayLineRenderer.GetPosition(0);
        Vector3 endPosition = rayLineRenderer.GetPosition(1);

        // 发射射线
        RaycastHit hit;
        if (Physics.Raycast(startPosition, endPosition - startPosition, out hit, Mathf.Infinity, layerMask))
        {
            // 射线碰撞到物体
            Gradient gradient = new Gradient();

            GradientColorKey[] colorKeys = rayLineRenderer.colorGradient.colorKeys;
            GradientAlphaKey[] alphaKeys = rayLineRenderer.colorGradient.alphaKeys;

            // 更新颜色
            colorKeys[0].color = hitColor;
            alphaKeys[0].alpha = 1.0f;

            gradient.SetKeys(colorKeys, alphaKeys);

            rayLineRenderer.colorGradient = gradient;

            currentHitObject = hit.collider.gameObject;
        }
        else
        {
            rayLineRenderer.colorGradient = defaultGradient;
            currentHitObject = null;
        }
    }

    public GameObject GetCurrentHitObject()
    {
        return currentHitObject;
    }
}
