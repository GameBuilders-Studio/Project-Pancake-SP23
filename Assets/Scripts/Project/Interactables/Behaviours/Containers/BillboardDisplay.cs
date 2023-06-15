using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardDisplay : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] private List<Renderer> renderers;
    [SerializeField] private int renderQueueVal = 4000;
    void Start()
    {
        mainCamera = Camera.main;
        int countVal = renderQueueVal;
        foreach (var r in renderers)
        {
            r.material.renderQueue = countVal;
            countVal += 1;
        }
    }
    void LateUpdate()
    {
        // we want the billboard to simply always be facing the opposite direction of camera's viewing angle
        // with the same up vector
        transform.rotation = mainCamera.transform.rotation;
        //Sprites face -Z so we don't have to do this
        transform.Rotate(mainCamera.transform.up, 180);
        //with this we do a final rotation on the camera's side axis to make the sprite face the camera even in perspective
        // first get the difference in position, project to camera frame
        var viewDiffVecProj = mainCamera.transform
            .InverseTransformDirection(mainCamera.transform.position - transform.position);
        // kill the x axis difference in position
        // because in camera frame we know for sure the x axis difference we don't care
        viewDiffVecProj.x = 0;
        // project back. now the differences are aligned on camera's vertical plane
        var viewDiffWorld = mainCamera.transform.TransformDirection(viewDiffVecProj).normalized;
        transform.rotation *= Quaternion.FromToRotation(-transform.forward, viewDiffWorld);

    }
}
