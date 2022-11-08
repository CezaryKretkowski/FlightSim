using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsEngine : MonoBehaviour
{
    const float PREDICTION_TIMESTEP_FRACTION = 0.5f;

    float thrust = 6000;
    [SerializeField] 
    public List<LiftSurface> aerodynamicSurfaces =  null;

    Rigidbody rb;
    float thrustPercent;
    BiVector3 currentForceAndTorque;

    public void SetThrustPercent(float percent)
    {
        thrustPercent = percent;
    }

    private void Awake()
    {   
        rb = GetComponent<Rigidbody>();     
    }

    private void FixedUpdate()
    {
        BiVector3 forceAndTorqueThisFrame = 
            CalculateLiftForces(rb.velocity, rb.angularVelocity);

        Vector3 velocityPrediction = PredictVelocity(forceAndTorqueThisFrame.p
            + transform.forward * thrust * thrustPercent + Physics.gravity * rb.mass);
        Vector3 angularVelocityPrediction = PredictAngularVelocity(forceAndTorqueThisFrame.q);

        BiVector3 forceAndTorquePrediction = 
            CalculateLiftForces(velocityPrediction, angularVelocityPrediction);

        currentForceAndTorque = (forceAndTorqueThisFrame + forceAndTorquePrediction) * 0.5f;
        rb.AddForce(currentForceAndTorque.p);
        rb.AddTorque(currentForceAndTorque.q);

        rb.AddForce(transform.forward * thrust * thrustPercent);
    }

    private BiVector3 CalculateLiftForces(Vector3 velocity, Vector3 angularVelocity)
    {
        BiVector3 forceAndTorque = new BiVector3();
        foreach (var surface in aerodynamicSurfaces)
        {
            Vector3 relativePosition = surface.transform.position- rb.worldCenterOfMass;
            forceAndTorque += surface.CalculateForces(-velocity 
                -Vector3.Cross(angularVelocity,
                relativePosition), relativePosition);

        }
        return forceAndTorque;
    }

    private Vector3 PredictVelocity(Vector3 force)
    {
        return rb.velocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION * force / rb.mass;
    }

    private Vector3 PredictAngularVelocity(Vector3 torque)
    {
        Quaternion inertiaTensorWorldRotation = rb.rotation * rb.inertiaTensorRotation;
        Vector3 torqueInDiagonalSpace = Quaternion.Inverse(inertiaTensorWorldRotation) * torque;
        Vector3 angularVelocityChangeInDiagonalSpace;
        angularVelocityChangeInDiagonalSpace.x = torqueInDiagonalSpace.x / rb.inertiaTensor.x;
        angularVelocityChangeInDiagonalSpace.y = torqueInDiagonalSpace.y / rb.inertiaTensor.y;
        angularVelocityChangeInDiagonalSpace.z = torqueInDiagonalSpace.z / rb.inertiaTensor.z;

        return rb.angularVelocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION
            * (inertiaTensorWorldRotation * angularVelocityChangeInDiagonalSpace);
    }


}


