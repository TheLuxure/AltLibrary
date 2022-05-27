using AltLibrary.Common;
using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.AltLiquidStyles;
using AltLibrary.Common.AltOres;
using AltLibrary.Common.Hooks;
using AltLibrary.Common.Systems;
using AltLibrary.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AltLibrary
{
    public class AltLibrary : Mod
    {
        private static AltLibrary instance;

        public static AltLibrary Instance { get => instance; internal set => instance = value; }

        internal static List<AltBiome> Biomes;

        internal static List<AltOre> Ores;

        internal static List<AltLiquidStyle> LiquidStyles;

        internal static List<CustomPreviews> PreviewWorldIcons;

        // Spreading related lists.
        internal static List<int> planteraBulbs;
        internal static List<int> jungleGrass;
        internal static List<int> jungleThorns;
        internal static List<int> evilStoppingOres;

        internal static int TimeHoveringOnIcon;
        internal static bool HallowBunnyUnlocked;
        internal static int ModIconVariation;

        public AltLibrary()
        {
            Instance = this;
        }

        public override void Load()
        {
            ILHooks.OnInitialize();
            AnimatedModIcon.Init();
            ALTextureAssets.Load();
            ModIconVariation = Main.rand.Next(ALTextureAssets.AnimatedModIcon.Length);
            TimeHoveringOnIcon = 0;
            HallowBunnyUnlocked = false;
            PreviewWorldIcons = new();
            Biomes = new();
            Ores = new();
            LiquidStyles = new();
            planteraBulbs = new() { TileID.PlanteraBulb };
            jungleGrass = new() { TileID.JungleGrass };
            jungleThorns = new() { TileID.JungleThorns };
            evilStoppingOres = new() { TileID.Chlorophyte, TileID.ChlorophyteBrick };
        }

        public override void PostSetupContent()
        {
            ALTextureAssets.PostContentLoad();
        }

        public override object Call(params object[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args), "Arguments cannot be null!");
            if (args.Length == 0)
                throw new ArgumentException("Arguments cannot be empty!");
            if (args[0] is string content)
            {
                switch (content.ToLower())
                {
                    case "addcustomseedpreviews":
                        if (args.Length != 5)
                            throw new ArgumentException("Arguments cannot be less or more than 5 in length for AddCustomSeedPreviews");
                        if (args[1] is not string seed)
                            throw new ArgumentException("Second argument (seed) is not string!");
                        if (args[2] is not string small)
                            throw new ArgumentException("Third argument (small) is not string!");
                        if (args[3] is not string medium)
                            throw new ArgumentException("Fourth argument (medium) is not string!");
                        if (args[4] is not string large)
                            throw new ArgumentException("Fifth argument (large) is not string!");
                        PreviewWorldIcons.Add(new CustomPreviews(seed, small, medium, large));
                        Logger.Info($"Registered custom preview! Seed: {seed} Path: {small} {medium} {large}");
                        break;
                }
            }
            return null;
        }

        public override void Unload()
        {
            AnimatedModIcon.Unload();
            ALTextureAssets.Unload();
            AltLibraryConfig.Config = null;
            TimeHoveringOnIcon = 0;
            HallowBunnyUnlocked = false;
            PreviewWorldIcons = null;
            if (!Main.dedServ)
            {
                Instance = null;
            }
            Biomes = null;
            Ores = null;
            LiquidStyles = null;
            planteraBulbs = null;
            jungleGrass = null;
            jungleThorns = null;
            evilStoppingOres = null;
            ILHooks.Unload();
            AltLibraryConfig.Config = null;
        }

        internal struct CustomPreviews
        {
            internal string seed;
            internal string pathSmall;
            internal string pathMedium;
            internal string pathLarge;

            public CustomPreviews(string seed, string pathSmall, string pathMedium, string pathLarge)
            {
                if (seed is null)
                {
                    throw new ArgumentNullException(nameof(seed), "Cannot be null!");
                }
                if (pathSmall is null)
                {
                    throw new ArgumentNullException(nameof(pathSmall), "Cannot be null!");
                }
                if (pathMedium is null)
                {
                    throw new ArgumentNullException(nameof(pathMedium), "Cannot be null!");
                }
                if (pathLarge is null)
                {
                    throw new ArgumentNullException(nameof(pathLarge), "Cannot be null!");
                }

                this.seed = seed;
                this.pathSmall = pathSmall;
                this.pathMedium = pathMedium;
                this.pathLarge = pathLarge;
            }
        }
    }
}