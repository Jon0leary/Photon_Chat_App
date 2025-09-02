// Cm3OrbitGate.cs
using Unity.Cinemachine;
using UnityEngine;

public class Cm3OrbitGate : MonoBehaviour
{
    [SerializeField] CinemachineInputAxisController inputCtrl;

    void Awake()
    {
        if (!inputCtrl) inputCtrl = GetComponent<CinemachineInputAxisController>();
        if (inputCtrl) inputCtrl.enabled = false;
    }

    void Update()
    {
        if (!inputCtrl) return;
        bool allow = Input.GetMouseButton(1) && !ChatManager.IsTyping;
        if (inputCtrl.enabled != allow) inputCtrl.enabled = allow;
    }
}
