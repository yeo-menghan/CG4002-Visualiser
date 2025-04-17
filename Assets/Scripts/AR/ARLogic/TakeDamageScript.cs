using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;

public class TakeDamageScript : MonoBehaviour
{
    public float intensity = 0;

    PostProcessVolume _volume;
    Vignette _vignette;

    void Start()
    {
        _volume = GetComponent<PostProcessVolume>();

        _volume.profile.TryGetSettings<Vignette>(out _vignette);

        if(!_vignette)
        {
            Debug.Log("TakeDamageScript: Error, vignette empty");
        }
        else
        {
            _vignette.enabled.Override(false);
        }
    }

    public void StartDamageEffect()
    {
        StartCoroutine(TakeDamageEffect());
    }

    private IEnumerator TakeDamageEffect()
    {
        intensity = 0.6f;
        _vignette.enabled.Override(true);
        _vignette.intensity.Override(0.6f);

        yield return new WaitForSeconds(0.6f);

        while (intensity > 0)
        {
            intensity -= 0.03f;

            if (intensity < 0)
            {
                intensity = 0;
            }

            _vignette.intensity.Override(intensity);

            yield return new WaitForSeconds(0.1f);
        }

        _vignette.enabled.Override(false);
        yield break;
    }
}
