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
            api.Logger.Notification("Hello from template mod: " + api.Side);
            api.RegisterBlockClass("BlockCropFruit", typeof(BlockCropFruit));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Logger.Notification("Hello from template mod server side: " + Lang.Get("augplantlib:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Logger.Notification("Hello from template mod client side: " + Lang.Get("augplantlib:hello"));
        }
    }
    public class BlockCropFruit : BlockCrop
    {
        protected double interactionInterval;
        protected int fruitStage;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            
            Vintagestory.API.Datastructures.JsonObject attributes = this.Attributes;
            this.fruitStage = ((attributes != null) ? attributes["fruitStage"].AsInt(0) : 0);
            Vintagestory.API.Datastructures.JsonObject attributes1 = this.Attributes;
            this.interactionInterval = ((attributes1 != null) ? attributes1["interactionInterval"].AsDouble(1) : 0.0);
            api.Logger.Notification("fruitStage: " + fruitStage);
            api.Logger.Notification("interactionInterval: " + interactionInterval);

        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            //api.Logger.Notification("Interact Start");
            if (this.CurrentStage() >= this.fruitStage)
            {
                api.Logger.Notification(blockSel.ToString());
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




                world.BlockAccessor.ExchangeBlock(GetPreviousGrowthStageBlock(world, blockSel.Position).BlockId, blockSel.Position);
                return true;
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
        /*public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            api.Logger.Notification("Interact Held for: " + secondsUsed);
            if (this.CurrentStage() >= this.fruitStage)
            {
                if(secondsUsed % (float) interactionInterval == 0) { }
                



                world.BlockAccessor.ExchangeBlock(GetPreviousGrowthStageBlock(world, blockSel.Position).BlockId, blockSel.Position);
            }

            return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);
        }*/

        private Block GetPreviousGrowthStageBlock(IWorldAccessor world, BlockPos pos)
        {
            int nextStage = this.CurrentStage() - 1;
            if (world.GetBlock(base.CodeWithParts(nextStage.ToString())) == null)
            {
                nextStage = 1;
            }
            return world.GetBlock(base.CodeWithParts(nextStage.ToString()));
        }
    }
}
