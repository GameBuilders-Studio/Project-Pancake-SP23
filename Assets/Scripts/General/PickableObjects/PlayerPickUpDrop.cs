using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickUpDrop : MonoBehaviour {


    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectPickPointTransform;
    [SerializeField] private LayerMask pickUpLayerMask;

    private ObjectPickable ObjectPickable;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            if (ObjectPickable == null) {
                // Not carrying an object, try to Pick
                float pickUpDistance = 4f;
                if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask)) {
                    if (raycastHit.transform.TryGetComponent(out ObjectPickable)) {
                        ObjectPickable.Pick(objectPickPointTransform);
                    }
                }
            } 
            else {
                // Currently carrying something, drop
                ObjectPickable.Drop();
                ObjectPickable = null;
            }
        }
    }
}