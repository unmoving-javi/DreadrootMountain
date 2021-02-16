using System;
using XRL.Rules;
using XRL.UI;
using XRL.Core;
using XRL.World.Parts.Skill;
using System.Collections.Generic;

//Almost all of this code is based on the Starapple Valley project by Acegiak, which is under no license.
//Their github can be found at https://github.com/acegiak/QudStarappleValley/
//This code is admittedly a bit of a frankenstein of their code and other stuff from the base game, not much here is original.
//Please check out their work as well! Without them this would have been much, much harder.

namespace XRL.World.Parts
{
    [Serializable]
    public class DMSeedDropper : IPart
    {
        public int Chance = 35;

        public string Seed = null;

        public long last = 0;

        public bool hasSeed = false;

        public bool bNoSmartUse;

        public string hasDomestic = "false";

        public DMSeedDropper()
        {
        }


        public override bool SameAs(IPart p)
        {
            return base.SameAs(p);
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "BeforeDeathRemoval");
            Object.RegisterPartEvent(this, "CommandSmartUse");
            Object.RegisterPartEvent(this, "CanSmartUse");
            base.Register(Object);

        }


        public GameObject CreateSeed()
        {
            if (Seed != null)
            {
                return GameObject.create(Seed);
            }
            GameObject seed = GameObject.create("Seed");
            DMSeed seedpart = seed.GetPart<DMSeed>();

            if (this.hasDomestic == "true")
            {
                seedpart.Result = "Domestic " + ParentObject.GetBlueprint().Name;
                if (seedpart.Result == null)
                {
                    //Failsafe for a plant lacking a domestic version still entering this code.
                    seedpart.Result = ParentObject.GetBlueprint().Name;
                }

            } else
            {
                seedpart.Result = ParentObject.GetBlueprint().Name;
            }
            
            seedpart.ResultName = ParentObject.GetBlueprint().DisplayName().Replace(" Tree", "");
            seedpart.displayname = "seed";
            seedpart.description = "A seed from " + ParentObject.a + ParentObject.DisplayNameOnly + ".";
            seed.pRender.DisplayName = seedpart.ResultName + " " + seedpart.displayname;
            seed.pRender.ColorString = ParentObject.pRender.ColorString;
            seed.pRender.TileColor = ParentObject.pRender.TileColor;
            seed.pRender.DetailColor = ParentObject.pRender.DetailColor;
            return seed;
        }


        public override bool FireEvent(Event E)
        {

            if (E.ID == "BeforeDeathRemoval")
            {
                Cell dropCell = ParentObject.GetDropCell();
                if (dropCell != null)
                {
                    if (Chance != 0 && Stat.Rnd2.NextDouble() < Chance / 100f)
                    {
                        dropCell.AddObject(CreateSeed());
                    }
                }
            }
            if (E.ID == "CanSmartUse")
            {
                return false;
            }

            return base.FireEvent(E);


        }
    }


}