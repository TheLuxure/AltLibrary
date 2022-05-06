﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace AltLibrary.Common.Hooks
{
    internal class AnimatedModIcon
    {
        private static MethodInfo Limits = null;

        private static event ILContext.Manipulator ModifyLimits
        {
            add
            {
                HookEndpointManager.Modify(Limits, value);
            }
            remove
            {
                HookEndpointManager.Unmodify(Limits, value);
            }
        }

        internal static void Init()
        {
            var UIMods = typeof(Main).Assembly.GetType("Terraria.ModLoader.UI.UIModItem");
            if (UIMods != null)
            {
                Limits = UIMods.GetMethod("OnInitialize", BindingFlags.Public | BindingFlags.Instance);
            }
            if (Limits != null)
            {
                ModifyLimits += AnimatedModIcon_ModifyLimits;
            }
        }

        internal static void Unload()
        {
            if (Limits != null)
            {
                ModifyLimits -= AnimatedModIcon_ModifyLimits;
            }
        }

        private static void AnimatedModIcon_ModifyLimits(ILContext il)
        {
            ILCursor c = new(il);
            FieldReference _mod = null;
            FieldReference _modIcon = null;
            if (!c.TryGotoNext(i => i.MatchLdfld(out _mod)))
                return;
            if (!c.TryGotoNext(i => i.MatchLdstr(".png")))
                return;
            if (!c.TryGotoNext(i => i.MatchStfld(out _modIcon)))
                return;
            if (!c.TryGotoNext(i => i.MatchLdstr("Unknown error")))
                return;
            if (!c.TryGotoNext(i => i.MatchLdarg(0)))
                return;

            var label = il.DefineLabel();

            c.Index++;
            c.Emit(OpCodes.Ldfld, _mod);
            c.Emit(OpCodes.Callvirt, typeof(Main).Assembly.GetType("Terraria.ModLoader.Core.LocalMod").GetMethod("get_Name"));
            c.Emit(OpCodes.Ldstr, "AltLibrary");
            c.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
            c.Emit(OpCodes.Brfalse_S, label);

            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldfld, _modIcon);
            c.EmitDelegate(() =>
            {
                UIImageFramed image = new(ModContent.Request<Texture2D>("AltLibrary/icon_anim", AssetRequestMode.ImmediateLoad), new Rectangle(0, 0, 80, 80))
                {
                    Left =
                    {
                        Percent = 0f
                    },
                    Top =
                    {
                        Percent = 0f
                    }
                };
                image.OnUpdate += Image_OnUpdate;
                return image;
            });
            c.Emit(OpCodes.Callvirt, typeof(UIElement).GetMethod(nameof(UIElement.Append), new Type[] { typeof(UIElement) }));

            c.MarkLabel(label);
            c.Emit(OpCodes.Ldarg, 0);
        }

        private static void Image_OnUpdate(UIElement affectedElement)
        {
            UIImageFramed e = affectedElement as UIImageFramed;
            int time = 3600;

            if (AltLibrary.TimeHoveringOnIcon >= time + 1)
            {
                e.SetFrame(new Rectangle(80, 0, 80, 80));
                AltLibrary.HallowBunnyUnlocked = true;
                return;
            }

            if (e.IsMouseHovering)
            {
                float i = Main.GlobalTimeWrappedHourly % 20;
                if (i >= 0 && i < 10 || i >= 15 && i < 20)
                {
                    e.SetFrame(new Rectangle(0, 80, 80, 80));
                }
                else
                {
                    e.SetFrame(new Rectangle(0, 160, 80, 80));
                }
                if (++AltLibrary.TimeHoveringOnIcon == time)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen);
                    e.SetFrame(new Rectangle(80, 0, 80, 80));
                    AltLibrary.TimeHoveringOnIcon = time + 1;
                }
                if (AltLibrary.TimeHoveringOnIcon >= time)
                {
                    AltLibrary.HallowBunnyUnlocked = true;
                }
                return;
            }

            AltLibrary.TimeHoveringOnIcon = 0;
            float index = Main.GlobalTimeWrappedHourly % 60;
            if (index < 30)
            {
                e.SetFrame(new Rectangle(0, 0, 80, 80));
            }
            else if (index >= 30 && index < 40 || index >= 45 && index < 60)
            {
                e.SetFrame(new Rectangle(0, 80, 80, 80));
            }
            else if (index >= 40 && index < 45)
            {
                e.SetFrame(new Rectangle(0, 160, 80, 80));
            }
        }
    }
}
