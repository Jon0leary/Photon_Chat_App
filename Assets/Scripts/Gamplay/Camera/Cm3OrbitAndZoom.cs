// Cm3OrbitAndZoom.cs  (Cinemachine 3.x)
using Unity.Cinemachine;
using UnityEngine;

public class Cm3OrbitAndZoom : MonoBehaviour
{
    [SerializeField] CinemachineCamera vcam;                       // the CinemachineCamera on this GO
    [SerializeField] CinemachineInputAxisController inputCtrl;     // also on this GO

    [Header("Zoom (Field Of View)")]
    public float minFov = 25f;
    public float maxFov = 60f;
    public float zoomFactor = 60f;   // wheel sensitivity

    void Awake()
    {
        if (!vcam) vcam = GetComponent<CinemachineCamera>();
        if (!inputCtrl) inputCtrl = GetComponent<CinemachineInputAxisController>();
        if (inputCtrl) inputCtrl.enabled = false; // only orbit while RMB held
    }

    void Update()
    {
        // Right mouse to orbit (and ignore while typing in chat)
        bool allowOrbit = Input.GetMouseButton(1) && !ChatManager.IsTyping;
        if (inputCtrl && inputCtrl.enabled != allowOrbit)
            inputCtrl.enabled = allowOrbit;

        // Scroll to zoom (FOV)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f && vcam != null)
        {
            var lens = vcam.Lens;
            lens.FieldOfView = Mathf.Clamp(lens.FieldOfView - scroll * zoomFactor, minFov, maxFov);
            vcam.Lens = lens;
        }
    }
}
