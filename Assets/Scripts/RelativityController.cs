using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativityController : MonoBehaviour
{
    public double ObserverTime;
    public double InitialProperTime;
    public Vector3d InitialVelocity;
    public List<Vector4> Accelerations;

    private Relativity[] scripts;
    private Material mat;
    // Start is called before the first frame update
    void Start()
    {
        scripts = gameObject.GetComponentsInChildren<Relativity>();
        mat = GetComponent<Renderer>()?.material;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var script in scripts)
        {
            script.ObserverTime = ObserverTime;
            script.InitialProperTime = InitialProperTime;
            script.InitialVelocity = InitialVelocity;
            script.Accelerations = Accelerations;
        }
        mat?.SetInt("_AccelCount", Accelerations.Count);
        if (Accelerations.Count != 0)
        {
            mat?.SetVectorArray("_Accelerations", Accelerations.ToArray());
        } else
        {
            mat?.SetVectorArray("_Accelerations", new Vector4[1]);
        }
        mat?.SetVector("_Velocity", InitialVelocity.Vector3());
        mat?.SetFloat("_ProperTime", (float)InitialProperTime);
        Shader.SetGlobalFloat("_ObserverTime", (float)ObserverTime);
    }
}
