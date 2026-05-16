using UnityEngine;

public class BasicProp : HoldableObject
{
    [Header("Prop Specific Settings")] public float dropForce = 2f;

    [Header("Layer Settings")] public string worldLayerName = "Interactables";
    public string heldLayerName = "ViewmodelInteractables";

    private Rigidbody rb;
    private Collider propCollider;
    private Vector3 originalWorldScale;

    private bool needsStabilization = false;
    private float stabilizationTimer = 0f;
    private const float GRACE_PERIOD = 2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        propCollider = GetComponent<Collider>();

        if (!isHeld && rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    protected override void GrabObject()
    {
        originalWorldScale = transform.lossyScale;

        if (propCollider != null) propCollider.enabled = false;
        if (rb != null) rb.isKinematic = true;

        base.GrabObject();

        ChangeLayerObject(gameObject, heldLayerName);
        needsStabilization = false;
    }

    protected override void DropObject()
    {
        base.DropObject();

        transform.localScale = originalWorldScale;

        Transform playerCam = GameObject.Find("PlayerCamera")?.transform;

        if (playerCam != null)
        {
            int obstacleLayerMask = LayerMask.GetMask("Default");

            if (Physics.Linecast(playerCam.position, transform.position, out RaycastHit hit, obstacleLayerMask))
            {
                transform.position = hit.point + (playerCam.position - hit.point).normalized * 0.15f;
                Debug.Log($"[BasicProp] {gameObject.name} repositioned to prevent clipping.");
            }
        }

        ChangeLayerObject(gameObject, worldLayerName);

        if (propCollider != null) propCollider.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            if (playerCam != null)
            {
                rb.AddForce(playerCam.forward * dropForce, ForceMode.Impulse);
            }
        }

        needsStabilization = true;
        stabilizationTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();
        if (needsStabilization && !isHeld && rb != null)
        {
            stabilizationTimer += Time.deltaTime;

            if (stabilizationTimer > GRACE_PERIOD)
            {
                if (rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    needsStabilization = false;
                }
            }
        }
    }
}