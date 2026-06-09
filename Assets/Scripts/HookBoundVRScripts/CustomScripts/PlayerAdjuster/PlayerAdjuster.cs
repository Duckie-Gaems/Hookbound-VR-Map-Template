using UnityEngine;
using TMPro;
public class PlayerAdjuster : MonoBehaviour
{
    [Header("Player Adjuster Setter Settings")]
    public bool ButtonManager = false;
    public bool TextDisplay = false;
    public bool UseOnStart = true;

    [Header("Player Adjuster Settings")]
    [Tooltip("1f = Normal jump. Higher values increase jump strength.")]
    public float JumpMultiplier = 1f;

    [Tooltip("1f = Normal run speed. Higher values increase speed.")]
    public float RunMultiplier = 1f;

    [Tooltip("Maximum allowed multiplier to prevent extreme physics.")]
    [Range(1f, 8f)]
    public float MultiCap = 5f;
    public TextMeshPro JumpMultiplierText;
    public TextMeshPro RunMultiplierText;
}
