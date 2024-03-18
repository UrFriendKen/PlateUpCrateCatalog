using Unity.Entities;

namespace KitchenCrateCatalog
{
    [InternalBufferCapacity(4)]
    public struct CPlayerTriggeredMenu : IBufferElementData
    {
        public int PlayerID;
        public int InputSource;
        public Entity Indicator;
    }
}
