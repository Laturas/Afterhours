using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerParams parameters => ScriptableObjects.instance.playerParams;
    [SerializeField] Vector3 velocityVector;
    [SerializeField] Vector3 pos;
    [SerializeField] Transform DEBUG_groundPos;
    [SerializeField] Transform DEBUG_wishPos;
    [SerializeField] Transform DEBUG_wallPos;
    [SerializeField] PlayerCamera pcam;
    private Rigidbody rb;
    float playerHeight = 1.0f;
    // [SerializeField] Collider feetCollider;

    /// Step 1: Project along the ground
    /// 
    ///             .__ Max step up height
    ///             |
    /// Player ---->|___ new floor
    /// _____floor  |
    ///             V Max step down height
    /// 
    /// Step 2: Raycast for colliders (wall detection)
    /// Step 3: Stop the player short at the nearest wall/pit
    // void movePlayer(Vector3 inputDirection)
    // {
    //     pos = transform.position;
    //     // Step 1.
    //     // TODO: Implement a real velocity system
    //     velocityVector = inputDirection.normalized * Time.deltaTime * parameters.playerSpeed;
    //     RaycastHit hit;
    //     if (!Physics.Raycast(pos, Vector3.down, out hit, 1f, 1 << 0)) {
    //         return;
    //     }
    //     Vector3 ground = hit.point;
    //     DEBUG_groundPos.position = ground;

    //     Vector3 stepCheckerOrigin = ground + velocityVector + (Vector3.up * parameters.playerStepUpHeight);
    //     Vector3 wishPosition = pos;
    //     // Does the ray intersect any objects excluding the player layer
    //     if (Physics.Raycast(stepCheckerOrigin, Vector3.down, out hit, parameters.playerStepUpHeight + parameters.playerStepDownHeight, 1 << 0)) {
    //         Debug.DrawRay(stepCheckerOrigin, hit.point - stepCheckerOrigin, Color.green); 
    //         wishPosition = hit.point + (Vector3.up * 1f);
    //     }
    //     Vector3 checkFrom = ground + (Vector3.up * parameters.playerStepUpHeight); // Checkfrom distance
    //     Vector3 checkFromShell = checkFrom + (checkFrom.normalized * 0.5f); // Now we need to cast it out from the shell of the player
    //     Vector3 movementVec = checkFromShell - wishPosition; 

    //     Debug.DrawRay(checkFrom, movementVec, Color.red);
    //     // Shoot a raycast from the player hitbox edge outward in the direction of the movemen
    //     if (Physics.Raycast(checkFrom, movementVec, out hit, movementVec.magnitude, 1 << 0)) {
    //         wishPosition = (movementVec.normalized * 0.5f) - hit.point;
    //     }
    //     Debug.DrawRay(checkFrom, wishPosition - transform.position, Color.blue); 
    //     transform.position = wishPosition;
    // }

    Vector3 getGroundNormal() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.01f, 1 << 0)) {
            return hit.normal;
        }
        return Vector3.up;
    }

    [SerializeField] RaycastHit[] results = new RaycastHit[25];
    void movePlayerPhysicsBased(Vector3 inputDirection) {
        Vector3 normal = getGroundNormal();

        Vector3 projVec = Vector3.ProjectOnPlane(inputDirection, normal).normalized;

        // Vector3 capsuleCastOrigin = transform.position + (transform.up * parameters.playerStepUpHeight);
        Vector3 velVec = parameters.playerSpeed * inputDirection * Time.deltaTime; // Velocity needs to be scaled to account for framerate.
        // Vector3 stopAtPosition = velVec;
        // RaycastHit hit;
        // if (Physics.CapsuleCast(transform.position + transform.up * playerHeight, transform.position - (-transform.up * parameters.playerStepUpHeight), 0.5f, projVec, velVec.magnitude, 1 << 0, QueryTriggerInteraction.Ignore)) {
        //     // (hit.point - velVec * 0.5f);
        //     Vector3 projected = Vector3.Project(hit.point - transform.position, (velVec - transform.position).normalized);
        //     stopAtPosition = (projected - (0.5f * projected.normalized)) + transform.position; // Account for radius
        // }
        // transform.position = stopAtPosition;

        // RaycastHit hit; Physics.CapsuleCast();


        // Debug.DrawRay(transform.position, velVec, Color.blue); 
        rb.linearVelocity = velVec;
        // Vector3 currentVelocity = rb.linearVelocity;
        // Vector3 velocityChange = velVec - currentVelocity;

        // rb.AddForce(velocityChange * parameters.playerAcceleration, ForceMode.Acceleration);
        // Debug.DrawRay(Vector3.up + transform.position, velVec, Color.blue); 

        // int colliderCount = Physics.CapsuleCastNonAlloc(Vector3.up + transform.position, transform.position - Vector3.down, 0.5f, inputDirection, results, parameters.playerSpeed, 1 << 0, QueryTriggerInteraction.Ignore); 
        // for (int i = 0; i < colliderCount; i++) {

        // }
    }

    Vector3 computeMovementVector(Vector3 inputDirection) {
        return inputDirection.normalized * parameters.playerSpeed * Time.deltaTime;
    }

    /// Starts from the player's step up height, raycasts down until the step-down height to find the first valid position.
    /// Returns Vector3.zero on failure.
    /// 
    /// TODO: Capsulecast better?
    Vector3 resolveGround(Vector3 movementVec) {
        Vector3 testFrom = new Vector3(movementVec.x, movementVec.y - 1f + parameters.playerStepUpHeight, movementVec.z);
        RaycastHit hit;
        if (!Physics.Raycast(testFrom, Vector3.down, out hit, parameters.playerStepUpHeight + parameters.playerStepDownHeight, 1 << 0)) {
            return Vector3.zero; // TODO: Binary search
        }
        Vector3 ground = hit.point;
        return ground;
    }

    void assert(bool boolean) {
        if (!boolean) Debug.LogError("Assertion failed");
    }

    Vector3 resolveWalls(Vector3 ground) {
        assert(ground != Vector3.zero);
        if (ground == Vector3.zero) {
            return transform.position;
        }
        Vector3 effectiveStand = Vector3.up * parameters.playerStepUpHeight;
        // Vector3 capsuleCastFrom = transform.position + effectiveStand;
        // Vector3 capsuleCastTo = standPos + Vector3.up * 1f + effectiveStand;
        RaycastHit hit;
        float playerRadius = 0.5f;
        float playerHeight = 1f;
        Vector3 standPos = ground + new Vector3(0,playerHeight,0);
        // Player's center offset to the stepUpHeight
        // Vector3 p1 = transform.position + (Vector3.up * -playerHeight/2 * playerRadius) + effectiveStand;
        // Vector3 p2 = p1 + Vector3.up * playerHeight;
        // Debug.Log("p1: " + p1 + "p2: " + p2);

        // Cast character controller shape 10 meters forward to see if it is about to hit anything.
        // if (Physics.CapsuleCast(p1, p2, playerRadius, standPos - transform.position, out hit, (standPos - transform.position).magnitude, 1 << 0, QueryTriggerInteraction.Ignore)) Debug.Log(hit.point);
        DEBUG_groundPos.transform.position = standPos;
        if (Physics.Raycast(transform.position, standPos - transform.position, out hit, (standPos - transform.position).magnitude, 1 << 0, QueryTriggerInteraction.Ignore)) {
            DEBUG_wallPos.transform.position = hit.point;
            return transform.position;
        }
        return ground + Vector3.up * 1f;
    }

    void movePlayerSweep(Vector3 inputDirection) {
        Vector3 movementVec = computeMovementVector(inputDirection);
        Vector3 wishPos = movementVec + transform.position;
        Vector3 ground = resolveGround(wishPos);
        Vector3 pos = resolveWalls(ground);
        transform.position = pos;
    }

    void Update()
    {
        Vector3 mov = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            mov += new Vector3(1f, 0f, 1f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            mov += new Vector3(-1f, 0f, 1f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            mov += new Vector3(-1f, 0f, -1f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            mov += new Vector3(1f, 0f, -1f);
        }
        // transform.position += mov.normalized * Time.deltaTime * parameters.playerSpeed;
        movePlayerSweep(mov);
        pcam.PlayerCameraUpdate();
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
}