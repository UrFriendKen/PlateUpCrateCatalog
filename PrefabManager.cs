using Kitchen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenCrateCatalog
{
    public static class PrefabManager
    {
        static Transform _container;
        static Transform Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new GameObject("CrateCatalog Prefabs").transform;
                    _container.gameObject.SetActive(false);
                    _container.Reset();
                }
                return _container;
            }
        }

        static Dictionary<string, GameObject> Prefabs;

        static void InitPrefabs()
        {
            Prefabs = Prefabs?.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value) ??
                new Dictionary<string, GameObject>();

            if (!Prefabs.ContainsKey("Catalog"))
            {
                int uiLayer = LayerMask.NameToLayer("UI");
                GameObject catalogGO = new GameObject("Catalog");
                Prefabs.Add("Catalog", catalogGO);
                catalogGO.layer = uiLayer;
                catalogGO.transform.SetParent(Container.transform);
                catalogGO.AddComponent<CatalogView>();
            }
        }

        public static GameObject GetPrefab(string name)
        {
            InitPrefabs();

            if (!Prefabs.TryGetValue(name, out GameObject prefab))
            {
                Main.LogError($"Unable to find prefab: {name}");
                return null;
            }
            return prefab;
        }
    }
}
