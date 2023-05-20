#if UNITY_EDITOR
using System.Linq;

using UnityEngine;

using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorTags
{
    public static string[] GetTags(this AnimatorController anim)
        => anim.layers.SelectMany(l => l.stateMachine.states.Select(s => s.state.tag)).ToArray();

    public static string DrawValues(this AnimatorController anim, string currentValue, Rect position, string label)
    {
        var currentInputIndex = 0;
        var tagsNames = anim.GetTags();
        for (var i = 0; i < tagsNames.Length; i++)
            if (tagsNames[i] == currentValue)
                currentInputIndex = i;
        return tagsNames[EditorGUI.Popup(position, label, currentInputIndex, tagsNames)];
    }
}
#endif