using UnityEngine;

public class MouseLook : MonoBehaviour
{
    /// <summary>
    /// 鼠标X轴灵敏度
    /// </summary>
    public float mouseXSensitivity = 100f;
    /// <summary>
    /// 玩家身体
    /// </summary>
    public Transform playerBody;
    /// <summary>
    /// 相机上下旋转角度
    /// </summary>
    private float xRotation = 0f;
    /// <summary>
    /// 是否暂停
    /// </summary>
    public bool IsPause;

    void Start()
    {
        // 游戏一开始隐藏并锁定鼠标
        // Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        if (IsPause) return; // 如果打开了 UI 界面，就停止转视角

        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseXSensitivity * Time.deltaTime;

        // 过滤异常数值
        if (mouseY < -150) return;

        // 计算上下旋转并限制角度 (-90到90度)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 应用相机的上下旋转
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // 应用玩家身体的左右旋转
        playerBody.Rotate(Vector3.up * mouseX);
    }
}