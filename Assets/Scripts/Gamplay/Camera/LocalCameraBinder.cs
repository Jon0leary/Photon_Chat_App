// LocalCameraBinder.cs  — Cinemachine 3.x
using Unity.Cinemachine;
using UnityEngine;

public class LocalCameraBinder : MonoBehaviour
{
    [Header("Assign your Cinemachine vcam (FreeLook) here")]
    [SerializeField] CinemachineVirtualCameraBase vcam;

    [Header("Optional")]
    [SerializeField] int forcePriority = 20; // make sure this vcam wins

    void Awake()
    {
#if UNITY_2022_2_OR_NEWER
        if (!vcam) vcam = Object.FindFirstObjectByType<CinemachineVirtualCameraBase>();
#else
        if (!vcam) vcam = FindObjectOfType<CinemachineVirtualCameraBase>();
#endif
        if (!vcam)
        {
            Debug.LogError("[CameraBinder] No Cinemachine vcam found in scene.");
            return;
        }

        vcam.Priority = forcePriority; // beat any default vcams

        // Ensure the Main Camera has a Brain
        var mainCam = Camera.main;
        if (mainCam && !mainCam.TryGetComponent<CinemachineBrain>(out _))
            mainCam.gameObject.AddComponent<CinemachineBrain>();
    }

    /// Bind the vcam to this pawn (Follow + LookAt)
    public void Bind(Transform pawnRoot)
    {
        if (!vcam || !pawnRoot) return;
        vcam.Follow = pawnRoot;
        vcam.LookAt = pawnRoot;
        Debug.Log($"[CameraBinder] Bound '{vcam.name}' to '{pawnRoot.name}', Priority={vcam.Priority}");
    }
}
