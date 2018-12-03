﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIR.Services
{
    public class AgeService
    {
        //TODO: Comment this mess too
        private readonly DataBaseHandlingService __database;

        public AgeService(IServiceProvider services, DataBaseHandlingService database)
        {
            __database = database;
        }
        public async Task AdvanceAllAsync(SocketGuild guild, int amount)
        {
            IGuild guildx = (IGuild) guild;
            IReadOnlyCollection<IGuildUser> users = await guildx.GetUsersAsync();
            foreach (IGuildUser x in users)
            {
                if (x.IsBot)
                {
                    continue;
                }
                string ageTemporary;
                int age;
                ageTemporary = await __database.GetFieldAsync(x.Id.ToString(), "age");
                if(ageTemporary == null) {
                    Random rand = new Random(); //Set up a RNG
                    age = rand.Next(20, 25); //Get num between 20 and 25
                } else {
                    age = int.Parse(ageTemporary);
                }
                int ageNew = age + amount;
                await __database.SetFieldAsync(x.Id.ToString(), "age", ageNew);
                int pp;
                string ppTemporary;
                ppTemporary = await __database.GetFieldAsync(x.Id.ToString(), "pp");
                if(ppTemporary == null) {
                    pp = 0;
                } else {
                    pp = int.Parse(ppTemporary);
                }
                int newpp = pp - (int) Math.Floor(((double.Parse(pp.ToString()) / 100) * amount));
                await __database.SetFieldAsync(x.Id.ToString(), "pp", newpp);
                int expectancy;
                string expectancyTemporary;
                expectancyTemporary = await __database.GetFieldAsync(x.Id.ToString(), "expectancy");
                if(expectancyTemporary == null) {
                    Random rand = new Random(); //Set up a RNG
                    expectancy = rand.Next(90, 130); //Get num between 90 and 130
                    await __database.SetFieldAsync(x.Id.ToString(), "expectancy", expectancy);
                } else {
                    expectancy = int.Parse(expectancyTemporary);
                }
                if(ageNew >= expectancy) {
                    await KillAsync(x);
                }
            }
        }
        public async Task KillAsync(IGuildUser user)
        {
            if(user.IsBot)
            {
                return;
            }
            await __database.RemoveUserAsync(user.Id.ToString());
            await user.SendMessageAsync("You are dead");
            SocketGuildUser userSocket = (SocketGuildUser) user;
            await user.RemoveRolesAsync(userSocket.Roles.Where(x => (x.Position < 91 && x.Position > 34)));
            await user.ModifyAsync(delegate(GuildUserProperties prop) {
                prop.Nickname = user.Username;
            });
        }
    }
}
