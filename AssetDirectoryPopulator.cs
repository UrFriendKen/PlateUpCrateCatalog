using Kitchen;
using KitchenMods;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace KitchenCrateCatalog
{
    [FilterModes(AllowedModes = GameConnectionMode.All)]
    public class AssetDirectoryPopulator : GenericSystemBase, IModSystem
    {
        bool _hasPopulated = false;

        protected override void OnUpdate()
        {
            if (_hasPopulated ||
                !TryGetSingletonEntity<SAssetDirectory>(out Entity singleton) ||
                !EntityManager.HasComponent<CViewDirectory>(singleton))
                return;
            _hasPopulated = true;

            Dictionary<ViewType, GameObject> dict = AssetDirectory.ViewPrefabs;

            if (dict == null)
                return;

            dict[Main.CatalogViewType] = PrefabManager.GetPrefab("Catalog");
        }
    }
}
