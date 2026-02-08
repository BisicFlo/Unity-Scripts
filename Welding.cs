using UnityEngine;

/// <summary>
/// Merges two objects into a new combined object 
/// </summary>
/// 
/// This script detects when two compatible objects enter a trigger system (OnTriggerEnter)
/// Both objects are stored temporarily and then merged by calling MergeObjects()
/// Current detection method: OnTriggerEnter -> can be replaced with raycasts, mouse input, proximity checks, etc
/// 
/// 1. Attach this script to the Welding Tool with a collider with "Is Trigger" enabled
/// 2. Make sure both objects have at least one Collider
/// 3. Call MergeObjects()

public class Welding : MonoBehaviour {

    // --------------------------------------------------------------
    //   Private 
    // --------------------------------------------------------------

    private GameObject ParentObject1; // Used to store the Parent Object that contains the Rigidbody component 
    private GameObject ParentObject2; // Parent 2

    private string nameObject1; // Name of the object 1 we want to merge 
    private string nameObject2; // Name of the object 2 we want to merge 

    private int increment = 0;

    // --------------------------------------------------------------
    //   MonoBehaviour
    // --------------------------------------------------------------

    public void MergeObjects() {

        if (ParentObject1 != null && ParentObject2 != null) {
            MergeGameObjects();
        }
    }


    private void OnTriggerEnter(Collider other) {

        if (ParentObject1 == null) {
            ParentObject1 = GetRootObject(other.gameObject, 12);
            nameObject1 = other.name; // On met le nom de l'objet racine et non celui du parent qui contient le rb
            if (ParentObject1 == null) Debug.Log("Error Object1 is null");

        } else if (ParentObject2 == null) {
            if (VerifyDuplicates(GetRootObject(other.gameObject, 12), ParentObject1)) { //true = ok, objects differents // if (VerifyDuplicates(other.gameObject, Object1)) { 
                ParentObject2 = GetRootObject(other.gameObject, 12);
                nameObject2 = other.name;
                if (ParentObject2 == null) Debug.Log("Error Object2 is null");
            }
        } else {
            Debug.Log("TooMuchObject");
        }
    }



    private void OnTriggerExit(Collider other) {

        if (other.name == nameObject1) {
            ParentObject1 = null;
            nameObject1 = "";
            ParentObject1 = null;

        } else if (other.name == nameObject2) {
            ParentObject2 = null;
            nameObject2 = "";
            ParentObject2 = null;
        }
    }


    private GameObject GetRootObject(GameObject myGameObject, int limit) { // limit : recursivity limit
        // We want to get the object containing the Rigidbody because it is not always the object with colliders 
        
        GameObject TopObject = null;
        limit--;
        if (limit <= 0) {
            Debug.Log("Limit reached");
            return null;
        }

        Rigidbody rb = myGameObject.GetComponent<Rigidbody>();
        if (rb == null) {
            if (myGameObject.transform.parent != null) TopObject = GetRootObject(myGameObject.transform.parent.gameObject, limit);
            if (myGameObject.transform.parent == null) Debug.Log("No Parent Found");

        } else {
            TopObject = myGameObject;
        }
        return TopObject;
    }



    private void MergeGameObjects() {

        // Calculate the average position of the two game objects
        Vector3 averagePosition = (ParentObject1.transform.position + ParentObject2.transform.position) / 2f; //Object1.transform.position + Object1.transform.position

        // Create a new game object to hold the combined mesh
        GameObject mergedObject = new GameObject("Merged Object"+ increment);        

        mergedObject.transform.position = (ParentObject1.transform.position + ParentObject2.transform.position) / 2f;

        ParentObject1.transform.SetParent(mergedObject.transform);
        ParentObject2.transform.SetParent(mergedObject.transform);

        //Setup
        RemoveComponents(ParentObject1);
        RemoveComponents(ParentObject2);
        Setup(mergedObject);
        increment++;

        Debug.Log("Log : " + ParentObject1.name + " and " + ParentObject2.name + "were Merged ");
        Debug.Log("Log Child : " + nameObject1 + " and " + nameObject2 + "were Merged ");

        // Fix Double Welding 
        ParentObject1 = null;
        ParentObject2 = null;
        ParentObject1 = mergedObject;
    }

    private void Setup(GameObject myGameObject ) {
        myGameObject.AddComponent<Rigidbody>();
        // //- - - For VR only - - -
        //GameObject myGameObjectChild = new GameObject("Attach");
        //myGameObjectChild.transform.SetParent(myGameObject.transform); // "Attach" is now The Child Of "myGameObjectChild"        
        //OffsetInteractable MyObject_OffSetGrab = myGameObject.AddComponent<OffsetInteractable>();
        //MyObject_OffSetGrab.attachTransform = myGameObjectChild.transform;
        //MyObject_OffSetGrab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        //myGameObject.layer = 8; // Layer 8 : Metal 
        //myGameObject.tag = "Tool";
        //MyObject_OffSetGrab.velocityScale = 0.5f;
        //MyObject_OffSetGrab.angularVelocityScale = 0.5f;
    }

    private bool VerifyDuplicates(GameObject myGameObject1, GameObject myGameObject2) {
        if (myGameObject1 != myGameObject2) return true;
        else return false;
    }

    private void RemoveComponents(GameObject myGameObject) {
        // - - - For VR only - - -
        // Destroy(myGameObject.GetComponent<OffsetInteractable>());
        Destroy(myGameObject.GetComponent<Rigidbody>());
    }
}
