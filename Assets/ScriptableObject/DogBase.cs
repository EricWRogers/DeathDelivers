using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DogBase", menuName = "Scriptable Objects/DogBase")]
public class DogBase : ScriptableObject
{
    public float speed = 0.5f;
    public float turnSpeed = 0.5f; 
    public float soulConsump = 0.5f;
    public float scytheTurnSpeed = 0.5f;
    public float startBoost = 10f;
}