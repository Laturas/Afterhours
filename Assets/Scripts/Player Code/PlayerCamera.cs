using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform defaultLookAt;
    [SerializeField] private Transform defaultMoveTo;
    [SerializeField] private Transform alternateLookAt = null;
    [SerializeField] private Transform alternateMoveTo = null;

    private Transform moveTo => (alternateMoveTo == null) ? defaultMoveTo : alternateMoveTo;
    private Transform lookAt => (alternateLookAt == null) ? defaultLookAt : alternateLookAt;
    private PlayerParams parameters => ScriptableObjects.instance.playerParams;

    public void PlayerCameraUpdate()
    {
        moveTo.transform.position = player.transform.position + Vector3.up * parameters.defaultCameraHeight;
        Quaternion rotation = Quaternion.AngleAxis(parameters.defaultCameraRotAngle, Vector3.up);
        moveTo.transform.position += (rotation * Vector3.forward).normalized * parameters.defaultCameraDistance;
        lookAt.position = player.transform.position;

        transform.position = moveTo.position;
        transform.LookAt(lookAt);
    }

    private Camera thisCam;

    void Awake() => thisCam = GetComponent<Camera>();


    void Start()
    {
        defaultLookAt = new GameObject("LookAt").transform;
        defaultLookAt.position = player.transform.position;
        defaultMoveTo = new GameObject("MoveTo").transform;
        defaultMoveTo.position = player.transform.position;

        defaultLookAt.parent = transform.parent;
        defaultMoveTo.parent = transform.parent;

        moveTo.transform.position = transform.position + Vector3.up * parameters.defaultCameraHeight;
        moveTo.transform.position += Vector3.forward * parameters.defaultCameraDistance;
    }
}
