using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a GameObject destructible —> the trick is to  replace it with a broken/prefab version 
/// and applies explosion force to its pieces when hit hard enough by an object 
/// with a specific tag.
/// 
/// Typical use case: breakable walls, crates, glass, etc.
/// 
/// You can also call Destruction() to trigger it manually 
/// </summary>
public class Destructible : MonoBehaviour {

    // --------------------------------------------------------------
    //   Inspector Fields
    // -------------------------------------------------------------- 
    [Tooltip("Prefab containing broken pieces")]
    [SerializeField] private Transform PrefabBrokenVersion;

    [Tooltip("Tag of objects that are allowed to break this destructible")]
    [SerializeField] private string selectedTag;

    // --------------------------------------------------------------
    //   Private 
    // --------------------------------------------------------------

    private bool isAlreadyDestroyed = false; // Prevents multiple destruction triggers

    // --------------------------------------------------------------
    //   MonoBehaviour
    // --------------------------------------------------------------

    void OnCollisionEnter(Collision collision) {
        // Already broken -> ignore any further collisions
        if (!isAlreadyDestroyed) {

            // Wrong tag -> ignore
            if (collision.gameObject.tag == selectedTag  {

                // Not fast enough -> ignore (tweak 4f threshold to your game's scale/physics)
                if (collision.relativeVelocity.magnitude > 4) {
                    isAlreadyDestroyed = true;

                    // Use the first contact point as explosion center 
                    ContactPoint contact = collision.contacts[0];          
                    Vector3 position = contact.point;

                    // Spawn broken version
                    Transform WallBrockenTransform = Instantiate(PrefabBrokenVersion, transform.position, transform.rotation);

                    // Apply explosion force to each rigidbody piece
                    foreach (Transform child in WallBrockenTransform) {
                        if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody)) {
                            childRigidbody.AddExplosionForce(100f, position, 5f);
                        }
                    }
                    Destroy(this.gameObject);
                }
            }
        }
    }


    /// <summary>
    /// Public method to destroy the object from other scripts (e.g. health = 0, timer, event, etc.)
    /// Explosion centered on object pivot.
    /// </summary>
    public void Destruction() {

        // Optional: hide original mesh (useful if you want particles/effects before destroy
        if (TryGetComponent<MeshRenderer>(out MeshRenderer renderer)) {
            renderer.enabled = false;
        }
        // Spawn broken pieces
        Transform WallBrockenTransform = Instantiate(PrefabBrokenVersion, transform.position, transform.rotation);

        // Apply explosion from center of object
        foreach (Transform child in WallBrockenTransform) {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody)) {
                childRigidbody.AddExplosionForce(100f, transform.position, 5f);
            }
        }
        Destroy(this.gameObject);
    }
}
