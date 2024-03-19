using Kitchen;
using System;
using System.Reflection;
using Unity.Entities;

namespace KitchenCrateCatalog.Extensions
{
    internal static class EntityContextExtensions
    {
        static Type t_CCircleLineTrack = typeof(AchievementCircleLine).GetNestedType("CCircleLineTrack", BindingFlags.NonPublic);
        static ComponentType ct_CCircleLineTrack = t_CCircleLineTrack != null ? ((ComponentType)t_CCircleLineTrack) : default;
        static MethodInfo f_HasCCircleLineTrack = typeof(EntityContext).GetMethod("Has", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Entity) }, null).MakeGenericMethod(t_CCircleLineTrack);
        static MethodInfo f_RemoveCCircleLineTrack = typeof(EntityContext).GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Entity) }, null).MakeGenericMethod(t_CCircleLineTrack);

        public static void HideCrate(this EntityContext ctx, Entity e)
        {
            if (ctx.Has<CItem>(e))
                ctx.Remove<CItem>(e);
            if (ctx.Has<CRequiresView>(e))
                ctx.Remove<CRequiresView>(e);
            if (ctx.Has<CHeldBy>(e))
                ctx.Remove<CHeldBy>(e);
            if (ctx.Has<CPosition>(e))
                ctx.Remove<CPosition>(e);
            if (ctx.Has<AchievementAntisocial.CAntisocialTracker>(e))
                ctx.Remove<AchievementAntisocial.CAntisocialTracker>(e);

            object[] param = new object[] { e };
            if (ct_CCircleLineTrack != null && (bool)(f_HasCCircleLineTrack?.Invoke(ctx, param) ?? false))
                f_RemoveCCircleLineTrack.Invoke(ctx, param);
        }
    }
}
