using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour {
    public bool activated;
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;

    [SerializeField] private Transform target; // The target (Position B)
    [SerializeField] private float jumpForce = 10f; // Initial jump speed
    [SerializeField] private float jumpTime = 1;
    [SerializeField] private float groundCheckRadius = 0.2f; // Radius for ground check
    [SerializeField] private LayerMask groundLayer; // Layer for ground detection
    [SerializeField] private float gravity = -9.81f; // Simulated gravity
    [SerializeField] private int tempo = 1;

    private Vector3 velocity; // Current velocity of the slime
    private bool isGrounded;
    private Vector3 groundCheckPosition;

    private Transform anchorPoint;
    private WaitForSeconds tempoDuration; //
    private bool isJumping = false;

    private void Start() {
        tempoDuration = new WaitForSeconds(tempo); 

        anchorPoint = this.transform;
    }

    void Update() {
        if (activated) {
            if (!isJumping) {
                isJumping = true;
                StartCoroutine(LerpPosition(this.transform.position, target.position, jumpTime, 1));
                StartCoroutine(LerpRotation(this.transform.rotation, target.rotation, jumpTime, 2));
            }
        }
    }

    // Visualize the ground check in the editor
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
    }

    private IEnumerator LerpPosition(Vector3 startValue, Vector3 endValue, float lerpDuration, int mode) {
        //Debug.Log("LerpPosition");
        float timeElapsed = 0;
        Vector3 startValueFrozen = startValue;
        Vector3 endValueFrozen = endValue;
        Vector3 jump = Vector3.zero;
        //\sin\left(3.14\cdot x\cdot0.8-x^{2}\right)
        while (timeElapsed < lerpDuration) {
            float t = timeElapsed / lerpDuration;
            if (mode == 1) t = Mathf.Sin(t * Mathf.PI * 0.5f);
            if (mode == 2) t = Mathf.SmoothStep(0, 1, t);


            if (skinnedMeshRenderer != null) SetBlendShapeJump(t);

            this.transform.position = Vector3.Slerp(startValueFrozen, endValueFrozen, t);
            jump.y = 0.5f * gravity * lerpDuration * timeElapsed - 0.5f * gravity * timeElapsed * timeElapsed;
            this.transform.position -= jump;
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        this.transform.position = endValue;
        //isJumping = false;
        if (skinnedMeshRenderer != null) StartCoroutine(Jiggle(1.5f));
        StartCoroutine(DontMove(this.transform.position, this.transform.rotation, Random.Range(0.5f, 2f)));
    }
    private IEnumerator LerpRotation(Quaternion startValue, Quaternion endValue, float lerpDuration, int mode) {
        float timeElapsed = 0;
        Quaternion startValueFrozen = startValue;
        Quaternion endValueFrozen = endValue;
        //\sin\left(3.14\cdot x\cdot0.8-x^{2}\right)
        while (timeElapsed < lerpDuration) {
            float t = timeElapsed / lerpDuration;
            if (mode == 1) t = Mathf.Sin(t * Mathf.PI * 0.5f);
            if (mode == 2) t = Mathf.SmoothStep(0, 1, t);

            this.transform.rotation = Quaternion.Slerp(startValueFrozen, endValueFrozen, t);
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        this.transform.rotation = endValue;
        //isJumping = false;
    }
    private IEnumerator DontMove(Vector3 startPosition, Quaternion startRotation, float duration) {
        float timeElapsed = 0;
        Vector3 startPositionFrozen = startPosition;
        Quaternion startRotationFrozen = startRotation;

        while (timeElapsed < duration) {
            this.transform.position = startPositionFrozen;
            this.transform.rotation = startRotationFrozen;
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        isJumping = false;
    }

    private IEnumerator Jiggle(float duration) {
        float amplitude = 3f; // Initial strength of the jiggle
        float decayRate = 2f;   // How quickly the jiggle fades
        float frequency = 15f;
        float timeElapsed = 0;
        //Debug.Log("starting Jiggle");

        while (timeElapsed < duration) {
            float jiggle = amplitude * Mathf.Exp(-decayRate * timeElapsed) * Mathf.Sin(frequency * timeElapsed) ;
            skinnedMeshRenderer.SetBlendShapeWeight(0, skinnedMeshRenderer.GetBlendShapeWeight(0)+jiggle );
            timeElapsed += Time.deltaTime;
            //Debug.Log("jiggle : " + jiggle);
            yield return null;
        }      
    }
    private void SetBlendShapeJump(float t) { //t:0->1
        if (t < 0.5f) {
            int sharpness = 10;
            float output = (sharpness * t * t - sharpness * t + 1) * 60;
            if (output > 100) output = 100;
            if (output < 0) output = 0;
            skinnedMeshRenderer.SetBlendShapeWeight(0, output);
        } else {
            int sharpness = 80;
            float output = (sharpness * t * t - sharpness * t + 1) * 60;
            if (output > 100) output = 100;
            if (output < 0) output = 0;
            skinnedMeshRenderer.SetBlendShapeWeight(0, output);
        }        
    }
}
