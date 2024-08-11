using Kitchen;
using KitchenData;
using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CCatalogMenuInfo : IComponentData, IModComponent, IPlayerSpecificUI
    {
        public InputIdentifier Player;

        public Entity PlayerEntity;

        public bool IsComplete;

        Entity IPlayerSpecificUI.PlayerEntity => PlayerEntity;

        bool IPlayerSpecificUI.IsComplete => IsComplete;
    }
}
