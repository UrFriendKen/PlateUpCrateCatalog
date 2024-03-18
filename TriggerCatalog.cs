using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    public class TriggerCatalog : ItemInteractionSystem, IModSystem
    {
        CPlayer _player;

        CPosition _position;

        DynamicBuffer<CPlayerTriggeredMenu> _buffer;

        protected override InteractionType RequiredType => InteractionType.Act;

        protected override bool BeforeRun()
        {
            base.BeforeRun();
            _buffer = default;
            return true;
        }

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require(data.Interactor, out _player))
                return false;

            if (!Has<CTriggersCatalogMenu>(data.Target) ||
                !Require(data.Target, out _position) ||
                !RequireBuffer(data.Target, out _buffer))
                return false;

            for (int i = 0; i < _buffer.Length; i++)
            {
                if (_buffer[i].InputSource == _player.InputSource)
                    return false;
            }

            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            _buffer.Add(CreateMenu<CCatalogMenu>(data.Context, data.Target, data.Interactor, _player, _position, Main.CatalogViewType));
        }

        private CPlayerTriggeredMenu CreateMenu<T>(EntityContext ctx, Entity source, Entity playerEntity, CPlayer player, CPosition position, ViewType view_type) where T : struct, IComponentData
        {
            Entity menuEntity = ctx.CreateEntity();
            ctx.Set(menuEntity, default(CDoNotPersist));
            ctx.Set(menuEntity, default(T));
            ctx.Set(menuEntity, new CRequiresView()
            {
                Type = view_type,
                ViewMode = ViewMode.WorldToScreen
            });
            ctx.Set(menuEntity, position);
            ctx.Set(menuEntity, new CLinkedTriggeredMenu()
            {
                SourceEntity = source,
                PlayerEntity = playerEntity,
                BelongsToPlayerID = player.ID,
                InputSource = player.InputSource
            });

            return new CPlayerTriggeredMenu()
            {
                PlayerID = player.ID,
                InputSource = player.InputSource,
                Indicator = menuEntity
            };
        }
    }
}
