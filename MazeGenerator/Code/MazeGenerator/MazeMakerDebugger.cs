using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Flags]
public enum DebugFeatures
{
    None,
    Animation,
    ColoredFlooring,
    DisableFOW,
    NoClip,
    DisableWalkingSanityDrain,
    DisableAllSanityDrain,
}

public static class MazeMakerDebugger
{
    public static DebugFeatures features;
    public static bool isAnimationOn;
    public static bool isColoredFlooringOn;
    public static bool isDisabledFOWOn;
    public static bool isNoClipOn;
    public static bool isDisableWalkingSanityDrainOn;
    public static bool isDisableSanityDrainOn;

    public static void SetFeature(DebugFeatures feature)
    {
        switch (feature)
        {
            case DebugFeatures.Animation:
                isAnimationOn = !isAnimationOn;
                break;
            case DebugFeatures.ColoredFlooring:
                isColoredFlooringOn = !isColoredFlooringOn;
                break;
            case DebugFeatures.DisableFOW:
                isDisabledFOWOn = !isDisabledFOWOn;
                break;
            case DebugFeatures.NoClip:
                isNoClipOn = !isNoClipOn;
                break;
            case DebugFeatures.DisableWalkingSanityDrain:
                isDisableWalkingSanityDrainOn = !isDisableWalkingSanityDrainOn;
                break;
            case DebugFeatures.DisableAllSanityDrain:
                isDisableSanityDrainOn = !isDisableSanityDrainOn;
                break;
            default:
                break;
        }

        FlagsHelper.Set<DebugFeatures>(ref features, feature);
    }

    public static bool HasFeature(DebugFeatures feature)
    {
        switch (feature)
        {
            case DebugFeatures.Animation:
                return isAnimationOn;

            case DebugFeatures.ColoredFlooring:
                return isColoredFlooringOn;

            case DebugFeatures.DisableFOW:
                return isDisabledFOWOn;

            case DebugFeatures.NoClip:
                return isNoClipOn;

            case DebugFeatures.DisableWalkingSanityDrain:
                return isDisableWalkingSanityDrainOn;

            case DebugFeatures.DisableAllSanityDrain:
                return isDisableSanityDrainOn;
        }

        return FlagsHelper.IsSet<DebugFeatures>(features, feature);
    }
}
