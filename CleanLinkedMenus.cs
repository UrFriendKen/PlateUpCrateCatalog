using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    public class CleanLinkedMenus : GameSystemBase, IModSystem
    {
        EntityQuery LinkedMenus;

        protected override void Initialise()
        {
            base.Initialise();
            LinkedMenus = GetEntityQuery(typeof(CLinkedTriggeredMenu));
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = LinkedMenus.ToEntityArray(Allocator.Temp);
            using NativeArray<CLinkedTriggeredMenu> linkedMenus = LinkedMenus.ToComponentDataArray<CLinkedTriggeredMenu>(Allocator.Temp);

            for (int i = linkedMenus.Length - 1; i > -1; i--)
            {
                CLinkedTriggeredMenu linkedMenu = linkedMenus[i];
                if (linkedMenu.SourceEntity != default &&
                    linkedMenu.PlayerEntity != default &&
                    !linkedMenu.IsComplete)
                    continue;

                int playerID = linkedMenu.BelongsToPlayerID;

                if (RequireBuffer(linkedMenu.SourceEntity, out DynamicBuffer<CPlayerTriggeredMenu> buffer))
                {
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        if (buffer[j].PlayerID != playerID)
                            continue;
                        buffer.RemoveAt(j);
                        break;
                    }
                }
                EntityManager.DestroyEntity(entities[i]);
            }
        }
    }
}
