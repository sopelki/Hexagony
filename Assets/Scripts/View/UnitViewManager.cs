using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logic.Unit;

namespace View
{
    public class UnitViewManager : MonoBehaviour
    {
        [SerializeField] private Transform unitsParent;
        [SerializeField] private UnitBuffsViewManager buffView;

        private UnitSystem unitSystem;

        private readonly Dictionary<UnitModel, UnitView> views = new();

        public void Initialize(UnitSystem system)
        {
            unitSystem = system;

            unitSystem.OnUnitCreated += HandleUnitCreated;
        }

        private void OnDestroy()
        {
            if (unitSystem == null) return;

            unitSystem.OnUnitCreated -= HandleUnitCreated;
        }

        private void HandleUnitCreated(UnitModel model)
        {
            var prefab = model.UnitData.unitPrefab;

            var go = Instantiate(prefab, model.WorldPosition, Quaternion.identity, unitsParent);

            var view = go.GetComponent<UnitView>();
            view.Initialize(model, buffView);
            view.OnDeathAnimationFinished += HandleDeathAnimationFinished;

            views.Add(model, view);
        }
        private void HandleDeathAnimationFinished(UnitModel model)
        {
            if (views.TryGetValue(model, out var view))
            {
                view.OnDeathAnimationFinished -= HandleDeathAnimationFinished;
                views.Remove(model);
            }
            
            unitSystem.RemoveUnit(model);
        }

        private void Update()
        {
            foreach (var pair in views.ToList())
            {
                pair.Value.UpdateView();
            }
        }
    }
}