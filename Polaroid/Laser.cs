using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {
    /// <summary>
    /// Use Raycast and OnTriggerEnter to know what the camera can see or can't see 
    /// 
    /// We need a "Fild of view" mesh as "Trigger" , a Mesh pyramid with MeshCollider with "IsTrigger" true
    /// 
    /// </summary>

    private List<GameObject> ObjectList = new List<GameObject>();

    private float rayLength;

    private void OnTriggerEnter(Collider other) {

        if (other.gameObject.name.Contains("RISK")) {
            ObjectList.Add(other.gameObject);
        } else {

            Transform MyParent = other.transform.parent;
            if (MyParent != null) {
                if (MyParent.name.Contains("RISK")) ObjectList.Add(MyParent.gameObject);
            }
        }

    }

    private void OnTriggerExit(Collider other) {

        if (other.gameObject.name.Contains("RISK")) {
            ObjectList.Remove(other.gameObject);
        } else {
            Transform MyParent = other.transform.parent;
            if (MyParent != null) {
                if (MyParent.name.Contains("RISK")) ObjectList.Remove(MyParent.gameObject);
            }
        }
    }

    public GameObject GetClosestObject() {

        GameObject closestObject = null;
        float closestDistance = rayLength;

        if (ObjectList.Count > 0) {
            foreach (GameObject obj in ObjectList) {
                float distance = Vector3.Distance(this.transform.position, obj.transform.position);
                Debug.Log($"Distance to {obj.name}: {distance}" + " - - -  rayLength : " + rayLength);
                if (distance < closestDistance) {
                    closestObject = obj;
                    closestDistance = distance;
                }
            }
        }
        return closestObject;
    }


    public GameObject GetCenterObject() {

        GameObject closestObject = null;
        float closestDistance = 1000f;

        if (ObjectList.Count > 0) {
            foreach (GameObject obj in ObjectList) {
                Vector3 AB = obj.transform.position - this.transform.position;
                Vector3 AC = Vector3.Project(AB, this.transform.forward);
                Vector3 BC = AC - AB;

                float distance = BC.sqrMagnitude;

                if (distance < closestDistance) {
                    closestObject = obj;
                    closestDistance = distance;
                }
            }
        }
        return closestObject;
    }

    public bool TestRaycast(GameObject MyObject) {
        int layerMask = 1 << 2;
        layerMask = ~layerMask;

        float maxRange = 1000f;
        RaycastHit hit;
        bool targetHit = false;

        if (Vector3.Distance(this.transform.position, MyObject.transform.position) < maxRange) {
            if (Physics.Raycast(this.transform.position, (MyObject.transform.position - this.transform.position), out hit, maxRange, layerMask)) {
                if (hit.transform == MyObject.transform) {
                    Debug.DrawRay(this.transform.position, (MyObject.transform.position - this.transform.position) * hit.distance, Color.yellow);
                    Debug.Log("Hit");
                    rayLength = hit.distance;
                    targetHit = true;
                }

            } else {
                Debug.DrawRay(this.transform.position, (MyObject.transform.position - this.transform.position) * 1000, Color.white);
                //Debug.Log("Did not Hit");
                rayLength = 1000;

            }
        }
        return targetHit;
    }

    public float ReturnAngleObjectAndCenter(GameObject MyObject) {
        float angle = 0;

        angle = Vector3.Angle(this.transform.forward, MyObject.transform.position - this.transform.position);

        return angle;        
    }


    public void RemoveFixedRisk(GameObject MyRisk) {

        if (MyRisk.name.Contains("RISK")) {
            ObjectList.Remove(MyRisk);
        }
        else {
            GameObject MyParent = MyRisk.transform.parent.gameObject;
            if (MyParent != null) {
                if (MyParent.name.Contains("RISK")) ObjectList.Remove(MyParent);
            }
        }
    }

}
