using UnityEngine;

public class FakeMoonFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0, 100, 500);

    void LateUpdate()
    {
        transform.position = cameraTransform.position + offset;
    }
}
