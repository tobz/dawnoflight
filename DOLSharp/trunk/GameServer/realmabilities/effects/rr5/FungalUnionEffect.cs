using System.Collections;
using System;
using System.Collections.Generic;
using DawnOfLight;
using DawnOfLight.GameServer;
using DawnOfLight.Events;
using DawnOfLight.Database;
using DawnOfLight.GameServer.PacketHandler;

namespace DawnOfLight.GameServer.Effects
{

    public class FungalUnionEffect : TimedEffect
    {
        private GameLiving owner;

        public FungalUnionEffect() : base(60000) { }


        public override void Start(GameLiving target)
        {
            base.Start(target);
            owner = target;
            GamePlayer player = target as GamePlayer;
            if (player != null)
            {
                player.Model = 1648;
            }
        }

        public override void Stop()
        {
            base.Stop();
            GamePlayer player = owner as GamePlayer;
            if (player is GamePlayer)
            {
                player.Model = (ushort)player.DBCharacter.CreationModel;
            }
        }


        public override string Name { get { return "Fungal Union"; } }


        public override ushort Icon { get { return 3061; } }


        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("Turns the animist into a mushroom for 60 seconds. Does not break on attack. Grants the animist a 10% chance of not spending power for each spell cast during the duration.");
                return list;
            }
        }
    }
}