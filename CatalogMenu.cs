using Kitchen;
using Kitchen.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenCrateCatalog
{
    public enum CatalogMenuAction
    {
        Back,
        Close
    }

    public class CatalogMenu : Menu<CatalogMenuAction>
    {
        public struct Item
        {
            public int ID;
            public string Name;
            public int Count;
        }

        protected int PlayerID;

        private int CurrentPage;

        protected List<Item> Items;

        protected bool IsSelectable;

        public int ItemsPerPage;

        public EventHandler OnRedraw;

        public EventHandler<Item> OnItemClick;

        public CatalogMenu(Transform container, ModuleList module_list)
            : base(container, module_list)
        {
            ItemsPerPage = 10;
            Items = new List<Item>();
            DefaultElementSize = new Vector2(2.5f, 0.35f);
            module_list.Padding = 0.05f;
        }

        public override void Setup(int player_id)
        {
            CurrentPage = 0;
            Redraw();
        }

        protected void Redraw()
        {
            ModuleList.Clear();

            if (Items.Count > 0)
                AddButton($"{Items.Select(x => x.Count).Sum()} Items", null).SetSelectable(false);
            
            int pageCount = Mathf.CeilToInt((float)Items.Count / ItemsPerPage);

            IEnumerable<int> pageValues = Enumerable.Range(0, Mathf.Max(pageCount, 1));
            Option<int> pageSelect = new Option<int>(pageValues.ToList(), CurrentPage, pageValues.Select(i => $"Page {i + 1}").ToList());
            pageSelect.OnChanged += delegate (object _, int i)
            {
                CurrentPage = i;
                Redraw();
            };
            Add(pageSelect);

            int startIndex = CurrentPage * ItemsPerPage;
            bool hasItem =  false;
            for (int i = startIndex; i < startIndex + ItemsPerPage && i < Items.Count; i++)
            {
                hasItem = true;
                Item item = Items[i];
                string itemText = $"{item.Name ?? "Unknown Item"} - {item.Count}";
                if (IsSelectable)
                {
                    AddButton(itemText, delegate (int _)
                    {
                        OnItemClick(this, item);
                    });
                }
                else
                {
                    AddLabel(itemText);
                }
            }

            if (!hasItem)
            {
                AddInfo($"No items!");
            }

            New<SpacerElement>();
            AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate
            {
                RequestAction(CatalogMenuAction.Back);
            }, 0, 0.75f);

            OnRedraw.Invoke(this, null);
        }

        public void SetItems(List<Item> items)
        {
            Items = items?.OrderByDescending(item => item.Count).ToList() ?? new List<Item>();
            if ((ModuleList.Modules?.Count ?? 0) > 0)
                Redraw();
        }

        public void SetSelectable(bool isSelectable)
        {
            IsSelectable = isSelectable;
            if ((ModuleList.Modules?.Count ?? 0) > 0)
                Redraw();
        }

        public void SetupWithPlayer(int player_id)
        {
            PlayerID = player_id;
            Setup(PlayerID);
        }
    }
}
