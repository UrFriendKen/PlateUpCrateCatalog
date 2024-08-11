using KitchenData;
using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;

namespace KitchenCrateCatalog
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CTriggersCatalogMenu : IComponentData, IModComponent, IPlayerSpecificUISource
    {

        public Vector3 DrawLocation;

        Vector3 IPlayerSpecificUISource.DrawLocation => DrawLocation;
    }
}
