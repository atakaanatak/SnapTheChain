using UnityEngine;

public class GravityZone : MonoBehaviour
{
    [Header("Bu Alanın Yerçekimi")]
    [Tooltip("Normal dünya yerçekimi -9.81'dir. Ağırlaştırmak için -15 veya -20 yap.")]
    public float zoneGravityY = -15f;

    void OnTriggerEnter(Collider other)
    {
        // BUM! Alana giren şey RICHARD ise yerçekimini sike sike bük!
        if (other.CompareTag("Player"))
        {
            Physics.gravity = new Vector3(0f, zoneGravityY, 0f);
            Debug.Log($"[Fizik Alanı] Richard {gameObject.name} alanına girdi! Yerçekimi {zoneGravityY} olarak kilitlendi!");
        }
    }
}