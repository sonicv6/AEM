﻿using UnityEngine;
using System.Collections;
using Anarchy.UI;

public class WheelRotate : MonoBehaviour {
    private Vector3 prevPos;
    private Vector3 newPos;
    private Vector3 objVelocity;
    private Vector3 fWheelPos = new Vector3(0, -1.05477f, -3.291072f);
    private Vector3 bWheelPos = new Vector3(0, 1.252668f, -0.2125787f);
    private Quaternion wheelRot = Quaternion.Euler(73.70572f, -180f, -180f);
    private bool rotating;

    public Transform FLWheelT;
    public Transform FRWheelT;
    public Transform BLWheelT;
    public Transform BRWheelT;

    private void Start () {
        prevPos = transform.position;
    }
    private void Update ()
    {
        rotating = false;
        newPos = transform.position;
        objVelocity = (newPos - prevPos) / Time.deltaTime;
        prevPos = newPos;
        float angleRotation = (0.672f * objVelocity.magnitude * Time.deltaTime * 57.296f);
        var axis = -transform.right;
        if (objVelocity.magnitude >= 1)
        {
            rotating = true;
            FLWheelT.RotateAround(FLWheelT.renderer.bounds.center, axis, angleRotation);
            FRWheelT.RotateAround(FRWheelT.renderer.bounds.center, axis, angleRotation);
            BLWheelT.RotateAround(BLWheelT.renderer.bounds.center, axis, angleRotation);
            BRWheelT.RotateAround(BRWheelT.renderer.bounds.center, axis, angleRotation);
        }
    }

    private void FixedUpdate()
    {
        if (FLWheelT.localPosition != fWheelPos && !rotating)
        {
            FLWheelT.localPosition = fWheelPos;
            FRWheelT.localPosition = fWheelPos;
            BLWheelT.localPosition = bWheelPos;
            BRWheelT.localPosition = bWheelPos;
            FLWheelT.localRotation = wheelRot;
            FRWheelT.localRotation = wheelRot;
            BLWheelT.localRotation = wheelRot;
            BRWheelT.localRotation = wheelRot;
        }
    }
}