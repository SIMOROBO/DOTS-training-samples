using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComputeParticleManager))]
public class ComputeParticleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var renderer = target as ComputeParticleManager;
        if(renderer == null)
        {
            return;
        }

        //if(GUILayout.Button("Restart"))
        //{
        //    renderer.Restart();
        //}
    }
}
