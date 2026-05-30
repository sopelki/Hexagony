using System.Collections.Generic;
using System.Linq;
using Logic.Trap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public class TrapViewManager : MonoBehaviour
    {
        private readonly Dictionary<TrapModel, TrapView> views = new();
        private Field.Field field;
        private TrapsModel model;
        private Tilemap tilemap;

        private void OnDestroy()
        {
            if (model != null)
            {
                model.OnTrapAdded -= HandleTrapAdded;
                model.OnTrapRemoved -= HandleTrapRemoved;
            }
        }

        public void Initialize(TrapsModel trapsModel, Field.Field field, Tilemap tilemap)
        {
            model = trapsModel;
            this.field = field;
            this.tilemap = tilemap;

            model.OnTrapAdded += HandleTrapAdded;
            model.OnTrapRemoved += HandleTrapRemoved;
        }

        private void HandleTrapAdded(TrapModel trap)
        {
            var pos = trap.Hexes.Select(h => tilemap.GetCellCenterWorld(field.GetHex(h).offset)).ToList();
            var finalPos = new Vector3(pos.Average(p => p.x), pos.Min(p => p.y) - tilemap.cellSize.y * 0.5f, -0.1f);
            var viewGo = Instantiate(trap.Data.viewPrefab, finalPos, Quaternion.identity);
            var view = viewGo.GetComponent<TrapView>();
            view.Initialize(trap.Data.viewPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
            views.Add(trap, view);
        }

        private void HandleTrapRemoved(TrapModel trap)
        {
            if (views.Remove(trap, out var view))
                view.AnimateAndDestroy();
        }

        public void DestroyAllTraps()
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