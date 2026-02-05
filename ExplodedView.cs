using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This Script is used to create a exploded view of mechanical sytems using two configurations 
public class ExplodedView : MonoBehaviour {

    // Reference to A and B objects which are the same object but in different position [ A : normal / B : exploded ]
    [SerializeField] private GameObject objectA; // Parent A
    [SerializeField] private GameObject objectB; // Parent B

    // Speed at which to interpolate between positions
    [SerializeField] private float lerpSpeed = 2.0f;

    // Flag to trigger movement to B
    [SerializeField] private bool reversed = false;

    // Flag to trigger movement back to original positions
    [SerializeField] private bool moving = false;

    // Array to store the original positions of children of A
    private Vector3[] originalPositions;

    private int timer;
    private WaitForSeconds waitBetweenUpdate = new WaitForSeconds(1);

    private void Start() {
        // Store the original positions of the children of A
        if (objectA != null) {
            int childCount = objectA.transform.childCount;
            originalPositions = new Vector3[childCount];

            for (int i = 0; i < childCount; i++) {
                originalPositions[i] = objectA.transform.GetChild(i).position;
            }
            StartCoroutine(Timer());
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) ChangeState();

        if (moving) {
            if (!reversed) {
                MoveChildrenToPositions(objectB);
            } else {
                MoveChildrenToOriginalPositions();
            }
        }
    }

    public void ChangeState() {  
        moving = true;           // Wake up the system 
        reversed = !reversed;    // Reverse the direction 
        timer = 4;               // Time Before the system rests
    }


    //Move children of A to positions of corresponding children in B
    private void MoveChildrenToPositions(GameObject targetObject) {
        // Ensure that both objects are assigned and have the same number of children
        if (objectA == null || targetObject == null || objectA.transform.childCount != targetObject.transform.childCount) {
            Debug.LogError("Object A and B must be not null and have the same number of children!");
            return;
        }

        // Iterate over children
        for (int i = 0; i < objectA.transform.childCount; i++) {
            Transform childA = objectA.transform.GetChild(i); // Child of A
            Transform childB = targetObject.transform.GetChild(i); // Child of B

            // Lerp position of each child of A to the corresponding child of B
            childA.position = Vector3.Lerp(childA.position, childB.position, Time.deltaTime * lerpSpeed);
        }
    }

    //Move children of A back to their original positions
    private void MoveChildrenToOriginalPositions() {
        // Ensure that object A has children and we have stored their original positions
        if (objectA == null || originalPositions == null || objectA.transform.childCount != originalPositions.Length) {
            Debug.LogError("Original positions are not properly set!");
            return;
        }

        // Iterate over children
        for (int i = 0; i < objectA.transform.childCount; i++) {
            Transform childA = objectA.transform.GetChild(i); // Child of A

            // Lerp position of each child of A back to its original position
            childA.position = Vector3.Lerp(childA.position, originalPositions[i], Time.deltaTime * lerpSpeed);
        }
    }

    private IEnumerator Timer() {
        while (true) {
            while (timer > 0) {
                timer--;
                yield return waitBetweenUpdate;
            }
            moving = false;
            yield return waitBetweenUpdate;
        }
    }

}
