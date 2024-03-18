using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace KitchenCrateCatalog
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CTriggersCatalogMenu : IComponentData, IModComponent
    {
    }
}
