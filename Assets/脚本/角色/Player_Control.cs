using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    OnFoot,
    InVehicle
}

public class Player_Control : MonoBehaviour
{
    private CharacterController controller;
    private Player_Condition playerCondition;
    private Transform cameraTransform;
    private Vector3 originalCameraPosition;
    public PlayerState CurrentPlayerState { get; set; } = PlayerState.OnFoot;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCondition = GetComponent<Player_Condition>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        originalCameraPosition = cameraTransform.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleCrouch();
    }
    
    void HandleMovement()
    {
        if (CurrentPlayerState == PlayerState.InVehicle)
        {
            return;
        }
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && playerCondition != null && playerCondition.CanSprint;
        bool isCrouching = playerCondition != null && playerCondition.IsCrouching;
        
        float currentSpeed;
        if (isCrouching)
        {
            currentSpeed = playerCondition != null ? playerCondition.crouchSpeed : 2.5f;
        }
        else if (isSprinting)
        {
            currentSpeed = playerCondition != null ? playerCondition.sprintSpeed : 10f;
        }
        else
        {
            currentSpeed = playerCondition != null ? playerCondition.walkSpeed : 5f;
        }
        
        if (isSprinting && move.magnitude > 0)
        {
           playerCondition.StartSprinting();
        }
        else if (playerCondition != null)
        {
            playerCondition.StopSprinting();
        }
        
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    
    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (playerCondition != null && !playerCondition.IsCrouching)
            {
                playerCondition.SetCrouch(true);
                controller.height = playerCondition.crouchHeight;
                controller.center = new Vector3(0, playerCondition.crouchHeight / 2, 0);
                
                float heightDifference = playerCondition.standingHeight - playerCondition.crouchHeight;
                Vector3 targetPosition = originalCameraPosition - Vector3.up * heightDifference;
                StartCoroutine(SmoothCameraMove(targetPosition));
            }
        }
        else
        {
            if (playerCondition != null && playerCondition.IsCrouching)
            {
                playerCondition.SetCrouch(false);
                controller.height = playerCondition.standingHeight;
                controller.center = new Vector3(0, playerCondition.standingHeight / 2, 0);
                
                StartCoroutine(SmoothCameraMove(originalCameraPosition));
            }
        }
    }
    
    IEnumerator SmoothCameraMove(Vector3 targetPosition)
    {
        float duration = 0.3f;
        float elapsedTime = 0;
        Vector3 startPosition = cameraTransform.localPosition;
        
        while (elapsedTime < duration)
        {
            cameraTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        cameraTransform.localPosition = targetPosition;
    }
}
