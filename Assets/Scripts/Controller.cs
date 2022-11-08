using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //[SerializeField]

    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;

    float thrustPercent;
    float brakesTorque;

    PhysicsEngine aircraftPhysics;
    Rigidbody rb;

    private void Start()
    {
        aircraftPhysics = GetComponent<PhysicsEngine>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass=new Vector3(0f,1.4f,0.7f);
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            thrustPercent = thrustPercent > 0 ? 0 : 1f;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }


    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            wheel.motorTorque = 0.01f;
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in aircraftPhysics.aerodynamicSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }
}
