using System.Collections;
using System.Collections.Generic;
using DawnOfLight.GameServer.commands.Player;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects.CustomNPC;
using DawnOfLight.GameServer.RealmAbilities.handlers;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerTrain, ClientStatus.PlayerInGame)]
    public class PlayerTrainHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GamePacketIn packet)
        {
            // A trainer of the appropriate class must be around (or global trainer, with TrainedClass = eCharacterClass.Unknow
            GameTrainer trainer = client.Player.TargetObject as GameTrainer;
            if (trainer == null || (trainer.CanTrain(client.Player) == false && trainer.CanTrainChampionLevels(client.Player) == false))
            {
                client.Out.SendMessage("You must select a valid trainer for your class.", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
                return;
            }

            //Specializations - 8 trainable specs max
            uint size = 8;
            long position = packet.Position;
            IList<uint> skills = new List<uint>();
            Dictionary<uint, uint> amounts = new Dictionary<uint, uint>();
            bool stop = false;
            for (uint i = 0; i < size; i++)
            {
                uint code = packet.ReadInt();
                if (!stop)
                {
                    if (code == 0xFFFFFFFF) stop = true;
                    else
                    {
                        if (!skills.Contains(code))
                            skills.Add(code);
                    }
                }
            }

            foreach (uint code in skills)
            {
                uint val = packet.ReadInt();

                if (!amounts.ContainsKey(code) && val > 1)
                    amounts.Add(code, val);
            }

            IList specs = client.Player.GetSpecList();
            uint skillcount = 0;
            IList<string> done = new List<string>();
            bool trained = false;

            // Graveen: the trainline command is called
            foreach (Specialization spec in specs)
            {
                if (amounts.ContainsKey(skillcount))
                {
                    if (spec.Level < amounts[skillcount])
                    {
                        TrainCommandHandler train = new TrainCommandHandler(true);
                        train.OnCommand(client, new string[] { "&trainline", spec.KeyName, amounts[skillcount].ToString() });
                        trained = true;
                    }
                }
                skillcount++;
            }

            //RealmAbilities
            packet.Seek(position + 64, System.IO.SeekOrigin.Begin);
            size = 50;//50 RA's max?
            amounts.Clear();
            for (uint i = 0; i < size; i++)
            {
                uint val = packet.ReadInt();

                if (val > 0 && !amounts.ContainsKey(i))
                {
                    amounts.Add(i, val);
                }
            }
            uint index = 0;
            if (amounts != null && amounts.Count > 0)
            {
                List<RealmAbility> ras = SkillBase.GetClassRealmAbilities(client.Player.CharacterClass.ID);
                foreach (RealmAbility ra in ras)
                {
                    if (ra is RR5RealmAbility)
                        continue;

                    if (amounts.ContainsKey(index))
                    {
                        RealmAbility playerRA = (RealmAbility)client.Player.GetAbility(ra.KeyName);
                        if (playerRA != null
                            && (playerRA.Level >= ra.MaxLevel || playerRA.Level >= amounts[index]))
                        {
                            index++;
                            continue;
                        }

                        int cost = 0;
                        for (int i = playerRA != null ? playerRA.Level : 0; i < amounts[index]; i++)
                            cost += ra.CostForUpgrade(i);
                        if (client.Player.RealmSpecialtyPoints < cost)
                        {
                            client.Out.SendMessage(ra.Name + " costs " + (cost) + " realm ability points!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
                            client.Out.SendMessage("You don't have that many realm ability points left to get this.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
                            index++;
                            continue;
                        }
                        if (!ra.CheckRequirement(client.Player))
                        {
                            client.Out.SendMessage("You are not experienced enough to get " + ra.Name + " now. Come back later.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
                            index++;
                            continue;
                        }

                        bool valid = false;
                        if (playerRA != null)
                        {
                            playerRA.Level = (int)amounts[index];
                            valid = true;
                        }
                        else
                        {
                            RealmAbility ability = SkillBase.GetAbility(ra.KeyName, (int)amounts[index]) as RealmAbility;
                            if (ability != null)
                            {
                                valid = true;
                                client.Player.AddAbility(ability, false);
                            }
                        }
                        if (valid)
                        {
                            client.Player.RealmSpecialtyPoints -= cost;
                            client.Out.SendUpdatePoints();
                            client.Out.SendUpdatePlayer();
                            client.Out.SendCharResistsUpdate();
                            client.Out.SendCharStatsUpdate();
                            client.Out.SendUpdatePlayerSkills();
                            client.Out.SendTrainerWindow();
                            trained = true;
                        }
                        else
                        {
                            client.Out.SendMessage("Unfortunately your training failed. Please report that to admins or game master. Thank you.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
                        }
                    }

                    index++;
                }
            }

            if (trained)
                client.Player.SaveIntoDatabase();
        }
    }
}
