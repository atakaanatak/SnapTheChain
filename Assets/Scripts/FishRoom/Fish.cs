using UnityEngine;

public class Fish : BasicProp
{
    [Header("Fish Specific Settings")]
    [Tooltip("Optional audio source triggered when the fish is grabbed.")]
    public AudioSource fishSlapAudio;
}