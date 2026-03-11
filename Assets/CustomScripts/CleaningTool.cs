using UnityEngine;

public enum ToolType { Sponge, Squeegee }

public class CleaningTool : MonoBehaviour
{
    [Tooltip("Select what kind of tool this is.")]
    public ToolType typeOfTool;
}