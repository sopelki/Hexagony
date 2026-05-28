using System.Collections.Generic;
using Logic.Projectile;
using UnityEngine;

namespace View
{
    public class ProjectileViewManager : MonoBehaviour
    {
        [SerializeField]
        private Transform parent;

        private readonly Dictionary<ProjectileModel, ProjectileView> views = new();

        public void Initialize(ProjectileSystem system)
        {
            Debug.Log("ProjectileViewManager Initialized");
            system.OnProjectileCreated += HandleCreated;
            system.OnProjectileDestroyed += HandleDestroyed;
        }

        private void HandleCreated(ProjectileModel model)
        {
            var go = Instantiate(
                model.Data.prefab,
                model.Position,
                Quaternion.identity,
                parent
            );

            var view = go.GetComponent<ProjectileView>();
            view.Initialize(model);

            views.Add(model, view);
        }

        private void HandleDestroyed(ProjectileModel model)
        {
            if (!views.TryGetValue(model, out var view))
                return;

            Destroy(view.gameObject);
            views.Remove(model);
        }
    }
}