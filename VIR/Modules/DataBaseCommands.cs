﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIR.Modules.Preconditions;
using VIR.Services;

namespace VIR.Modules
{
    public class DataBaseCommands : ModuleBase<SocketCommandContext>
    {
        //TODO: Comment this
        private readonly DataBaseHandlingService DataBaseHandlingService;
        private readonly AgeService AgeService;

        public DataBaseCommands(DataBaseHandlingService db, AgeService age)
        {
            DataBaseHandlingService = db;
            AgeService = age;
        }

        [Command("balance")]
        public async Task BalanceAsync(IUser user = null)
        {
            string aget;
            int age;
            if (user == null)
            {
                user = user ?? Context.User;
                aget = await DataBaseHandlingService.GetFieldAsync(user.Id.ToString(), "age");
                if (aget == null)
                {
                    Random rand = new Random(); //Set up a RNG
                    age = rand.Next(20, 25); //Get num between 20 and 25
                    await DataBaseHandlingService.SetFieldAsync(user.Id.ToString(), "age", age);
                }
                else
                {
                    age = int.Parse(aget);
                }
                string PPt;
                int pp;
                PPt = await DataBaseHandlingService.GetFieldAsync(user.Id.ToString(), "pp");
                if(PPt == null) {
                    pp = 0;
                    await DataBaseHandlingService.SetFieldAsync(user.Id.ToString(), "pp", pp);
                } else
                {
                    pp = int.Parse(PPt);
                }
                EmbedFieldBuilder ppField = new EmbedFieldBuilder().WithIsInline(false).WithName("PP:").WithValue(pp.ToString());
                Embed embedd = new EmbedBuilder().WithImageUrl(Context.User.GetAvatarUrl()).WithFooter("Brought to you by Ohcitrade").WithTitle($"Inventory of {Context.User.Username}").WithDescription($"Age: {age.ToString()}").AddField(ppField).Build();
                await user.SendMessageAsync("", false, embedd);
                await ReplyAsync("Your balance was sent to you privately.");
                return;
            }
            aget = await DataBaseHandlingService.GetFieldAsync(user.Id.ToString(), "age");
            if(aget == null) {
                Random rand = new Random(); //Set up a RNG
                age = rand.Next(20, 25); //Get num between 20 and 25
                await DataBaseHandlingService.SetFieldAsync(user.Id.ToString(), "age", age);
            } else
            {
                age = int.Parse(aget);
            }
            await ReplyAsync($"Age of {user.Mention}: {age.ToString()}");
        }

        [Command("get")]
        [HasMasterOfBots]
        [IsInDPSGuild]
        public async Task GetAsync([Summary("Field to get")] string field, [Summary("User")] IUser user)
        {
            string result = await DataBaseHandlingService.GetFieldAsync(user.Id.ToString(), field);
            await ReplyAsync($"{field} value: {result}");
        }

        [Command("set")]
        [HasMasterOfBots]
        [IsInDPSGuild]
        public async Task SetAsync([Summary("Field to get")] string field, [Summary("User")] IUser user, [Summary("Value to set to")] int value)
        {
            await DataBaseHandlingService.SetFieldAsync(user.Id.ToString(), field, value);
            await ReplyAsync($"{field} value set to {value}");
        }

        [Command("add")]
        [HasMasterOfBots]
        [IsInDPSGuild]
        public async Task AddAsync([Summary("Field to get")] string field, [Summary("User")] IUser user, [Summary("Value to add")] int value)
        {
            string x = await DataBaseHandlingService.GetFieldAsync(user.Id.ToString(), field);
            int val;
            if (x == null)
            {
                val = 0;
            }
            else
            {
                val = int.Parse(x);
            }
            await DataBaseHandlingService.SetFieldAsync(user.Id.ToString(), field, value + val);
            await ReplyAsync($"{field} value set to {value}");
        }

        [Command("advance")]
        [HasMasterOfBots]
        [IsInDPSGuild]
        public async Task AdvanceAsync([Summary("Years to advance")] string years)
        {
            int yearsi = int.Parse(years);
            SocketGuild guild = Context.Guild;
            await AgeService.AdvanceAllAsync(guild, yearsi);
            await ReplyAsync($"Time advanced by {years}");
        }

        [Command("kill")]
        [HasMasterOfBots]
        [IsInDPSGuild]
        public async Task KillAsync([Summary("Person to kill")] IUser user)
        {
            var userg = user as IGuildUser;
            await AgeService.KillAsync(userg);
            await ReplyAsync($"Killed {user.Mention}");
        }
    }
}
