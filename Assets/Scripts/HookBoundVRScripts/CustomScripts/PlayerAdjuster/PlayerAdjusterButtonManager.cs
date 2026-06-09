using UnityEngine;
using EasyXRSystems;
public class PlayerAdjusterButtonManager : XRButton
{
    public PlayerAdjuster Adjuster;

    public enum AdjustmentType { Jump, Run }
    public enum ButtonAction { Increase, Decrease }

    public AdjustmentType adjustmentType;
    public ButtonAction buttonAction;

    private float step = 0.25f; // Smaller, smoother increments
    private float minValue = 0.1f; // Prevent negative or zero multipliers
}
