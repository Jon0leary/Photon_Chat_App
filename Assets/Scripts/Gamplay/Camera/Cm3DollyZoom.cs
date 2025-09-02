// Cm3DollyZoom.cs
using Unity.Cinemachine;
using UnityEngine;

public class Cm3DollyZoom : MonoBehaviour
{
    [SerializeField] CinemachineOrbitalFollow orbital;  // your Orbital Follow component
    [SerializeField] float minRadius = 2f;
    [SerializeField] float maxRadius = 10f;
    [SerializeField] float zoomSpeed = 5f;

    void Awake()
    {
        if (!orbital) orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (!orbital) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            orbital.Radius = Mathf.Clamp(
                orbital.Radius - scroll * zoomSpeed,
                minRadius,
                maxRadius
            );
        }
    }
}
