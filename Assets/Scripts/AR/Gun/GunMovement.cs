using UnityEngine;

public class GunMovement : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.05f;
    public float swaySmoothing = 8f;
    public float rotationSwayMultiplier = 1.5f;

    [Header("Bobbing Settings")]
    public float bobbingSpeed = 0.08f;
    public float bobbingAmount = 0.005f;
    public float rotationBobbingMultiplier = 0.5f;

    // Device motion parameters
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 newWeaponPosition;
    private Quaternion newWeaponRotation;
    private Vector3 previousAcceleration;
    private float timer = 0;

    private void Start()
    {
        // Store initial position and rotation
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        newWeaponPosition = initialPosition;
        newWeaponRotation = initialRotation;
        previousAcceleration = Input.acceleration;
    }

    private void Update()
    {
        ApplyDeviceMotionSway();
        ApplyBobbing();
        UpdateGunTransform();
    }

    private void ApplyDeviceMotionSway()
    {
        // Get device acceleration (phone motion)
        Vector3 acceleration = Input.acceleration;

        // Calculate delta from previous frame to detect motion
        Vector3 accelerationDelta = acceleration - previousAcceleration;
        previousAcceleration = acceleration;

        // Smooth the acceleration input
        Vector3 swayInput = Vector3.Lerp(Vector3.zero, accelerationDelta * swayAmount, Time.deltaTime * swaySmoothing);

        // Apply sway to position (left/right and up/down)
        float positionSwayX = -swayInput.x * swayAmount;
        float positionSwayY = -swayInput.y * swayAmount;

        // Calculate target position with sway
        Vector3 targetPosition = initialPosition + new Vector3(positionSwayX, positionSwayY, 0);

        // Limit maximum sway
        targetPosition.x = Mathf.Clamp(targetPosition.x, initialPosition.x - maxSwayAmount, initialPosition.x + maxSwayAmount);
        targetPosition.y = Mathf.Clamp(targetPosition.y, initialPosition.y - maxSwayAmount, initialPosition.y + maxSwayAmount);

        // Apply rotational sway based on position sway
        Quaternion targetRotation = initialRotation * Quaternion.Euler(
            -positionSwayY * rotationSwayMultiplier * 100,
            positionSwayX * rotationSwayMultiplier * 100,
            0);

        // Update weapon position and rotation with smoothing
        newWeaponPosition = Vector3.Lerp(newWeaponPosition, targetPosition, Time.deltaTime * swaySmoothing);
        newWeaponRotation = Quaternion.Slerp(newWeaponRotation, targetRotation, Time.deltaTime * swaySmoothing);
    }

    private void ApplyBobbing()
    {
        // Update timer for bobbing effect
        timer += Time.deltaTime * bobbingSpeed;

        // Calculate bobbing offsets using sine wave for vertical and cosine for horizontal
        float verticalBob = Mathf.Sin(timer) * bobbingAmount;
        float horizontalBob = Mathf.Cos(timer / 2) * bobbingAmount * 0.5f;

        // Add bobbing to position
        Vector3 bobbingPosition = new Vector3(horizontalBob, verticalBob, 0);
        newWeaponPosition += bobbingPosition;

        // Add slight rotation bobbing for more natural feel
        Quaternion bobbingRotation = Quaternion.Euler(
            verticalBob * rotationBobbingMultiplier * 100,
            horizontalBob * rotationBobbingMultiplier * 50,
            horizontalBob * rotationBobbingMultiplier * 25);

        newWeaponRotation *= bobbingRotation;
    }

    private void UpdateGunTransform()
    {
        // Apply the final position and rotation to the gun
        transform.localPosition = newWeaponPosition;
        transform.localRotation = newWeaponRotation;
    }
}
