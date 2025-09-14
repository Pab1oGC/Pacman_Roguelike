using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [SerializeField] private float smoothSpeed = 5f; // velocidad de suavizado
    [SerializeField] private float height = 10f; // altura de la cámara desde arriba
    private Vector3 targetPosition;

    private void Awake()
    {
        Instance = this;
        targetPosition = transform.position;
    }

    private void LateUpdate()
    {
        // Mantener la cámara en altura fija y mover suavemente
        Vector3 desiredPos = new Vector3(targetPosition.x, height, targetPosition.z);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
    }

    public void SetTargetRoom(Vector3 roomCenter)
    {
        targetPosition = roomCenter;
    }
}
