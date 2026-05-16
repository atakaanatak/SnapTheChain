using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelEntrance : MonoBehaviour
{
    [Header("Entrance Settings")] public Image blackCurtain;
    public float fadeInDuration = 1.5f;

    private void Start()
    {
        if (blackCurtain != null)
        {
            StartCoroutine(OpenCurtainRoutine());
        }
    }

    private IEnumerator OpenCurtainRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        float elapsed = 0f;
        Color c = blackCurtain.color;
        c.a = 1f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            blackCurtain.color = c;
            yield return null;
        }

        c.a = 0f;
        blackCurtain.color = c;

        blackCurtain.raycastTarget = false;
    }
}