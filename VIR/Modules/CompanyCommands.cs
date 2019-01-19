﻿using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VIR.Modules.Objects.Company;
using VIR.Modules.Preconditions;
using VIR.Services;
using VIR.Objects;

namespace VIR.Modules
{
    /// <summary>
    /// Contains async methods regarding corporations
    /// </summary>
    public class CompanyCommands : ModuleBase<SocketCommandContext>
    {
        private readonly CompanyService CompanyService;
        private readonly DataBaseHandlingService dataBaseService;
        private readonly CommandHandlingService CommandService;
        private readonly StockMarketService MarketService;
        public readonly List<int> r = new List<int> { 4, 5, 6, 7 };
        public readonly List<int> w = new List<int> { 2, 3, 6, 7 };
        public readonly List<int> e = new List<int> { 1, 3, 5, 7 };

        public CompanyCommands(CompanyService com, DataBaseHandlingService db, CommandHandlingService comm, StockMarketService markserv)
        {
            CompanyService = com;
            dataBaseService = db;
            CommandService = comm;
            MarketService = markserv;
        }

        [Command("createcompany")]
        [Alias("createcorporation", "addcompany", "addcorporation")]
        [HasMasterOfBots]
        public async Task CreateCompanyTask(string ticker, [Remainder]string name)
        {
            Company company = new Company();
            company.name = name;
            company.shares = 0;
            company.id = ticker;
            company.employee = new Dictionary<string, Employee>();
            company.positions = new Dictionary<string, Position>();
            Employee employee = new Employee();
            employee.userID = Context.User.Id.ToString();
            employee.salary = 0;
            employee.wage = 0;
            employee.wageEarned = 0;
            Position position = new Position();
            position.ID = "CEO";
            position.level = 99;
            position.manages = 7;
            position.name = "CEO";
            employee.position = position;
            company.employee.Add(Context.User.Id.ToString(), employee);
            company.positions.Add(position.ID, position);
            await CompanyService.setCompany(company);
            JObject user = await dataBaseService.getJObjectAsync(Context.User.Id.ToString(), "users");
            Collection<string> corps = new Collection<string>();
            try {
                foreach (string x in (Array) user["corps"])
                {
                    corps.Add(x);
                }
            } catch {}
            corps.Add(ticker);
            await dataBaseService.SetFieldAsync(Context.User.Id.ToString(), "corps", JArray.FromObject(corps.ToArray()), "users");
            await ReplyAsync("Company successfully created!");
        }

        [Command("companies")]
        [Alias("corporations")]
        public async Task GetCompaniesTask()
        {
            /*string tmp = "";
            Collection<JObject> ids = await dataBaseService.getJObjects("companies");
            foreach(JObject x in ids)
            {
                tmp += (string)x["id"] + " - " + (string)x["name"] + "\n";
            }
            await ReplyAsync($"Current Companies:\n{tmp}");*/

            Collection<string> ids = await dataBaseService.getIDs("companies");
            int companyCount = ids.Count;
            Collection<EmbedFieldBuilder> companyEmbedList = new Collection<EmbedFieldBuilder>();

            foreach(string ID in ids)
            {
                Company temp = new Company(await dataBaseService.getJObjectAsync(ID, "companies"));

                EmbedFieldBuilder tempEmb = new EmbedFieldBuilder().WithIsInline(true).WithName($"{temp.name} ({temp.id})").WithValue($"Share Price: ${temp.SharePrice}. Total Value: ${temp.SharePrice * temp.shares}. Amount of Shares: {await MarketService.CorpShares(temp.id)}");

                companyEmbedList.Add(tempEmb);
            }

            EmbedBuilder embed = new EmbedBuilder().WithColor(Color.Gold).WithTitle("Companies").WithDescription("This is a list of all companies").WithFooter($"Total amount of companies: {companyCount}");

            foreach(EmbedFieldBuilder field in companyEmbedList)
            {
                embed.AddField(field);
            }

            await CommandService.PostEmbedTask(Context.Channel.Id.ToString(), embed.Build());
        }

        [Command("addposition")]
        public async Task AddPositionToPlayer(IUser user, string companyTicker, string positionID)
        {
            Company company = await CompanyService.getCompany(companyTicker);
            if (!company.employee.ContainsKey(Context.User.Id.ToString()))
                await ReplyAsync("You are not part of this corporation!");
            if (e.Contains(company.employee[Context.User.Id.ToString()].position.manages)) 
            {
                if(!company.employee.ContainsKey(user.Id.ToString()))
                {
                    await ReplyAsync("The User specified is not a part of this corporation!");
                } else
                {
                    if (company.positions.ContainsKey(positionID)) {
                        company.employee[user.Id.ToString()].position = company.positions[positionID];
                        await ReplyAsync($"Successfully changed {user.Mention} to position of {company.positions[positionID].name}");
                    } else
                    {
                        await ReplyAsync("The position id you specified is invalid.");
                    }
                }
            }
        }

        [Command("deletecompany")]
        [Alias("deletecorporation","removecorporation","removecompany")]
        [HasMasterOfBots]
        public async Task RemoveCorpAsync(string ticker)
        {
            Collection<string> tickers = await dataBaseService.getIDs("companies");

            if (!tickers.Contains(ticker))
            {
                await ReplyAsync("That company does not exist");
            }
            else
            {
                await dataBaseService.RemoveObjectAsync(ticker, "companies");

                Collection<string> shareholderIDs = await dataBaseService.getIDs("shares");

                foreach (string ID in shareholderIDs)
                {
                    UserShares shares = new UserShares(await dataBaseService.getJObjectAsync(ID, "shares"), true);

                    shares.ownedShares.Remove(ticker);

                    await dataBaseService.RemoveObjectAsync(ID, "shares");
                    await dataBaseService.SetJObjectAsync(dataBaseService.SerializeObject<UserShares>(shares), "shares");
                }

                await ReplyAsync("Company deleted");
            }
        }

        [Command("createPosition")]
        public async Task createPosition(string ticker, string positionID, int level, int manages, [Remainder] string name)
        {
            Company company = await CompanyService.getCompany(ticker);
            if (!company.employee.ContainsKey(Context.User.Id.ToString()))
                await ReplyAsync($"You are not an employee in {company.name}");
            if (w.Contains(company.employee[Context.User.Id.ToString()].position.manages))
                await ReplyAsync("You do not have the permission to make/manage positions.");
            if (manages > 7 || manages < 0)
                await ReplyAsync("Manages must be in range of 0 to 7.");
            Position pos = new Position();
            pos.ID = positionID;
            pos.level = level;
            pos.manages = manages;
            pos.name = name;
            company.positions.Add(positionID, pos);
        }
    }
}
