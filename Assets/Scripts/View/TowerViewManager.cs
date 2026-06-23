using System.Collections.Generic;
using System.Linq;
using Logic.Tower;
using UnityEngine;

namespace View
{
    public class TowerViewManager : MonoBehaviour
    {
        private readonly Dictionary<TowerModel, TowerView> views = new();
        private TowersModel model;
        public static TowerViewManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (model != null)
                model.OnChanged -= HandleTowerAdded;

            foreach (var pair in views)
                pair.Key.OnLevelUp -= pair.Value.SetLevel;
        }

        public void Initialize(TowersModel modelToInitialize)
        {
            model = modelToInitialize;
            model.OnChanged += HandleTowerAdded;

            foreach (var tower in model.Towers)
                HandleTowerAdded(tower);
        }

        public void SyncWithModel()
        {
            foreach (var tower in model.Towers)
            {
                if (!views.ContainsKey(tower))
                    HandleTowerAdded(tower);
            }
        }

        private void HandleTowerAdded(TowerModel towerModel)
        {
            var viewGo = Instantiate(towerModel.Data.viewPrefab, towerModel.WorldPosition, Quaternion.identity);
            var view = viewGo.GetComponent<TowerView>();

            if (view != null)
            {
                view.Initialize(towerModel.Data.viewPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                view.SetLevel(towerModel.Level);
                towerModel.OnLevelUp += view.SetLevel;
                views.Add(towerModel, view);
            }

            Debug.Log($"TowerView created and linked for tower at {towerModel.GridPosition}");
        }

        public void DestroyAllTowers()
        {
            foreach (var pair in views.Where(pair => pair.Value != null))
            {
                pair.Key.OnLevelUp -= pair.Value.SetLevel;
                Destroy(pair.Value.gameObject);
            }
            views.Clear();
        }

        public TowerView GetViewAtCell(Vector3Int cellPos)
        {
            var towerModel = views.Keys.FirstOrDefault(t => t.GridPosition == cellPos);
            return towerModel != null ? views[towerModel] : null;
        }
    }
}