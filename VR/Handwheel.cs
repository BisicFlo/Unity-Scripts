using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 
using UnityEngine.XR.Interaction.Toolkit; // VR

/// <summary>
/// XR-based rotatable wheel / knob / dial interactable.
/// Works with both one-handed and two-handed interaction.
/// Rotates around local forward axis (usually Z).
/// </summary>
public class Handwheel : XRBaseInteractable {

    [Header("References")]
    [SerializeField, Tooltip("Empty GameObject that will be actually rotated (pivot point)")]
    private Transform Pivot;

    [Header("Behaviour")]
    [SerializeField, Tooltip("When true, wheel rotation is completely blocked")]
    private bool isBlocked = false;

    [Header("Events")]
    [Tooltip("Called every time the wheel rotates - argument = delta angle this frame")]
    public UnityEvent<float> OnWheelRotated;

    // Internal tracking
    private float currentAngle = 0.0f;


    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        base.OnSelectEntered(args);
        currentAngle = FindWheelAngle();
    }

    protected override void OnSelectExited(SelectExitEventArgs args) {
        base.OnSelectExited(args);
        currentAngle = FindWheelAngle();
    }


    /// <summary>
    /// Main XR interaction update loop
    /// </summary>
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {

        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic) {
            if (isSelected) {
                if (!isBlocked) {
                    RotateWheel();
                }
            }
        }
    }


    /// <summary>
    /// Calculates how much the wheel should rotate this frame and applies it
    /// </summary>
    private void RotateWheel() {
        // Calculates how much the wheel should rotate this frame and applies it

        // Current combined angle from all grabbing hands/controllers
        float totalAngle = FindWheelAngle();

        // How much it changed since last frame
        float angleDifference = currentAngle - totalAngle;

        // Apply rotation to the pivot object 
        Pivot.Rotate(transform.forward, -angleDifference, Space.World);

        // Remember new reference angle for next frame
        currentAngle = totalAngle;

        // Notify listeners about the rotation amount
        OnWheelRotated?.Invoke(angleDifference);

    }


    /// <summary>
    /// Calculates "virtual angle" from all current interactors combined
    /// </summary>
    private float FindWheelAngle() {
        float totalAngle = 0;

        // Average contribution from each hand
        foreach (IXRSelectInteractor interactor in interactorsSelecting) {
            Vector2 direction = FindLocalPoint(interactor.transform.position);
            totalAngle += ConvertToAngle(direction) * FindRotationSensitivity();
        }
        return totalAngle;
    }


    /// <summary>
    /// When multiple hands grab -> each hand contributes less rotation
    /// </summary>
    private float FindRotationSensitivity() {
        return 1.0f / interactorsSelecting.Count;
    }


    /// <summary>
    /// Converts 2D direction vector into signed angle from "up" direction
    /// </summary>
    private float ConvertToAngle(Vector2 direction) {
        return Vector2.SignedAngle(Vector2.up, direction); 
    }


    /// <summary>
    /// Gets normalized direction vector in local space of the wheel
    /// </summary>
    private Vector2 FindLocalPoint(Vector3 position) {
        return transform.InverseTransformPoint(position).normalized;
    }
}


