using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;  // Reference to the UDP receive script
    public GameObject bat;         // Reference to your bat model
    private Vector3 initialPosition; // Store the initial position of the bat for relative movement
    private Vector3 smoothedPosition; // Smoothed bat position
    private Quaternion smoothedRotation; // Smoothed bat rotation

    public float movementScaleX = 0.02f; // Scale for x-axis movement sensitivity
    public float movementScaleY = 0.02f; // Scale for y-axis movement sensitivity
    public float movementScaleZ = 0.02f; // Scale for z-axis movement sensitivity
    public float smoothFactorPosition = 0.1f; // Smoothing factor for position
    public float smoothFactorRotation = 0.2f; // Slight smoothing for rotation
    public float deadzoneThreshold = 0.05f; // Deadzone threshold to ignore small movements

    public Vector3 batOffset = new Vector3(0, 1.0f, 0);  // Offset to align bat's handle with your hand, raised higher

    void Start()
    {
        // Adjust the initial position to be higher than the default
        initialPosition = bat.transform.localPosition + new Vector3(0, 2.0f, 0); // Raise initial position by 2 units on the Y-axis
        smoothedPosition = initialPosition; // Initialize smoothed position
        smoothedRotation = bat.transform.rotation; // Initialize smoothed rotation
    }

    void Update()
    {
        string data = udpReceive.data;

        if (string.IsNullOrEmpty(data))
            return; // Exit if no data received

        data = data.Trim(new char[] { '[', ']' });  // Remove '[' and ']' from the data string
        string[] points = data.Split(',');

        if (points.Length >= 63) // Ensure there are enough points (21 landmarks * 3 coordinates)
        {
            // Get x, y, z coordinates of the index finger landmark (adjusted to match bat movement)
            float x = 7 - float.Parse(points[0]) * movementScaleX;
            float y = -float.Parse(points[1]) * movementScaleY + 1.0f;  // Inverted Y-axis and raised by 1 unit
            float z = float.Parse(points[2]) * movementScaleZ;

            // Apply deadzone to ignore small movements
            if (Mathf.Abs(x) < deadzoneThreshold) x = 0;
            if (Mathf.Abs(y) < deadzoneThreshold) y = 0;
            if (Mathf.Abs(z) < deadzoneThreshold) z = 0;

            // Target position based on hand movement and bat offset
            Vector3 targetPosition = initialPosition + new Vector3(x, y, z) + batOffset;

            // Smooth bat movement
            smoothedPosition = Vector3.Lerp(smoothedPosition, targetPosition, smoothFactorPosition);
            bat.transform.localPosition = smoothedPosition;

            // Calculate rotation based on hand landmarks
            Vector3 direction = new Vector3(
                float.Parse(points[3]) - float.Parse(points[0]),
                float.Parse(points[4]) - float.Parse(points[1]),
                float.Parse(points[5]) - float.Parse(points[2])
            ).normalized;

            // Convert direction vector into rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Apply 90-degree corrective rotation along Z-axis
            Quaternion correctiveRotation = Quaternion.Euler(0, 0, -50);
            targetRotation *= correctiveRotation;

            // Apply slight smoothing for rotation to reduce jitter
            smoothedRotation = Quaternion.Slerp(smoothedRotation, targetRotation, smoothFactorRotation);
            bat.transform.rotation = smoothedRotation;
        }
    }
}
