using System.Text.Json.Nodes;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using System;
using Vintagestory.API.MathTools;


[assembly: ModInfo("augplantlib",
                    Authors = new string[] { "Unknown" },
                    Description = "This is a sample mod",
                    Version = "1.0.0")]

namespace augplantlib
{
    public class augplantlibModSystem : ModSystem
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            //api.Logger.Notification("Hello from template mod: " + api.Side);
            api.RegisterBlockClass("BlockCropFruit", typeof(BlockCropFruit));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            //api.Logger.Notification("Hello from template mod server side: " + Lang.Get("augplantlib:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            //api.Logger.Notification("Hello from template mod client side: " + Lang.Get("augplantlib:hello"));
        }
    }
    public class BlockCropFruit : BlockCrop
    {
        protected double interactionInterval;
        protected int fruitStage;
        protected Block prevStage;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            
            Vintagestory.API.Datastructures.JsonObject attributes = this.Attributes;
            this.fruitStage = ((attributes != null) ? attributes["fruitStage"].AsInt(0) : 0);
            Vintagestory.API.Datastructures.JsonObject attributes1 = this.Attributes;
            this.interactionInterval = ((attributes1 != null) ? attributes1["interactionInterval"].AsDouble(1) : 0.0);
            //api.Logger.Notification("fruitStage: " + fruitStage);
            //api.Logger.Notification("interactionInterval: " + interactionInterval);
            //prevStage = GetPreviousGrowthStageBlock(world);

        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            //api.Logger.Notification("Interact Start");
            //EnumHandling handling = EnumHandling.PreventSubsequent;
            prevStage = GetPreviousGrowthStageBlock(world);
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }
            if (this.CurrentStage() >= this.fruitStage)
            {
                BlockPos pos = blockSel.Position;
                return true;
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
       public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (this.CurrentStage() >= this.fruitStage)
            {
                BlockPos pos = blockSel.Position;
                BlockSounds sounds = this.Sounds;
                if(secondsUsed % 0.25 <= 0.05)
                world.PlaySoundAt((sounds != null) ? sounds.GetHitSound(byPlayer) : null, (double)pos.X, (double)pos.Y, (double)pos.Z, byPlayer, true, 32f, 1f);
                if (secondsUsed >= (float) interactionInterval) {
                    gibDrop(world, byPlayer, blockSel, prevStage);
                    return false;
                }
                return true;
            }
            return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);
        }
        protected void gibDrop(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, Block c)
        {
            BlockPos pos = blockSel.Position;
            ItemStack[] drops = this.GetDrops(world, pos, byPlayer, 1f);
            if (drops != null)
            {
                for (int i = 0; i < drops.Length; i++)
                {
                    if (this.SplitDropStacks)
                    {
                        for (int j = 0; j < drops[i].StackSize; j++)
                        {
                            ItemStack stack = drops[i].Clone();
                            stack.StackSize = 1;
                            world.SpawnItemEntity(stack, new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                        }
                    }
                    else
                    {
                        world.SpawnItemEntity(drops[i].Clone(), new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                    }
                }
            }
            BlockSounds sounds = this.Sounds;
            world.PlaySoundAt((sounds != null) ? sounds.GetBreakSound(byPlayer) : null, (double)pos.X, (double)pos.Y, (double)pos.Z, byPlayer, true, 32f, 1f);
            if (c != null) { world.BlockAccessor.ExchangeBlock(c.BlockId, blockSel.Position); }
        }



        private Block GetPreviousGrowthStageBlock(IWorldAccessor world)
        {
            int nextStage = this.CurrentStage() - 1;
            if(nextStage < fruitStage) nextStage = fruitStage-1;
            if (world.GetBlock(base.CodeWithParts(nextStage.ToString())) == null)
            {
                nextStage = 1;
            }
            return world.GetBlock(base.CodeWithParts(nextStage.ToString()));
        }
    }
}
