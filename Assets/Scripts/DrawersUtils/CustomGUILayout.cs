#if UNITY_EDITOR
using UnityEngine;

public static class CustomGUILayout
{
    private const float PROPERTY_HEIGHT = 20f;

    public static Rect GetPosition(this Rect targetPosition, ref float propertyHeight, int lines = 1)
    {
        var newPosition = targetPosition;

        newPosition.y += propertyHeight;
        newPosition.height = lines * PROPERTY_HEIGHT;

        propertyHeight += newPosition.height;

        return newPosition;
    }
}
#endif