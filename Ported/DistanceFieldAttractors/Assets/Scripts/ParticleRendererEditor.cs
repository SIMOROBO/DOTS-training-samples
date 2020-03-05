using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParticleRenderer))]
public class ParticleRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var renderer = target as ParticleRenderer;
        if(renderer == null)
        {
            return;
        }

        if(GUILayout.Button("Restart"))
        {
            renderer.Restart();
        }
    }
}
