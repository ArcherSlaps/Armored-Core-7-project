using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;

    [Header("Movement Speeds")]
    public float walkingSpeed = 7f;
    public float movementSpeed = 7;
    public float sprintingSpeed = 7;
    public float rotationSpeed = 15;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
       
        //moveDirection = cameraObject.forward * inputManager.verticalInput;
        //moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
        //moveDirection.Normalize();
        //moveDirection.y = 0;

        //If walking select speed
        //If running select speed
        //If sprinting select speed
        //if(inputManager.moveAmount >= 0.5f)
        //{
           // moveDirection = moveDirection * boostSpeed;
        //}
        //else
        //{
           // moveDirection = moveDirection * walkingSpeed;
        //}
        

        
        //Vector3 movementVelocity = moveDirection;
        //movementVelocity.y = playerRigidbody.linearVelocity.y; // Use .velocity.y if on older Unity versions

       
        //playerRigidbody.linearVelocity = movementVelocity; // Use .velocity if on older Unity versions

        // 1. Get horizontal camera directions (ignore the camera's tilt)
        Vector3 camForward = cameraObject.forward;
        Vector3 camRight = cameraObject.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 2. Calculate direction based only on flat ground plane
        moveDirection = (camForward * inputManager.verticalInput) + (camRight * inputManager.horizontalInput);

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 3. MULTIPLY BY SPEED (Fixes slow movement)
        // Adjust 'movementSpeed' here based on sprinting/walking logic if needed
        Vector3 targetVelocity = moveDirection * movementSpeed;

        // 4. Preserve existing gravity/fall velocity
        targetVelocity.y = playerRigidbody.linearVelocity.y; // Use .velocity.y on older Unity versions

        // 5. Apply to Rigidbody
        playerRigidbody.linearVelocity = targetVelocity; // Use .velocity on older Unity versions

    }

    public void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();

        // Keep rotation calculations flat
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }
}
