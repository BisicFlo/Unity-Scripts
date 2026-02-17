using UnityEngine;

/// <summary>
/// Snaps an object with a specific tag to the disered object along one chosen local axis (X/Y/Z).
/// Creates and configures a ConfigurableJoint to simulate sliding + limited rotation behavior.
/// Mainly used for shaft-like sliding mechanics (pistons, rails, drawers, etc.).
/// </summary>

[RequireComponent(typeof(Collider))] // + isTrigger = true

public class SnappingArea : MonoBehaviour {

    // --------------------------------------------------------------
    //   Inspector Fields
    // -------------------------------------------------------------- 

    [Header("Connection Settings")]
    [Tooltip("The Rigidbody this object should be physically connected to (usually the shaft/rail)")]
    public Rigidbody connectedBody; // Shaft / rail / parent rigidbody

    public enum AxisType { X, Y, Z };
    [Tooltip("Defines the sliding local direction of the two objects")]
    public AxisType Axis; // exemple : Z -> the blue arrow of the two objects must be aligned to snap

    [UnityTag] // need "UnityTagDrawer.cs" : custom property drawer
    [SerializeField]
    public string objectTag;

    [Header("Layer Management (optional)")]
    [SerializeField] private bool changeLayer = false;
    [SerializeField] private int defaultLayer = 5;
    [SerializeField] private int newLayer = 5;  // Usually a layer with no collision with the main object

    // --------------------------------------------------------------
    //   Private 
    // --------------------------------------------------------------

    //Which linear axes should be locked vs free/driven
    private bool lockX;
    private bool lockY;
    private bool lockZ;

    //Which angular axes should be locked
    private bool lockAngularX;
    private bool lockAngularY;
    private bool lockAngularZ;

    //Axis used to check alignment
    private Vector3 shaftAxis;
    private Vector3 objectAxis;

    // Very simple debounce / double-enter protection
    private GameObject lastEntered;
    private GameObject lastExited;

    // --------------------------------------------------------------
    //   MonoBehaviour
    // --------------------------------------------------------------

    private void OnTriggerEnter(Collider other) {

        // Ignore if already processing this object or if tag is not correct
        if (other.gameObject == lastEntered) return;
        if (!other.CompareTag(objectTag)) return;

        lastEntered = other.gameObject;
        lastExited = null;

        // Choose which local axis we consider the sliding direction
        if (Axis == AxisType.X) {
            shaftAxis = connectedBody.transform.right;
            objectAxis = other.transform.right;

        } else if (Axis == AxisType.Y) {
            shaftAxis = connectedBody.transform.up;
            objectAxis = other.transform.up;

        } else if (Axis == AxisType.Z) {
            shaftAxis = connectedBody.transform.forward;
            objectAxis = other.transform.forward;
        }

        // Only snap if the object is roughly aligned with the shaft (±~53° tolerance)
        // We don't need to use ".normalized" because right, up and forward are normalized
        float dot = Vector3.Dot(objectAxis, shaftAxis); 
        if (Mathf.Abs(dot) >= 0.6f) {

            other.transform.position = connectedBody.transform.position + Vector3.Project(other.transform.position - connectedBody.transform.position, shaftAxis);

            float angle1 = Vector3.Angle(shaftAxis, objectAxis);
            float angle2 = Vector3.Angle(-shaftAxis, objectAxis);
            Vector3 rotationAxis = Vector3.Cross(shaftAxis, objectAxis).normalized;

            if (angle1 <= angle2) {
                other.transform.RotateAround(other.transform.position, -rotationAxis, angle1);

            } else {
                other.transform.RotateAround(other.transform.position, rotationAxis, angle2);
            }

            // Optional: move to special layer so it doesn't collide with shaft collider
            if (changeLayer) other.gameObject.layer = newLayer;

            // Set which degrees of freedom should be locked
            ContraintBooleansConfig();

            // Create / update ConfigurableJoint
            JointConfig(other.gameObject);
        }

    }

    /// <summary>
    /// Called usually from UnsnappingArea.cs when user wants to detach
    /// </summary>
    public void DetachObject(GameObject attachedObject) {
        if (!attachedObject.CompareTag(objectTag)) return;

        ConfigurableJoint myJoint = attachedObject.GetComponent<ConfigurableJoint>();
        if (myJoint == null) return;

        // Disconnect joint
        myJoint.connectedBody = null;

        // Free all movement
        myJoint.xMotion = ConfigurableJointMotion.Free;
        myJoint.yMotion = ConfigurableJointMotion.Free;
        myJoint.zMotion = ConfigurableJointMotion.Free;

        // Free all rotation
        myJoint.angularXMotion = ConfigurableJointMotion.Free;
        myJoint.angularYMotion = ConfigurableJointMotion.Free;
        myJoint.angularZMotion = ConfigurableJointMotion.Free;

        // Remove drives / friction
        JointDrive drive = new JointDrive();
        drive.positionDamper = 0;  
        myJoint.xDrive = drive;
        myJoint.yDrive = drive;
        myJoint.zDrive = drive;
        myJoint.angularXDrive = drive;
        myJoint.angularYZDrive = drive;

        // Restore original layer if we changed it
        if (changeLayer) attachedObject.layer = defaultLayer;

    }

    private void OnTriggerExit(Collider other) {

        // Very basic exit debounce
        if (other.gameObject != lastExited && other.CompareTag(objectTag)) {
            lastExited = other.gameObject;
            lastEntered = null;
        }
    }

    /// <summary>
    /// Decides which linear and angular axes should be LOCKED vs FREE
    /// according to chosen sliding Axis
    /// </summary>
    private void ContraintBooleansConfig() {
        if (Axis == AxisType.X) {
            lockX = false;
            lockY = true;
            lockZ = true;
            lockAngularX = false;
            lockAngularY = true;
            lockAngularZ = true;

        } else if (Axis == AxisType.Y) {
            lockX = true;
            lockY = false;
            lockZ = true;
            lockAngularX = true;
            lockAngularY = false;
            lockAngularZ = true;

        } else if (Axis == AxisType.Z) {
            lockX = true;
            lockY = true;
            lockZ = false;
            lockAngularX = true;
            lockAngularY = true;
            lockAngularZ = false;
        }
    }

    /// <summary>
    /// Adds / configures ConfigurableJoint so object can slide along chosen axis
    /// with some damping/friction and can slightly rotate around sliding axis
    /// </summary>
    void JointConfig(GameObject myComponent) {
        ConfigurableJoint myJoint = myComponent.GetComponent<ConfigurableJoint>();
        if (myJoint == null) myJoint = myComponent.AddComponent<ConfigurableJoint>();

        myJoint.connectedBody = connectedBody; //1

        // Joint anchors at local origin of both objects
        myJoint.anchor = Vector3.zero;
        myJoint.autoConfigureConnectedAnchor = false;
        myJoint.connectedAnchor = Vector3.zero;

        // Linear constraints
        myJoint.xMotion = lockX ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;
        myJoint.yMotion = lockY ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;
        myJoint.zMotion = lockZ ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;

        // Angular constraints
        myJoint.angularXMotion = lockAngularX ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;
        myJoint.angularYMotion = lockAngularY ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;
        myJoint.angularZMotion = lockAngularZ ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;

        // Drive along sliding axis (strong position spring + damping)
        JointDrive slidingDrive = new JointDrive {
            positionDamper = 20f,          
            maximumForce = Mathf.Infinity
        };

        // Very weak angular drive -> allows slight wobble/rotation around axis
        JointDrive angularDrive = new JointDrive {
            positionDamper = 0.2f,
            maximumForce = Mathf.Infinity
        };


        if (Axis == AxisType.X) {
            myJoint.xDrive = slidingDrive;
            myJoint.angularXDrive = angularDrive;

        } else if (Axis == AxisType.Y) {
            myJoint.yDrive = slidingDrive;
            myJoint.angularYZDrive = angularDrive;

        } else if (Axis == AxisType.Z) {
            myJoint.zDrive = slidingDrive;
            myJoint.angularYZDrive = angularDrive;
        }
    }
}
