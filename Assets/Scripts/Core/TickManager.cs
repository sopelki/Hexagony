using System;
using UnityEngine;

namespace Core
{
    public class TickManager : MonoBehaviour
    {
        public static TickManager Instance;

        public float tickInterval = 0.02f;
        private float timer;

        public event Action OnTick;

        private void Awake() => Instance = this;

        private void Update()
        {
            timer += Time.deltaTime;

            while (timer >= tickInterval)
            {
                timer -= tickInterval;
                OnTick?.Invoke();
            }
        }

        private void OnDestroy()
        {
            OnTick = null;
            if (Instance == this)
                Instance = null;
        }
    }
}