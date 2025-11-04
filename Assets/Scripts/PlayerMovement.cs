using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerParams parameters => ScriptableObjects.instance.playerParams;

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
        transform.position += mov.normalized * Time.deltaTime * parameters.playerSpeed;
    }
    
    void Start()
    {
        
    }
}