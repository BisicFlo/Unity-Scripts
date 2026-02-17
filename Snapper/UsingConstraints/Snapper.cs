using System;
using UnityEngine;
using UnityEngine.Events;

    /// <summary>
    /// Snaps compatible objects to this transform (shaft) based on shape rules.
    /// Uses trigger detection + name prefix matching.
    /// Supports cylindrical, Cube, polygonal, key etc.
    /// The Object to snap must have a Rigidbody and a BoxCollider, and be oriented in X axis (Red Arrow)
    /// The Shaft must have a BoxCollider in Trigger mode (isTrigger = true), and be oriented in X axis (Red Arrow)
    /// </summary>
public class Snapper : MonoBehaviour {
    
    // --------------------------------------------------------------
    //   Inspector Fields
    // -------------------------------------------------------------- 

    [Tooltip("The transform to snap towards (usually this transform or a child)")]
    public Transform Shaft; // L'axe principale

    [Tooltip("Objects must contain this string in their name to be considered")]
    public string Prefix = "Snap";

    public enum AxisType { X, Y, Z };
    [Tooltip("Axis in World Space along which the object will be constrained")]
    public AxisType Axis;

    [Tooltip("Number of sides — See documentation for more details")]
    [SerializeField] private int N; // 

    [Tooltip("Allow 180° flip")]
    [SerializeField] private bool CanBeFlipped;

    [Tooltip("Parent the snapped object to this transform when snapping")]
    [SerializeField] private bool SwitchParent;

    [Tooltip("Freeze movement/rotation completely when snapped")]
    [SerializeField] private bool LockWhenSnap;

    [Tooltip("Change layer of the object while inside the trigger")]
    [SerializeField] private bool ChangeLayer;

    [Tooltip("Layer to apply when inside")]
    [SerializeField] private int LayerIn;

    [Tooltip("Layer to restore when exiting")]
    [SerializeField] private int LayerOut;

    // --------------------------------------------------------------
    //   Events
    // --------------------------------------------------------------

    [Serializable] public class TriggerEvent : UnityEvent<GameObject> { }

    [Header("Events")]
    [Space(10)]

    [Tooltip("Fired when a compatible object successfully snaps")]
    [SerializeField] private TriggerEvent m_OnEnter = new TriggerEvent();

    [Tooltip("Fired when a compatible object leaves / unsnaps")]
    [SerializeField] private TriggerEvent m_OnExit = new TriggerEvent();

    public TriggerEvent OnEnter => m_OnEnter;
    public TriggerEvent OnExit => m_OnExit;

    // --------------------------------------------------------------
    //   Private 
    // --------------------------------------------------------------

    private Vector3 X1;
    private Vector3 X2;
    private Vector3 Y1;
    private Vector3 Y2;
    private Vector3 Z1;
    private Vector3 Z2;
    private Vector3 AxeRotation;

    // --------------------------------------------------------------
    //   MonoBehaviour
    // --------------------------------------------------------------

    private void Start() {
        if (Shaft == null) Shaft = this.transform;
    }

    private void OnTriggerEnter(Collider other) {

        if (other.name.Contains(Prefix)) {

            if (ChangeLayer) {
                other.gameObject.layer = LayerIn;
            }

            Debug.Log("detected");
            RefreshValue(other.gameObject); //X1 X2 Y1 Y2 Z1 Z2

            float dotProduct = Vector3.Dot(X2, X1);
            if ((dotProduct >= 0.6f && CanBeFlipped) || dotProduct <= -0.6f) {

                if (SwitchParent) {
                    Debug.Log("SwitchParent");
                    Vector3 ChildScale = other.transform.localScale;
                    Vector3 ParentScale = this.transform.localScale;
                    other.transform.SetParent(this.transform, true); // "worldPositionStays == true" 
                    RefreshValue(other.gameObject); //X1 X2 Y1 Y2 Z1 Z2
                }

                CylinderSetUp(other.gameObject);
                RefreshValue(other.gameObject); // We need to refresh the values because CylinderSetUp() modifies the configuration 
                NGonSetUp(other.gameObject);

                m_OnEnter?.Invoke(other.gameObject);

                if (LockWhenSnap) {
                    other.GetComponent<Rigidbody>().isKinematic = true;
                }   
            }
        }
    }

    private void OnTriggerExit(Collider other) {

        if (other.name.Contains(Prefix)) {
            if (ChangeLayer) other.gameObject.layer = LayerOut;
            
            if (SwitchParent) this.transform.DetachChildren();            

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null) rb.constraints = RigidbodyConstraints.None;            

            if (LockWhenSnap) other.GetComponent<Rigidbody>().isKinematic = false;
            
            m_OnExit?.Invoke(other.gameObject);
        }
    }


    private void CylinderSetUp(GameObject other) {
        other.transform.position = Shaft.position + Vector3.Project(other.transform.position - Shaft.position, X1);
        float angle1 = Vector3.Angle(X1, X2);
        float angle2 = Vector3.Angle(-X1, X2);
        AxeRotation = Vector3.Cross(X1, X2).normalized;

        if (angle1 <= angle2) {
            other.transform.RotateAround(other.transform.position, -AxeRotation, angle1);
        }

        if (angle1 > angle2) {
            other.transform.RotateAround(other.transform.position, AxeRotation, angle2);
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            if (Axis == AxisType.X) rb.constraints &= ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX);
            if (Axis == AxisType.Y) rb.constraints &= ~(RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX);
            if (Axis == AxisType.Z) rb.constraints &= ~(RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX);
        }
    }

    private void NGonSetUp(GameObject other) {
        if (N == 0) { Debug.Log("Error N=0"); return; }

        float angle3 = Vector3.Angle(Y1, Y2);
        float angle4 = Vector3.Angle(-Y1, Y2);

        float ModAngle1 = angle3 % (360 / N);
        float ModAngle2 = (360 / N) - ModAngle1;

        Debug.Log("ModAngle1 : " + ModAngle1);
        Debug.Log("ModAngle2 : " + ModAngle2);

        AxeRotation = Vector3.Cross(Y1, Y2).normalized;

        if (ModAngle1 < 360 / (N * 2)) {
            other.transform.RotateAround(other.transform.position, -AxeRotation, ModAngle1);
        } else {
            other.transform.RotateAround(other.transform.position, AxeRotation, ModAngle2);
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        }
    }

    private void RefreshValue(GameObject other) {
        X1 = Shaft.right;
        X2 = other.transform.right;
        Y1 = Shaft.up;
        Y2 = other.transform.up;
        Z1 = Shaft.forward;
        Z2 = other.transform.forward;
    }
}






