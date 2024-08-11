using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace KitchenCrateCatalog
{
    public class CreateCatalogTrigger : FranchiseFirstFrameSystem, IModSystem
    {
        private EntityQuery Crates;

        protected override void Initialise()
        {
            base.Initialise();
            Crates = GetEntityQuery(typeof(CCrateAppliance));
        }

        protected override void OnUpdate()
        {
            if (Crates.IsEmpty)
            {
                return;
            }

            Entity applianceEntity = Create(GameData.Main.Get<Appliance>(949631021), LobbyPositionAnchors.Workshop + new Vector3(-1f, 0f, 0f), Vector3.forward);  // Accounting Desk
            Set(applianceEntity, default(CDoNotPersist));
            Set(applianceEntity, default(CTriggersCatalogMenu));
            Set(applianceEntity, default(CTriggerPlayerSpecificUI));
        }
    }
}
