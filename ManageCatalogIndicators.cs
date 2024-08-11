using Kitchen;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    public class ManageCatalogIndicators : PlayerSpecificUIIndicator<CTriggersCatalogMenu, CCatalogMenuInfo>
    {
        protected override ViewType ViewType => Main.CatalogViewType;

        protected override CCatalogMenuInfo GetInfo(Entity source, CTriggersCatalogMenu selector, CTriggerPlayerSpecificUI trigger, CPlayer player)
        {
            CCatalogMenuInfo result = default;
            result.Player = player;
            result.PlayerEntity = trigger.TriggerEntity;
            return result;
        }
    }
}
