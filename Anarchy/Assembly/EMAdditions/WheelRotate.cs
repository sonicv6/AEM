using UnityEngine;
using System.Collections;
using Anarchy.UI;

public class WheelRotate : MonoBehaviour {
    private Vector3 prevPos;
    private Vector3 newPos;
    private Vector3 objVelocity;

    public Transform FLWheelT;
    public Transform FRWheelT;
    public Transform BLWheelT;
    public Transform BRWheelT;
    public WheelCollider Col;

    // Use this for initialization
    private void Start () {
        prevPos = transform.position;
    }
    private void Update () {
        newPos = transform.position;  // each frame track the new position
        objVelocity = (newPos - prevPos) / Time.deltaTime;
        prevPos = newPos;  // update position for next frame calculation
        float angleRotation = (Col.radius * objVelocity.magnitude * Time.deltaTime * 57.296f);
        var axis = -transform.right;
        if (objVelocity.magnitude >= 1)
        {
            FLWheelT.RotateAround(FLWheelT.renderer.bounds.center, axis, angleRotation);
            FRWheelT.RotateAround(FRWheelT.renderer.bounds.center, axis, angleRotation);
            BLWheelT.RotateAround(BLWheelT.renderer.bounds.center, axis, angleRotation);
            BRWheelT.RotateAround(BRWheelT.renderer.bounds.center, axis, angleRotation);
        }
    }
}