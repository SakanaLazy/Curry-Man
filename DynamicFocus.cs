using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicFocus : MonoBehaviour
{
    public Transform player;
    public Volume volume;
    private DepthOfField dof;

    void Start()
    {
        if (volume.profile.TryGet(out dof))
            Debug.Log("Depth of Field linked!");
    }

    void Update()
    {
        if (dof != null && player != null)
        {
            dof.focusDistance.value = Vector3.Distance(transform.position, player.position);
        }
    }
}
