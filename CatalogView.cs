﻿using Controllers;
using Kitchen;
using Kitchen.Modules;
using KitchenData;
using KitchenMods;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using static Controllers.InputLock;

namespace KitchenCrateCatalog
{
    public class CatalogView : ResponsiveObjectView<CatalogView.ViewData, CatalogView.ResponseData>, IInputConsumer
    {
        public class UpdateView : ResponsiveViewSystemBase<ViewData, ResponseData>, IModSystem
        {
            EntityQuery Views;
            EntityQuery CrateAppliances;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(typeof(CCatalogMenu), typeof(CLinkedTriggeredMenu), typeof(CLinkedView));
                CrateAppliances = GetEntityQuery(typeof(CCrateAppliance));
                RequireForUpdate(Views);
            }

            protected override void OnUpdate()
            {
                using NativeArray<CCrateAppliance> crateAppliances = CrateAppliances.ToComponentDataArray<CCrateAppliance>(Allocator.Temp);

                Dictionary<int, int> applianceCounts = new Dictionary<int, int>();
                foreach (CCrateAppliance crateAppliance in crateAppliances)
                {
                    applianceCounts.TryGetValue(crateAppliance.Appliance, out int count);
                    applianceCounts[crateAppliance.Appliance] = count + 1;
                }

                using NativeArray<Entity> entities = Views.ToEntityArray(Allocator.Temp);
                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using NativeArray<CLinkedTriggeredMenu> menuLinks = Views.ToComponentDataArray<CLinkedTriggeredMenu>(Allocator.Temp);
                for (int i = 0; i < views.Length; i++)
                {
                    CLinkedView view = views[i];
                    CLinkedTriggeredMenu menuLink = menuLinks[i];

                    SendUpdate(view, new ViewData()
                    {
                        PlayerID = menuLink.BelongsToPlayerID,
                        Items = applianceCounts
                    });

                    bool isComplete = false;
                    if (ApplyUpdates(view, delegate (ResponseData response)
                    {
                        isComplete = response.IsComplete;
                    }, only_final_update: true))
                    {
                        menuLink.IsComplete = isComplete;
                        Set(entities[i], menuLink);
                    }
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : IViewData, IViewResponseData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)]
            public int PlayerID;

            [Key(1)]
            public Dictionary<int, int> Items;

            public bool IsChangedFrom(ViewData check)
            {
                return PlayerID != check.PlayerID ||
                    Items.Count != check.Items.Count ||
                    Items.Where(kvp => !check.Items.TryGetValue(kvp.Key, out int checkValue) || kvp.Value != checkValue).Any();
            }
        }

        [MessagePackObject(false)]
        public struct ResponseData : IResponseData, IViewResponseData
        {
            [Key(0)]
            public bool IsComplete;
        }

        private bool IsComplete;

        private int PlayerID;

        private InputLock.Lock Lock;

        private ModuleList ModuleList;

        private PanelElement Panel;

        private CatalogMenu Menu;
        
        public override void Initialise()
        {
            base.Initialise();
            Panel = Add<PanelElement>();
            ModuleList = new ModuleList();
            Menu = new CatalogMenu(base.transform, ModuleList);
            Menu.OnRequestMenu += delegate (object _, Type t)
            {
                CloseCatalog();
            };
            Menu.OnRequestSkipStackMenu += delegate (object _, Type t)
            {
                CloseCatalog();
            };
            Menu.OnRequestAction += OnRequestAction;
            Menu.OnPreviousMenu += delegate
            {
                OnRequestAction(null, CatalogMenuAction.Back);
            };
            Menu.OnRedraw += delegate
            {
                Panel?.SetTarget(ModuleList);
            };
        }

        private void OnRequestAction(object _, CatalogMenuAction e)
        {
            switch (e)
            {
                case CatalogMenuAction.Back:
                case CatalogMenuAction.Close:
                    CloseCatalog();
                    break;
            }
        }

        protected override void UpdateData(ViewData view_data)
        {
            if (InputSourceIdentifier.DefaultInputSource != null)
            {
                if (!Players.Main.Get(view_data.PlayerID).IsLocalUser)
                {
                    base.gameObject.SetActive(value: false);
                    return;
                }
                base.gameObject.SetActive(value: true);
                IEnumerable<CatalogMenu.Item> items = view_data.Items?
                    .Select(kvp =>
                    {
                        string name = default;
                        if (GameData.Main.TryGet(kvp.Key, out GameDataObject gdo))
                        {
                            if (gdo is Appliance applianceGDO)
                                name = applianceGDO.Name;
                            else if (gdo is Dish dishGDO)
                                name = dishGDO.Name;
                        }
                        if (name == default)
                            name = $"{kvp.Key}";

                        return new CatalogMenu.Item()
                        {
                            Name = name,
                            Count = kvp.Value
                        };
                    });
                InitialiseForPlayer(view_data.PlayerID, items);
            }
        }

        public override bool HasStateUpdate(out IResponseData state)
        {
            state = null;
            if (IsComplete)
            {
                state = new ResponseData
                {
                    IsComplete = IsComplete
                };
            }
            return IsComplete;
        }

        private void InitialiseForPlayer(int player, IEnumerable<CatalogMenu.Item> items)
        {
            LocalInputSourceConsumers.Register(this);
            if (Lock.Type != 0)
            {
                InputSourceIdentifier.DefaultInputSource.ReleaseLock(PlayerID, Lock);
            }
            PlayerID = player;
            Lock = InputSourceIdentifier.DefaultInputSource.SetInputLock(PlayerID, PlayerLockState.NonPause);
            if (Menu == null)
            {
                CloseCatalog();
                return;
            }
            Menu.SetItems(items.ToList());
            Menu.SetupWithPlayer(PlayerID);
            Panel.SetColour(PlayerID);
        }

        public InputConsumerState TakeInput(int player_id, InputState state)
        {
            if (PlayerID != 0 && player_id == PlayerID)
            {
                if (state.MenuTrigger == ButtonState.Pressed)
                {
                    IsComplete = true;
                    InputSourceIdentifier.DefaultInputSource.ReleaseLock(PlayerID, Lock);
                    return InputConsumerState.Terminated;
                }
                if (!ModuleList.HandleInteraction(state) && state.MenuCancel == ButtonState.Pressed)
                {
                    CloseCatalog();
                }
                if (!IsComplete)
                {
                    return InputConsumerState.Consumed;
                }
                return InputConsumerState.Terminated;
            }
            return InputConsumerState.NotConsumed;
        }

        private void CloseCatalog()
        {
            IsComplete = true;
            InputSourceIdentifier.DefaultInputSource.ReleaseLock(PlayerID, Lock);
            LocalInputSourceConsumers.Remove(this);
        }
        public override void Remove()
        {
            IsComplete = true;
            InputSourceIdentifier.DefaultInputSource.ReleaseLock(PlayerID, Lock);
            base.Remove();
        }

        private void OnDestroy()
        {
            LocalInputSourceConsumers.Remove(this);
        }

    }
}
