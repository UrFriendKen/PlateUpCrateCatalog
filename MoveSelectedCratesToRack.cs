using Kitchen;
using KitchenCrateCatalog.Extensions;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    public struct CMoveApplianceCratesToRackRequest : IComponentData, IModComponent
    {
        public int ID;
    }

    public class MoveSelectedCratesToRack : GameSystemBase, IModSystem
    {
        EntityQuery Requests;

        EntityQuery Racks;

        EntityQuery Crates;

        protected override void Initialise()
        {
            base.Initialise();
            Crates = GetEntityQuery(new QueryHelper()
                .All(typeof(CCrateAppliance), typeof(CPersistentItem))
                .None(typeof(CItem), typeof(CCreateItem)));
            Requests = GetEntityQuery(typeof(CMoveApplianceCratesToRackRequest));
            Racks = GetEntityQuery(typeof(CAppliance), typeof(CItemHolder), typeof(CPersistentItemStorageLocation));
            RequireForUpdate(Requests);
        }

        protected override void OnUpdate()
        {
            EntityContext ctx = new EntityContext(EntityManager);

            if (!Has<SFranchiseMarker>() ||
                !TryGetSelectedCrateAppliance(out int applianceID, out ItemCategory crateItemCategory) ||
                !TryGetMatchingCrateIndices(applianceID, out Queue<int> matchingCrateIndices))
            {
                EntityManager.DestroyEntity(Requests);
                return;
            }

            using NativeArray<Entity> crateApplianceEntities = Crates.ToEntityArray(Allocator.Temp);
            using NativeArray<CPersistentItem> cratePersistentItems = Crates.ToComponentDataArray<CPersistentItem>(Allocator.Temp);

            using NativeArray<Entity> rackEntities = Racks.ToEntityArray(Allocator.Temp);
            using NativeArray<CItemHolder> holders = Racks.ToComponentDataArray<CItemHolder>(Allocator.Temp);
            for (int i = 0; i < rackEntities.Length; i++)
            {
                if (matchingCrateIndices.Count < 1)
                    break;

                Entity rackEntity = rackEntities[i];
                CItemHolder holder = holders[i];

                if (ctx.Require(rackEntity, out CItemHolderFilter holderFilter) &&
                    (holderFilter.NoDirectInsertion || !holderFilter.AllowCategory(crateItemCategory)))
                    continue;

                if (holder.HeldItem != default)
                {
                    if (!Require(holder.HeldItem, out CCrateAppliance currentApplianceCrate) || currentApplianceCrate.Appliance == applianceID)
                        continue;
                    ctx.HideCrate(holder.HeldItem);
                }
                int crateApplianceIndex = matchingCrateIndices.Dequeue();
                ctx.Set(crateApplianceEntities[crateApplianceIndex], new CCreateItem()
                {
                    ID = cratePersistentItems[crateApplianceIndex].ItemID,
                    Holder = rackEntity
                });
            }
            EntityManager.DestroyEntity(Requests);
        }

        private bool TryGetSelectedCrateAppliance(out int applianceID, out ItemCategory crateItemCategory)
        {
            applianceID = 0;
            crateItemCategory = default;
            using NativeArray<CMoveApplianceCratesToRackRequest> requests = Requests.ToComponentDataArray<CMoveApplianceCratesToRackRequest>(Allocator.Temp);
            foreach (CMoveApplianceCratesToRackRequest request in requests)
            {
                if (!GameData.Main.TryGet(request.ID, out Appliance appliance))
                    continue;
                int crateItemID = appliance.CrateItem?.ID ?? AssetReference.ApplianceCrate;
                if (!GameData.Main.TryGet(crateItemID, out Item crateItem))
                    continue;
                applianceID = request.ID;
                crateItemCategory = crateItem.ItemCategory;
                break;
            }
            return applianceID != 0;
        }

        private bool TryGetMatchingCrateIndices(int applianceID, out Queue<int> indices)
        {
            if (Crates.IsEmpty)
            {
                indices = null;
                return false;
            }

            indices = new Queue<int>();
            using NativeArray<CCrateAppliance> crateAppliances = Crates.ToComponentDataArray<CCrateAppliance>(Allocator.Temp);
            for (int i = crateAppliances.Length - 1; i > -1; i--)
            {
                if (crateAppliances[i].Appliance == applianceID)
                    indices.Enqueue(i);
            }
            Main.LogWarning($"{indices.Count} matching crates found");
            return indices.Count > 0;
        }
    }
}
