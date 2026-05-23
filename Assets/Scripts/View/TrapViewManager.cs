using System.Linq;

namespace View
{
    using UnityEngine;
    using Logic.Trap;
    using System.Collections.Generic;
    using UnityEngine.Tilemaps;

    namespace View
    {
        public class TrapViewManager : MonoBehaviour
        {
            private TrapsModel model;
            private Field.Field field;
            private Tilemap tilemap;

            private readonly Dictionary<TrapModel, TrapView> views = new();

            public void Initialize(TrapsModel trapsModel, Field.Field field, Tilemap tilemap)
            {
                model = trapsModel;
                this.field = field;
                this.tilemap = tilemap;

                model.OnTrapAdded += HandleTrapAdded;
                model.OnTrapRemoved += HandleTrapRemoved; 
            }

            private void OnDestroy()
            {
                if (model != null)
                {
                    model.OnTrapAdded -= HandleTrapAdded;
                    model.OnTrapRemoved -= HandleTrapRemoved;
                }
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
                if (views.TryGetValue(trap, out var view))
                {
                    views.Remove(trap);
                    view.AnimateAndDestroy();
                }
            }
        }
    }
}