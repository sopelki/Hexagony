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
                Debug.Log($"Trap axial: {trap.Hex}");
                var hexObj = field.GetHex(trap.Hex);
                if (hexObj == null)
                    return;

                var worldPos = tilemap.GetCellCenterWorld(hexObj.offset);

                // worldPos.z = -0.1f;   // ✅ очень важно

                var viewGo = Instantiate(trap.Data.viewPrefab, worldPos, Quaternion.identity);
                var view = viewGo.GetComponent<TrapView>();

                views.Add(trap, view);
            }
        }
    }
}