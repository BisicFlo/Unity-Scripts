using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Companion trigger zone that detects when a snapped object is being pulled OUT
/// of the snapping/snapping area and calls DetachObject() on the corresponding SnappingArea.
/// 
/// Works in pair with SnappingArea to create a clean attach/detach mechanic.
/// Usually the corresponding Collider is bigger than the one used by 'SnappingArea.cs"
/// to create an Hysteresis and prevent wobble effect in the edge of the colliders
/// </summary>

[RequireComponent(typeof(Collider))] // + isTrigger = true

public class UnsnappingArea : MonoBehaviour {

    [Header("References")]
    [Tooltip("Reference to the SnappingArea that actually manages the joint and snapping logic")]
    public SnappingArea SnappingArea;

    //  Debounce mechanism 
    private GameObject lastExited;
    private GameObject lastEntered;

    private void OnTriggerExit(Collider other) {

        // Prevent duplicate calls if OnTriggerExit is called multiple times for same object
        if (other.gameObject == lastExited) return;

        lastExited = other.gameObject;
        lastEntered = null;

        if (SnappingArea!= null) SnappingArea.DetachObject(other.gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject != lastEntered && other.CompareTag(SnappingArea.objectTag)) {
            lastEntered = other.gameObject;
            lastExited = null;
        }
    }
}
