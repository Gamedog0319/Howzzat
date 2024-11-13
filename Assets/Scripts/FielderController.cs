using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FielderController : MonoBehaviour
{
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    public float fielderSpeed = 8f; // Increased speed
    public float rotationSpeed = 5f; // Added rotation speed for flexibility
    public Vector3 initialPosition; // Store initial position
    private Quaternion initialRotation;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position; // Store initial position
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            Vector3 targetPosition = other.transform.position;

            // Move towards the ball smoothly
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, fielderSpeed * Time.deltaTime));

            // Rotate towards the ball smoothly for better flexibility
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Method to reset the fielder's position
    public void ResetPosition()
    {
        transform.position = initialPosition; // Reset to initial position
        transform.rotation = initialRotation;
        rb.velocity = Vector3.zero; // Stop any movement
    }
}
