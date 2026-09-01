using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Editor 模式自动设置：禁用 AR 组件，调整相机，添加地面参考
/// 挂到任意 GameObject 上即可，只在 Editor 里生效
/// </summary>
public class EditorModeSetup : MonoBehaviour
{
    [Header("Editor 模式（真机构建时自动忽略）")]
    [SerializeField] private bool enableEditorMode = true;

    [Header("相机设置")]
    [SerializeField] private Vector3 cameraPosition = new(0, 2, -3);
    [SerializeField] private Vector3 cameraRotation = new(30, 0, 0);

    void Awake()
    {
#if !UNITY_EDITOR
        // 真机不执行
        return;
#else
        if (!enableEditorMode) return;

        // 1. 禁用 AR Session
        var session = FindObjectOfType<ARSession>();
        if (session != null)
        {
            session.enabled = false;
            Debug.Log("[EditorMode] AR Session 已禁用");
        }

        // 2. 禁用 AR Plane Manager
        var planeManager = FindObjectOfType<ARPlaneManager>();
        if (planeManager != null)
        {
            planeManager.enabled = false;
            Debug.Log("[EditorMode] AR Plane Manager 已禁用");
        }

        // 3. 调整主相机：位置、朝向、清除模式
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = cameraPosition;
            cam.transform.rotation = Quaternion.Euler(cameraRotation);
            cam.clearFlags = CameraClearFlags.Skybox;
            Debug.Log("[EditorMode] 相机已调整");
        }

        // 4. 创建地面参考平面（灰色半透明，方便看到放置位置）
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "EditorGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2, 1, 2); // 20x20 单位
        var groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.material = new Material(Shader.Find("Standard"))
        {
            color = new Color(0.5f, 0.5f, 0.5f, 0.5f)
        };
        groundRenderer.material.SetFloat("_Mode", 3); // Transparent mode
        groundRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        groundRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        groundRenderer.material.SetInt("_ZWrite", 0);
        groundRenderer.material.DisableKeyword("_ALPHATEST_ON");
        groundRenderer.material.EnableKeyword("_ALPHABLEND_ON");
        groundRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        groundRenderer.material.renderQueue = 3000;

        Debug.Log("[EditorMode] 地面参考平面已创建");
        Debug.Log("[EditorMode] 设置完成！鼠标左键点击 Game 窗口放置标记");
#endif
    }
}
