using UnityEngine;

public class CarRotator : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0f, 20f, 0f); // Putar di sumbu Y

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
