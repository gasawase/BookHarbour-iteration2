using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BookHarbour
{
    public class Draggable : MonoBehaviour
    {
        private Vector2 dragOffset; // Offset between pointer and object's center
        private Transform originalParent; // Original parent for re-parenting
        private Vector2 originalPosition;
        private bool isDragging = false;
        private Camera mainCamera;

        // Input Actions
        public UserInputActions userInputActions;
        private InputAction dragAction;
        private InputAction pointerLocationAction;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (isDragging)
            {
                Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
            }
        }
        
        private void MoveObject()
        {
            Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
            
        }
        
    }
}
