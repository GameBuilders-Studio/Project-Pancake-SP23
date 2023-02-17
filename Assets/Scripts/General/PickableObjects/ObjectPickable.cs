using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPickable : MonoBehaviour {


    private Rigidbody objectRigidbody;
    private Transform objectPickPointTransform;

    private void Awake() {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    public void Pick(Transform objectPickPointTransform) {
        this.objectPickPointTransform = objectPickPointTransform;
        objectRigidbody.useGravity = false;
    }

    public void Drop() {
        this.objectPickPointTransform = null;
        objectRigidbody.useGravity = true;
    }

    private void FixedUpdate() {
        if (objectPickPointTransform != null) {
            float lerpSpeed = 10f;
            Vector3 newPosition = Vector3.Lerp(transform.position, objectPickPointTransform.position, Time.deltaTime * lerpSpeed);
            objectRigidbody.MovePosition(newPosition);
        }
    }


}