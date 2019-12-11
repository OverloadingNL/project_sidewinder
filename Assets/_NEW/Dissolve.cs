using System.Collections;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [SerializeField] float dissolveDelay = 2f;
    MeshRenderer[] renderersInChildren;
    MeshRenderer rendererParent;
    int childCount;

    void Start()
    {
        renderersInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
        childCount = renderersInChildren.Length;
        rendererParent = gameObject.GetComponent<MeshRenderer>();
        CheckHierarchy();
    }

    void CheckHierarchy()
    {
        if (rendererParent == null)
        {
            foreach (MeshRenderer renderer in renderersInChildren)
            {
                DissolveAndDestroy(renderer);
            }
        }
        else
        {
            DissolveAndDestroy(rendererParent);
        }
    }

    private void DissolveAndDestroy(MeshRenderer renderer)
    {
        Color startLerp = renderer.material.color;
        Color targetLerp = renderer.material.color;
        targetLerp.a = 0f;
        StartCoroutine(Lerp_MeshRenderer_Color(renderer, dissolveDelay, startLerp, targetLerp));
    }

    private IEnumerator Lerp_MeshRenderer_Color(MeshRenderer target_MeshRender, float lerpDuration, Color startLerp, Color targetLerp)
    {
        float lerpStart_Time = Time.time;
        float lerpProgress;
        bool lerping = true;
        while (lerping)
        {
            yield return new WaitForEndOfFrame();

            lerpProgress = Time.time - lerpStart_Time;
            if (target_MeshRender != null)
            {
                target_MeshRender.material.color = Color.Lerp(startLerp, targetLerp, lerpProgress / lerpDuration);
            }
            else
            {
                lerping = false;
            }


            if (lerpProgress >= lerpDuration)
            {
                lerping = false;
            }
        }

        Object.Destroy(target_MeshRender.gameObject);
        childCount--;

        if (childCount == 0)
        {
            Object.Destroy(gameObject);
        }

        yield break;
    }
}
