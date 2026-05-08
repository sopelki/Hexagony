using System;
using System.Collections.Generic;
using Logic.Unit;
using UnityEngine;

namespace View
{
    [CreateAssetMenu(menuName = "BuffViewManger")]
    public class UnitBuffsViewManager: ScriptableObject
    {
        [Serializable]
        public struct BuffMap
        {
            public string buffClassName; // Сюда пишем "AttackPercentBuff" или "HealthPercentBuff"
            public GameObject glowPrefab; // Префаб магического свечения
        }

        public List<BuffMap> mappings;

        public GameObject GetPrefabForBuff(Buff buff)
        {
            var typeName = buff.GetType().Name;
            var map = mappings.Find(m => m.buffClassName == typeName);
            return map.glowPrefab;
        }
    }
}