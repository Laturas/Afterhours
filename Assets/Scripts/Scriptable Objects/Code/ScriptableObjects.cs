using UnityEngine;

public class ScriptableObjects : MonoBehaviour
{
    public PlayerParams playerParams;
    public static ScriptableObjects instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
}
