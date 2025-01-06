using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedDisplay : MonoBehaviour
{
    public Rigidbody rb; // 关联角色的刚体
    public TextMeshProUGUI speedText; // 关联 UI 文本组件
    public TextMeshProUGUI stateText;

    public PlayerMovement playerMovement;
    void Start()
    {
        if (playerMovement == null)
            Debug.LogError("PlayerMovement is not assigned!");
    }

    void Update()
    {
        if (speedText == null || stateText == null)
        {
            Debug.LogError("SpeedText or StateText is not assigned!");
            return; 
        }

        // 获取角色的速度大小
        float speed = rb.velocity.magnitude;

        // 显示速度
        if (speed == 0f)
        {
            speedText.text = "Speed: 0";
        }
        else
        {
            speedText.text = $"Speed: {speed:F2}";
        }

        // 显示玩家当前状态
        stateText.text = $"State: {playerMovement.state}";
    }
}
