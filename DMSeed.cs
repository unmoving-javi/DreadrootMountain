using System;
using XRL.Rules;
using XRL.UI;
using XRL.Core;
using System.Collections.Generic;
using System.Text;
using XRL.Liquids;
using XRL.World.Parts.Mutation;
using XRL.World.Capabilities;
using Qud.API;

namespace XRL.World.Parts
{

    //Almost all of this code is based on the Starapple Valley project by Acegiak, which is under no license.
    //Their github can be found at https://github.com/acegiak/QudStarappleValley/
    //This code is admittedly a bit of a frankenstein of their code and other stuff from the base game, not much here is original.
    //Please check out their work as well! Without them this would have been much, much harder.


    [Serializable]
    public class DMSeed : IPart
    {

        public string Result;
        public long growth = 0;

        public int health = 0;

        public long lastseen = 0;

        public bool planted = false;

        public int growthTime = 1200;


        public bool Dead = false;

        public string ResultName;

        public string displayname;
        public string description;

        public DMSeed()
        {

        }

        public double timeUntilGrown()
        {
            return (double)growthTime - (double)growth;
        }

        public override bool SameAs(IPart p)
        {
            if (p is DMSeed)
            {
                DMSeed s = p as DMSeed;
                if (s.Result == this.Result && s.ResultName == this.ResultName && s.growth == this.growth)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "GetInventoryActions");
            Object.RegisterPartEvent(this, "InvCommandPlant");

            Object.RegisterPartEvent(this, "EndTurn");
            Object.RegisterPartEvent(this, "GetDisplayName");
            Object.RegisterPartEvent(this, "GetShortDisplayName");
            Object.RegisterPartEvent(this, "GetShortDescription");
            Object.RegisterPartEvent(this, "AccelerateRipening");
            Object.RegisterPartEvent(this, "CanSmartUse");

            base.Register(Object);
        }


        public void Plant(GameObject who)
        {
            GameObject thisone = ParentObject.RemoveOne();
            if (thisone.InInventory != null)
            {
                EquipmentAPI.DropObject(thisone);
            }
            Cell cell = thisone.CurrentCell;
            if (cell == null)
            {
                Popup.Show("Put things on the ground to plant them.");
                return;
            }
            thisone.pPhysics.Takeable = false;
            thisone.GetPart<DMSeed>().planted = true;
            thisone.GetPart<DMSeed>().tileUpdate();

        }

        public void tileUpdate()
        {
            if (this.Dead)
            {
                ParentObject.pRender.Tile = "Items/plantedseeddead.png";
                this.displayname = "rot";
                this.description = "This plant has withered and died.";
            }
            if (this.planted)
            {
                ParentObject.pRender.Tile = "Items/plantedseed.png";
                this.displayname = "seed";
                this.description = "A planted seed for a " + ResultName + ". Given time, it will grow.";
            }
        }

        public void growthTick()
        {
            if (!this.planted)
            {
                return;
            }
            if (!this.Dead)
            {
                long newGrowth = XRLCore.Core.Game.TimeTicks - this.lastseen;

                if (this.lastseen == 0)
                {
                    newGrowth = 0;
                }

                this.lastseen = XRLCore.Core.Game.TimeTicks;
                this.growth += newGrowth;
            }



            if (this.growth >= this.growthTime && !this.Dead)
            {
                Cell cell = ParentObject.CurrentCell;
                if (cell == null)
                {
                    Popup.Show("Plants cannot grow in the void!");
                    return;
                }
                GameObject growInto = GameObject.create(Result);
                if (growInto.GetPart<Brain>() != null)
                {
                    //growInto.GetPart<Brain>().PerformReequip();
                    //growInto.GetPart<Brain>().BecomeCompanionOf(growInto.ThePlayer);
                    //growInto.GetPart<Brain>().IsLedBy(growInto.ThePlayer);
                    //growInto.GetPart<Brain>().SetFeeling(growInto.ThePlayer, 100);
                    //growInto.GetPart<Brain>().Goals.Clear();
                    //growInto.GetPart<Brain>().Calm = false;
                    //growInto.GetPart<Brain>().Hibernating = false;
                    growInto.GetPart<Brain>().FactionMembership.Clear();
                    //growInto.AddPart(new Combat());

                    XRLCore.Core.Game.ActionManager.AddActiveObject(growInto);
                }
                

                cell.AddObject(growInto);

                ParentObject.FireEvent(new Event("DMSeedGrowth", "From", ParentObject, "To", growInto));
                cell.RemoveObject(ParentObject);
                ParentObject.Destroy();

            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "GetInventoryActions")
            {
                if (ParentObject.pPhysics.CurrentCell != null || ParentObject.InInventory != null)
                {
                    if (ParentObject.pPhysics.Takeable)
                    {
                        E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("plant", 'p', false, "&Wp&ylant", "InvCommandPlant", 5);
                    }
                }
            }
            else if (E.ID == "InvCommandPlant")
            {
                Plant(E.GetParameter<GameObject>("Owner"));
                E.RequestInterfaceExit();
            }
            else if (E.ID == "CanSmartUse")
            {
                return false;
            }
            if (E.ID == "EndTurn" || E.ID == "AccelerateRipening")
            {
                growthTick();
            }
            if (E.ID == "GetShortDescription" && this.planted)
            {
                if (Scanning.HasScanningFor(XRLCore.Core.Game.Player.Body, Scanning.Scan.Bio)){
                    int days = (int)Math.Floor((double)(growthTime-growth) / 1200);
                    int hours = (int)Math.Floor((double)((growthTime - growth) % 1200) / (1200 / 24));

                    E.SetParameter("Postfix", E.GetParameter("Postfix") + "\n&gScans indicate the plant will be fully grown in " + days.ToString() + "d " + hours.ToString() + "h.");
                }
            }
            if (E.ID == "GetDisplayName" || E.ID == "GetShortDisplayName")
            {
                if (this.planted)
                {
                    E.AddParameter("DisplayName", new StringBuilder(this.ResultName + " " + this.displayname));
                }
            }
            return base.FireEvent(E);


        }
    }
}