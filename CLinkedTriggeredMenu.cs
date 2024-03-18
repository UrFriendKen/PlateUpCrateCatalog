using KitchenMods;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    public struct CLinkedTriggeredMenu : IComponentData, IModComponent
    {
        public Entity SourceEntity;

        public Entity PlayerEntity;

        public int BelongsToPlayerID;

        public int InputSource;

        public bool IsComplete;
    }
}
