using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using Logic.Monster;

namespace View
{
    public class MonsterViewManager : MonoBehaviour
    {
        [SerializeField] private Transform parent;

        private readonly Dictionary<MonsterModel, MonsterView> views = new();
        private MonsterSystem system;

        public void Initialize(MonsterSystem system)
        {
            this.system = system;

            system.OnMonsterCreated += HandleCreated;

            if (TickManager.Instance != null)
                TickManager.Instance.OnTick += HandleTick;
        }

        private void HandleCreated(MonsterModel model)
        {
            var prefab = model.Data.prefab;

            var go = Instantiate(
                prefab,
                model.WorldPosition,
                Quaternion.identity,
                parent
            );

            var view = go.GetComponent<MonsterView>();
            view.Initialize(model, model.Data.visualOffsetY);
            view.OnDeathAnimationFinished += HandleDeathAnimationFinished;

            views.Add(model, view);
        }
        
        private void HandleDeathAnimationFinished(MonsterModel model)
        {
            if (views.TryGetValue(model, out var view))
            {
                view.OnDeathAnimationFinished -= HandleDeathAnimationFinished;
                views.Remove(model);
            }
        }
        
        private void HandleTick()
        {
            foreach (var pair in views.ToList())
            {
                pair.Value.UpdateView();
            }
        }
        
        private void OnDestroy()
        {
            if (system != null)
                system.OnMonsterCreated -= HandleCreated;

            if (TickManager.Instance != null)
                TickManager.Instance.OnTick -= HandleTick;
        }
    }
}