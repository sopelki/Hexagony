using System.Collections;
using UnityEngine;

namespace UI
{
    public class CameraShaker : MonoBehaviour
    {
        private Vector3 originalPos;
        private Coroutine currentShake;

        public void Shake(float duration, float magnitude)
        {
            if (currentShake != null)
            {
                StopCoroutine(currentShake);
                transform.localPosition = originalPos;
            }
            currentShake = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            originalPos = transform.localPosition;
            var elapsed = 0.0f;

            while (elapsed < duration)
            {
                if (Time.timeScale > 0)
                    elapsed += Time.unscaledDeltaTime;

                var t = elapsed / duration;
                var currentMagnitude = magnitude * (1f - t);

                var x = Random.Range(-1f, 1f) * currentMagnitude;
                var y = Random.Range(-1f, 1f) * currentMagnitude;

                transform.localPosition = originalPos + new Vector3(x, y, 0);

                yield return null;
            }

            transform.localPosition = originalPos;
            currentShake = null;
        }
    }
}