using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FadeTMProText : MonoBehaviour
{
    TextMeshProUGUI textmeshPro;
    Color32 startColor;
    byte startAlpha;
    byte currentAlpha;

    void Awake()
    {
        textmeshPro = GetComponent<TextMeshProUGUI>();
        startAlpha = textmeshPro.faceColor.a;
    }

    public void Fade()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        for (currentAlpha = startAlpha; currentAlpha > 0; currentAlpha--)
        {
            Color32 _color = startColor;
            _color.a = currentAlpha;
            textmeshPro.faceColor = _color;
            Debug.Log("Tutorial alpha: " + currentAlpha);
            
            yield return new WaitForSeconds(0.01f);
        }
    }
}
