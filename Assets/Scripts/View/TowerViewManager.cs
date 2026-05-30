using System.Collections.Generic;
using Logic.Tower;
using UnityEngine;

namespace View
{
    public class TowerViewManager : MonoBehaviour
    {
        private readonly Dictionary<TowerModel, TowerView> views = new();
        private TowersModel model;

        private void OnDestroy()
        {
            if (model != null)
                model.OnChanged -= HandleTowerAdded;
        }

        public void Initialize(TowersModel modelToInitialize)
        {
            model = modelToInitialize;
            model.OnChanged += HandleTowerAdded;
        }

        private void HandleTowerAdded(TowerModel towerModel)
        {
            var viewGo = Instantiate(towerModel.Data.viewPrefab, towerModel.WorldPosition, Quaternion.identity);
            var view = viewGo.GetComponent<TowerView>();

            views.Add(towerModel, view);

            Debug.Log($"TowerView created for tower at {towerModel.GridPosition}");
        }

        public void DestroyAllTowers()
        {
            foreach (var view in views.Values)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }
            views.Clear();
        }
    }
}