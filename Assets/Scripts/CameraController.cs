using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    public Vector2 minLimits;
    public Vector2 maxLimits;

    private float verticalOffsetTarget = 0f;
    private float currentVerticalOffset = 0f;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        currentVerticalOffset = Mathf.Lerp(currentVerticalOffset, verticalOffsetTarget, smoothSpeed * Time.deltaTime);
        Vector3 desiredPosition = target.position + new Vector3(offset.x, offset.y + currentVerticalOffset, offset.z);

        float clampedX = Mathf.Clamp(desiredPosition.x, minLimits.x, maxLimits.x);
        float clampedY = Mathf.Clamp(desiredPosition.y, minLimits.y, maxLimits.y);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
        transform.position = clampedPosition;
    }

    public void SetVerticalOffset(float offset)
    {
        verticalOffsetTarget = offset;
    }

    public void ResetVerticalOffset()
    {
        verticalOffsetTarget = 0f;
    }
}
