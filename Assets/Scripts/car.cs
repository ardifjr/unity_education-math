using UnityEngine;

public class Car : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Vector3 originalPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalPosition = transform.position;
    }

    void Update()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        // Store current position
        Vector3 currentPosition = transform.position;

        // Horizontal movement (left-right)
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
            transform.rotation = Quaternion.Euler(0, -10, 0);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
            transform.rotation = Quaternion.Euler(0, 10, 0);
        }

        // Vertical movement (forward-backward)
        if (Input.GetKey(KeyCode.UpArrow))
        {
            verticalInput = 1f;
            transform.rotation = Quaternion.Euler(-10, transform.rotation.eulerAngles.y, 0);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalInput = -1f;
            transform.rotation = Quaternion.Euler(10, transform.rotation.eulerAngles.y, 0);
        }

        // Calculate movement in world space
        Vector3 movement = new Vector3(horizontalInput * speed * Time.deltaTime, 0, verticalInput * speed * Time.deltaTime);
        
        // Apply movement while keeping Y position constant
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            rb.MovePosition(new Vector3(
                currentPosition.x + movement.x, 
                currentPosition.y,  // Keep Y position the same
                currentPosition.z + movement.z
            ));
        }
        else
        {
            // Reset rotation when not moving
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}