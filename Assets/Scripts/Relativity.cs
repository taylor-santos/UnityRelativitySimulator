using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relativity : MonoBehaviour
{
    public float InitialProperTime;
    public Vector3 InitialVelocity;
    public List<Vector4> Accelerations;

    void Update() {
        var currBoost = boost(-InitialVelocity);
        var objOffset = combineSpaceTime(transform.position, InitialProperTime);
        var currPos_Obs = currBoost * objOffset;
        var pastDir = currBoost * combineSpaceTime(Vector3.zero, -1);
        Debug.DrawRay(DrawEvent(currPos_Obs), DrawEvent(pastDir)*1000);
        Debug.DrawRay(Vector3.zero, new Vector3(1, 0, 1) * 1000);
        Debug.DrawRay(Vector3.zero, new Vector3(-1, 0, 1) * 1000);
        Debug.DrawRay(Vector3.zero, new Vector3(-1, 0, -1) * 1000);
        Debug.DrawRay(Vector3.zero, new Vector3(1, 0, -1) * 1000);

        for (int i = 0; i < Accelerations.Count; i++) {
            var accel = Accelerations[i];
            var properDuration = getTime(accel);
            var properAccel = getSpace(accel);
            var MCRFDuration = properAccel.magnitude > Mathf.Epsilon
                ? (float)System.Math.Sinh(properDuration * properAccel.magnitude) / properAccel.magnitude
                : properDuration;

            var T = getTime(objOffset);
            var X = getSpace(objOffset);
            var Xa = Vector3.Dot(X, properAccel);
            var A = T * (1 - Xa * Xa + properAccel.sqrMagnitude * X.sqrMagnitude) / (T * T * properAccel.sqrMagnitude - (Xa - 1) * (Xa - 1)) - T;
            var B = Mathf.Sqrt(Mathf.Pow(Xa - 1, 2) * (properAccel.sqrMagnitude * Mathf.Pow(T * T - X.sqrMagnitude, 2) + 4 * (Xa * (T * T - X.sqrMagnitude) + X.sqrMagnitude)) / Mathf.Pow(-T * T * properAccel.sqrMagnitude + Mathf.Pow(Xa - 1, 2), 2));
            if (0 <= (A + B) / 2 && (A + B) / 2 < MCRFDuration) {
                var x = properAccel * (Mathf.Sqrt(1 + Mathf.Pow((A + B) / 2, 2) * properAccel.sqrMagnitude) - 1) / properAccel.sqrMagnitude;
                var y = combineSpaceTime(x, (A + B) / 2);
                var z = currBoost * objOffset + currBoost * y;
                Debug.Log(Mathf.Pow(getTime(z), 2) + " " + getSpace(z).sqrMagnitude);
                if (Mathf.Abs(Mathf.Pow(getTime(z), 2) - getSpace(z).sqrMagnitude) < 0.01f) {
                    Debug.DrawRay(DrawEvent(z), Vector3.up, Color.green);
                }
            }
            if (0 <= (A - B) / 2 && (A - B) / 2 < MCRFDuration) {
                var x = properAccel * (Mathf.Sqrt(1 + Mathf.Pow((A - B) / 2, 2) * properAccel.sqrMagnitude) - 1) / properAccel.sqrMagnitude;
                var y = combineSpaceTime(x, (A - B) / 2);
                var z = currBoost * objOffset + currBoost * y;
                Debug.Log(Mathf.Pow(getTime(z), 2) + " " + getSpace(z).sqrMagnitude);
                if (Mathf.Abs(Mathf.Pow(getTime(z), 2) - getSpace(z).sqrMagnitude) < 0.01f) {
                    Debug.DrawRay(DrawEvent(z), Vector3.up, Color.green);
                }
            }
            Vector3 MCRFDisplacement;
            Vector4 newPos_Obs;
            var n = (int)properDuration*10;
            var properTime = 0f;
            for (int j = 0; j < n; j++) {
                properTime += properDuration / n;
                var t = (float)System.Math.Sinh(properTime * properAccel.magnitude) / properAccel.magnitude;
                MCRFDisplacement = properAccel.magnitude > Mathf.Epsilon
                    ? properAccel * (float)(System.Math.Sqrt(1 + t * t * properAccel.sqrMagnitude) - 1) / properAccel.sqrMagnitude
                    : Vector3.zero;
                newPos_Obs = currBoost * objOffset + currBoost * combineSpaceTime(MCRFDisplacement, t);
                Debug.DrawLine(DrawEvent(currPos_Obs), DrawEvent(newPos_Obs), properAccel.magnitude <= Mathf.Epsilon ? Color.white : i % 2 == 0 ? Color.red : Color.black);
                Debug.DrawRay(DrawEvent(currPos_Obs), Vector3.up, properAccel.magnitude <= Mathf.Epsilon ? Color.white : i % 2 == 0 ? Color.red : Color.black);
                currPos_Obs = newPos_Obs;
            }
            MCRFDisplacement = properAccel.magnitude > Mathf.Epsilon
                ? properAccel * (float)(System.Math.Sqrt(1 + MCRFDuration * MCRFDuration * properAccel.sqrMagnitude) - 1) / properAccel.sqrMagnitude
                : Vector3.zero;
            objOffset += combineSpaceTime(MCRFDisplacement, MCRFDuration);
            currPos_Obs = currBoost * objOffset;
            var addedVel = properAccel * MCRFDuration / (float)System.Math.Sqrt(1 + MCRFDuration * MCRFDuration * properAccel.sqrMagnitude);
            currBoost = currBoost * boost(-addedVel);
            objOffset = currBoost.inverse * currPos_Obs;
           
        }
        var futureDir = currBoost * combineSpaceTime(Vector3.zero, 1);
        Debug.DrawRay(DrawEvent(currPos_Obs), DrawEvent(futureDir) * 1000);
    }

    Matrix4x4 boost(Vector3 velocity) {
        if (velocity.magnitude == 0) return Matrix4x4.identity;
        var gamma = (float)(1.0 / System.Math.Sqrt(1.0 - velocity.sqrMagnitude));
        return new Matrix4x4(
            new Vector4(gamma,               -gamma * velocity.x,                                                -gamma * velocity.y,                                                -gamma * velocity.z),
            new Vector4(-gamma * velocity.x, 1f + (gamma - 1) * velocity.x * velocity.x / velocity.sqrMagnitude, (gamma - 1) * velocity.x * velocity.y / velocity.sqrMagnitude,      (gamma - 1) * velocity.x * velocity.z / velocity.sqrMagnitude),
            new Vector4(-gamma * velocity.y, (gamma - 1) * velocity.y * velocity.x / velocity.sqrMagnitude,      1f + (gamma - 1) * velocity.y * velocity.y / velocity.sqrMagnitude, (gamma - 1) * velocity.y * velocity.z / velocity.sqrMagnitude),
            new Vector4(-gamma * velocity.z, (gamma - 1) * velocity.z * velocity.x / velocity.sqrMagnitude,      (gamma - 1) * velocity.z * velocity.y / velocity.sqrMagnitude,      1f + (gamma - 1) * velocity.z * velocity.z / velocity.sqrMagnitude)
        );
    }

    Vector4 combineSpaceTime(Vector3 space, float time) {
        return new Vector4(time, space.x, space.y, space.z);
    }

    Vector3 DrawEvent(Vector4 e) { return getSpace(e) + new Vector3(0, 0, getTime(e)); }

    Vector3 getSpace(Vector4 e) { return new Vector3(e.y, e.z, e.w); }

    float getTime(Vector4 e) { return e.x; }
}
