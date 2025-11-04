using UnityEngine;

public class GodSingleton : MonoBehaviour
{
    public static GodSingleton instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }
}
