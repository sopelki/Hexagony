using System.Collections.Generic;
using UnityEngine;
using Logic.Unit;

namespace View
{
    public class UnitViewManager : MonoBehaviour
    {
        [SerializeField] private Transform unitsParent;
        [SerializeField] private UnitBuffsViewManager buffView;

        private UnitSystem unitSystem;

        // Model → View
        private readonly Dictionary<UnitModel, UnitView> views = new();

        public void Initialize(UnitSystem system)
        {
            unitSystem = system;

            unitSystem.OnUnitCreated += HandleUnitCreated;
            unitSystem.OnUnitDied += HandleUnitDied;
        }

        private void OnDestroy()
        {
            if (unitSystem == null) return;

            unitSystem.OnUnitCreated -= HandleUnitCreated;
            unitSystem.OnUnitDied -= HandleUnitDied;
        }

        // ----------------------------
        // Создание юнита
        // ----------------------------
        private void HandleUnitCreated(UnitModel model)
        {
            var prefab = model.UnitData.unitPrefab;

            var go = Instantiate(prefab, model.WorldPosition, Quaternion.identity, unitsParent);

            var view = go.GetComponent<UnitView>();
            view.Initialize(model, buffView);

            views.Add(model, view);
        }

        // ----------------------------
        // Удаление юнита
        // ----------------------------
        private void HandleUnitDied(UnitModel model)
        {
            if (!views.TryGetValue(model, out var view))
                return;

            Destroy(view.gameObject);
            views.Remove(model);
        }

        // ----------------------------
        // Синхронизация позиции
        // ----------------------------
        private void Update()
        {
            foreach (var (model, view) in views)
            {
                view.SetPosition(model.WorldPosition);
            }
        }
    }
}