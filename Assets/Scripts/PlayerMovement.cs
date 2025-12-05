using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerParams parameters => ScriptableObjects.instance.playerParams;
    [SerializeField] Vector3 velocityVector;
    [SerializeField] Vector3 pos;
    [SerializeField] Transform DEBUG_groundPos;
    [SerializeField] Transform DEBUG_wallPos;
    [SerializeField] PlayerCamera pcam;
    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private float playerHeight => (playerCollider != null) ? playerCollider.height : 2f;
    private float playerRadius => (playerCollider != null) ? playerCollider.radius : 0.5f;

    Vector3 getGroundNormal() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.01f, parameters.playerMovementColliders)) {
            return hit.normal;
        }
        return Vector3.up;
    }

    Vector3 computeMovementVector(Vector3 inputDirection) {
        return inputDirection.normalized * parameters.playerSpeed * Time.deltaTime;
    }

    bool tryGetGroundInDirection(Vector3 testFrom, out RaycastHit hit) {
        RaycastHit outHit = new RaycastHit();
        Vector3 diff = new Vector3(testFrom.x - transform.position.x, 0, testFrom.z - transform.position.z);
        for (int i = 0; i < 3; i++) {
            if (Physics.Raycast(testFrom, Vector3.down, out outHit, parameters.playerStepUpHeight + parameters.playerStepDownHeight, parameters.playerMovementColliders) 
                && (Vector3.Angle(outHit.normal, Vector3.up) < parameters.wallFloorAngleBarrier)) {
                hit = outHit;
                return true;
            } else {
                testFrom = new Vector3(transform.position.x + diff.x / 2, testFrom.y, transform.position.z + diff.z / 2);
            }
        }
        hit = outHit;
        return false;
    }

    /// Starts from the player's step up height, raycasts down until the step-down height to find the first valid position.
    /// Returns Vector3.zero on failure.
    Vector3 resolveGround(Vector3 movementVec) {
        Vector3 testFrom1 = new Vector3(transform.position.x, movementVec.y - 1f + parameters.playerStepUpHeight, movementVec.z);
        Vector3 testFrom2 = new Vector3(movementVec.x, movementVec.y - 1f + parameters.playerStepUpHeight, transform.position.z);
        RaycastHit hit;
        Vector3 ground = Vector3.zero;
        bool found = false;
        if (tryGetGroundInDirection(testFrom1, out hit)) {
            ground = hit.point;
            found = true;
        }
        if (tryGetGroundInDirection(testFrom2, out hit)) {
            ground.x = hit.point.x;
            if (!found) {
                ground = hit.point;
            } else {
                ground.y = (hit.point.y >= ground.y) ? hit.point.y : ground.y;
            }
        }
        return ground;
    }

    Vector3 resolveWalls(Vector3 ground) {
        // assert(ground != Vector3.zero);
        if (ground == Vector3.zero) {
            return transform.position;
        }
        Vector3 effectiveStand = Vector3.up * parameters.playerStepUpHeight;
        RaycastHit hit;
        Vector3 standPos = ground + new Vector3(0, playerHeight / 2 ,0);
        Vector3 movVec = standPos - transform.position;

        DEBUG_groundPos.transform.position = standPos;
        Vector3 first_pos = transform.position + new Vector3(0,parameters.playerStepUpHeight - playerRadius,0);
        Vector3 second_pos = transform.position + new Vector3(0,playerRadius,0);

        if (Physics.CapsuleCast(first_pos, second_pos, playerRadius, movVec.normalized, out hit, movVec.magnitude, parameters.playerMovementColliders, QueryTriggerInteraction.Ignore)) {
            DEBUG_wallPos.transform.position = hit.point;
            Vector3 moveable = movVec.normalized * (hit.distance - 0.01f);

            Vector3 remainingProj = Vector3.ProjectOnPlane(movVec, hit.normal);
            if (!Physics.CapsuleCast(first_pos, second_pos, playerRadius, remainingProj.normalized, out hit, remainingProj.magnitude, parameters.playerMovementColliders, QueryTriggerInteraction.Ignore)) {
                DEBUG_wallPos.transform.position = hit.point;

                moveable += remainingProj;
            } else
            {
                Vector3 extraMoveable = remainingProj.normalized * (hit.distance - 0.01f);
                moveable += extraMoveable;
            }

            return transform.position + moveable;
        }
        return ground + Vector3.up * 1f;
    }

    Vector3 resolveWallsDash(Vector3 ground) {
        Vector3 effectiveStand = Vector3.up * parameters.playerStepUpHeight;
        RaycastHit hit;
        Vector3 standPos = ground + new Vector3(0, playerHeight / 2, 0);
        Vector3 movVec = standPos - transform.position;

        DEBUG_groundPos.transform.position = standPos;
        Vector3 first_pos = transform.position + new Vector3(0,parameters.playerStepUpHeight - playerRadius,0);
        Vector3 second_pos = transform.position + new Vector3(0,playerRadius,0);
        if (Physics.CapsuleCast(first_pos, second_pos, playerRadius, movVec.normalized, out hit, movVec.magnitude, parameters.playerDashingColliders, QueryTriggerInteraction.Ignore)) {
            DEBUG_wallPos.transform.position = hit.point;
            Vector3 moveable = movVec.normalized * (hit.distance - 0.01f);

            return transform.position + moveable;
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


    void SpaceBoost(Vector3 direction) {
        Vector3 movVec = direction.normalized * parameters.playerDashDistance;
        int appliedSamples = parameters.playerDashSampleDensity;
        for (; appliedSamples > 0; appliedSamples--) {
            Vector3 testFrom = transform.position + movVec;
            testFrom.y += parameters.playerStepUpHeight - (playerHeight / 2);
            RaycastHit hit;
            if (Physics.Raycast(testFrom, Vector3.down, out hit, parameters.playerStepUpHeight + parameters.playerStepDownHeight, parameters.playerDashingColliders)
                && (Vector3.Angle(hit.normal, Vector3.up) < parameters.wallFloorAngleBarrier)) {
                transform.position = resolveWallsDash(hit.point);
                break;
            }
            movVec -= direction.normalized * parameters.playerDashDistance / parameters.playerDashSampleDensity;
        }
        for (; appliedSamples < parameters.playerDashSampleDensity; appliedSamples++) {
            Vector3 testFrom = transform.position + (direction.normalized * parameters.playerDashDistance / parameters.playerDashSampleDensity);
            testFrom.y += parameters.playerStepUpHeight - (playerHeight / 2);
            RaycastHit hit;
            if (Physics.Raycast(testFrom, Vector3.down, out hit, parameters.playerStepUpHeight + parameters.playerStepDownHeight, parameters.playerDashingColliders)
                && (Vector3.Angle(hit.normal, Vector3.up) < parameters.wallFloorAngleBarrier)) {
                transform.position = resolveWallsDash(hit.point);
            } else
            {
                return;
            }
        }
    }

    private Vector3 currentFacingDirection;
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
        currentFacingDirection = (mov == Vector3.zero) ? currentFacingDirection : mov;
        if (Input.GetKeyDown(KeyCode.Space)) {
            SpaceBoost(currentFacingDirection);
        } else {
            movePlayerSweep(mov);
        }
        pcam.PlayerCameraUpdate();
        transform.rotation = Quaternion.LookRotation(currentFacingDirection);
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }
}