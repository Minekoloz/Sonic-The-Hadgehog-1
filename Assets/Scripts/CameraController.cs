using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new(0f, 1f, -10f);
    public Vector2 minLimits, maxLimits;
    public float smoothSpeed = 5f;

    private float verticalOffsetTarget = 0f;
    private float currentVerticalOffset = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        currentVerticalOffset = Mathf.Lerp(currentVerticalOffset, verticalOffsetTarget, smoothSpeed * Time.deltaTime);

        Vector3 targetPosition = target.position + new Vector3(offset.x, offset.y + currentVerticalOffset, offset.z);

        float x = Mathf.Clamp(targetPosition.x, minLimits.x, maxLimits.x);
        float y = Mathf.Clamp(targetPosition.y, minLimits.y, maxLimits.y);

        transform.position = new Vector3(x, y, targetPosition.z);
    }

    public void SetVerticalOffset(float offset) => verticalOffsetTarget = offset;
    public void ResetVerticalOffset() => verticalOffsetTarget = 0f;
}
