using UnityEngine;

public class Spinner : MonoBehaviour
{
    [Header("Spin Settings")]
    public Vector3 axis = Vector3.up;
    public float degreesPerSecond = 90f;
    public Space space = Space.World;

    void Update()
    {
        if (degreesPerSecond == 0f)
        {
            return;
        }
        if (axis.sqrMagnitude < 0.000001f)
        {
            return;
        }
        transform.Rotate(axis.normalized, degreesPerSecond * Time.deltaTime, space);
    }
}


