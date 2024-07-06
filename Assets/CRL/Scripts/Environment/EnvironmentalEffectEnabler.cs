using UnityEngine;

/// <summary>
/// Enable these game objects when the scene starts.
/// Workaround for VFX issues in unity editor.
/// </summary>
public class EnvironmentalEffectEnabler : MonoBehaviour
{
    #region Editor Fields

    [SerializeField] private GameObject[] _objectsToEnable;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        foreach (var o in _objectsToEnable)
        {
            o.SetActive(true);
        }
    }

    #endregion
}
