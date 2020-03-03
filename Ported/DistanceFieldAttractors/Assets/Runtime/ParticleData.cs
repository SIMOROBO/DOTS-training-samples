using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public class ParticleData : IComponentData
{
	public Vector3 position;
	public Vector3 velocity;
	public Vector4 color;
}
