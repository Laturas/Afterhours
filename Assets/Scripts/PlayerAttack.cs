using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator playerAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerAnimator.Play("Attack");
        }
    }
}
