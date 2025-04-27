using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0f, 1f, -10f); 
    public float smoothSpeed = 0f; 

    public Vector2 minLimits; 
    public Vector2 maxLimits;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        float clampedX = Mathf.Clamp(desiredPosition.x, minLimits.x, maxLimits.x);
        float clampedY = Mathf.Clamp(desiredPosition.y, minLimits.y + 10f, maxLimits.y);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed * Time.deltaTime);
        transform.position = clampedPosition;
    }
}