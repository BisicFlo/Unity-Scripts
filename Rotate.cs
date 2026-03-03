using UnityEngine;

[DisallowMultipleComponent]
public class Rotate : MonoBehaviour
{
    public enum AxisType { X, Y, Z };
    [Tooltip("Axis in World Space along which the object will rotate")]
    public AxisType Axis;

    [SerializeField, Tooltip("Degrees per second")]
    private float rotationSpeed = 90f;
  
    private Transform myTransform;
    private int rotationType = 0;

    void Awake() {
        myTransform = transform;
        if (Axis == AxisType.X) rotationType = 1;
        if (Axis == AxisType.Y) rotationType = 2;
        if (Axis == AxisType.Z) rotationType = 3;
    }

    void Update() {
        //  single-axis local rotation 
        if      (rotationType == 1) myTransform.Rotate(rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
        else if (rotationType == 2) myTransform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.Self);
        else if (rotationType == 3) myTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
