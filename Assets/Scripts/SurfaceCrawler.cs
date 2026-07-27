using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class SurfaceCrawler : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 90f;
    public float alignmentSpeed = 15f;

    [Header("Crawling & Detection")]
    public LayerMask walkableLayers;
    public float distanceToFeet = 1.0f; // Center of body down to feet
    public float downRayLength = 3.0f;
    public float forwardRayLength = 2.0f;
    public float pivotOffsetY = 0.5f;

    [Header("Anti-Clipping Settings")]
    // 1. Pushes the player slightly off the wall surface to stop mesh intersection
    public float wallSafetyBuffer = 0.15f;
    // 2. The thickness of the box radar checking for walls (matches character width)
    public float boxRadarSize = 0.5f;

    private Rigidbody rb;
    private Collider playerCollider;
    private Vector3 currentNormal = Vector3.up;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (playerCollider != null)
        {
            distanceToFeet = playerCollider.bounds.extents.y;
        }
    }

    void FixedUpdate()
    {
        DetectSurface();
        HandleMovement();
    }

    void DetectSurface()
    {
        RaycastHit hit;
        bool surfaceFound = false;
        Vector3 originPoint = transform.position + (transform.up * pivotOffsetY);

        // 3. BOX RADAR: We use a BoxCast instead of a thin ray so your shoulders/sides catch walls
        Vector3 boxHalfExtents = new Vector3(boxRadarSize, boxRadarSize, 0.1f);

        // CHECK FORWARD (Walls)
        if (Physics.BoxCast(originPoint, boxHalfExtents, transform.forward, out hit, transform.rotation, forwardRayLength, walkableLayers))
        {
            if (hit.collider != playerCollider)
            {
                currentNormal = hit.normal;
                surfaceFound = true;

                // Added wallSafetyBuffer so your feet hit, but your body never enters the wall mesh
                Vector3 targetPos = hit.point + (currentNormal * (distanceToFeet + wallSafetyBuffer));
                rb.MovePosition(Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * alignmentSpeed));
            }
        }

        // CHECK DOWNWARD (Floor/Ceilings)
        if (!surfaceFound)
        {
            if (Physics.BoxCast(originPoint, boxHalfExtents, -transform.up, out hit, transform.rotation, downRayLength, walkableLayers))
            {
                if (hit.collider != playerCollider)
                {
                    currentNormal = hit.normal;
                    surfaceFound = true;

                    Vector3 targetPos = hit.point + (currentNormal * (distanceToFeet + wallSafetyBuffer));
                    rb.MovePosition(Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * alignmentSpeed));
                }
            }
        }

        // Airborne Fallback
        if (!surfaceFound)
        {
            currentNormal = Vector3.up;
            rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
        }

        // Smoothly match rotation
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, currentNormal) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * alignmentSpeed));
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        transform.Rotate(0, turnInput * turnSpeed * Time.fixedDeltaTime, 0);

        Vector3 moveDirection = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }



}
