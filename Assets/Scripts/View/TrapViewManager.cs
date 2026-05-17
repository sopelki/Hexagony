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
            }

            private void OnDestroy()
            {
                if (model != null)
                    model.OnTrapAdded -= HandleTrapAdded;
            }

            private void HandleTrapAdded(TrapModel trap)
            {
                // var sumPos = Vector3.zero;
                // foreach (var h in trap.Hexes)
                // {
                //     var hexObj = field.GetHex(h);
                //     sumPos += tilemap.GetCellCenterWorld(hexObj.offset);
                // }
                // var centerPos = sumPos / trap.Hexes.Count;
                // centerPos.z = -0.1f;
                //
                // var viewGo = Instantiate(trap.Data.viewPrefab, centerPos, Quaternion.identity);
                // var view = viewGo.GetComponent<TrapView>();
                // var prefabRenderer = trap.Data.viewPrefab.GetComponentInChildren<SpriteRenderer>();
                // if (prefabRenderer != null)
                // {
                //     view.Initialize(prefabRenderer.sprite);
                // }
                // views.Add(trap, view);
                
                var pos = trap.Hexes.Select(h => tilemap.GetCellCenterWorld(field.GetHex(h).offset)).ToList();
                var finalPos = new Vector3(pos.Average(p => p.x), pos.Min(p => p.y) - tilemap.cellSize.y * 0.5f, -0.1f);
                var viewGo = Instantiate(trap.Data.viewPrefab, finalPos, Quaternion.identity);
                var view = viewGo.GetComponent<TrapView>();
                view.Initialize(trap.Data.viewPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                
                views.Add(trap, view);
            }
        }
    }
}