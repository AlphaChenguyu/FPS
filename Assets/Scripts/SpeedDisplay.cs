using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedDisplay : MonoBehaviour
{
    public Rigidbody rb; // ������ɫ�ĸ���
    public TextMeshProUGUI speedText; // ���� UI �ı����
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

        // ��ȡ��ɫ���ٶȴ�С
        float speed = rb.velocity.magnitude;

        // ��ʾ�ٶ�
        if (speed == 0f)
        {
            speedText.text = "Speed: 0";
        }
        else
        {
            speedText.text = $"Speed: {speed:F2}";
        }

        // ��ʾ��ҵ�ǰ״̬
        stateText.text = $"State: {playerMovement.state}";
    }
}
