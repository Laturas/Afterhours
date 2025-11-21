using UnityEngine;

[CreateAssetMenu(fileName = "PlayerParams", menuName = "Scriptable Objects/PlayerParams")]
public class PlayerParams : ScriptableObject
{
    [Header("Camera")]
    public float defaultCameraHeight;
    public float defaultCameraDistance;
    public float defaultCameraRotAngle;
    public float defaultCameraLerpSpeed;

    [Header("Player Movement")]
    public float playerSpeed;
    public float playerAcceleration;
    public float playerStepUpHeight;
    public float playerStepDownHeight;



}
