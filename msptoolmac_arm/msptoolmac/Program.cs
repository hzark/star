﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using StarService.Enum;
using StarService.Utility;
using static StarService.Utility.AMFCall;
using static StarService.Utility.ChecksumCalculator;
using static StarService.Utility.SignatureCalculator;
using static msptool.localisation;
using Rule = Spectre.Console.Rule;
using WebClient = System.Net.WebClient;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;

namespace msptool
{
    internal class Program
    {
        private static readonly string currentVersion = "1.6";

        private static readonly string checkVersion =
            "https://raw.githubusercontent.com/lcfidev/star/main/msptool/version.txt";
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var spt1 = @"
                                                                                   
                                                   .=+**+-.                        
                                                  =*######*-                       
                                                 :###*--*###.                      
                                           :--===*##*-  =###*===--.                
                                         :*#########+   .+#########+:              
                                        :*###======-.    .-=====+###*.             
                                      .:=###*-.                :=###*.             
                                 :-+**########*+:.          .-+####*:              
                             .-+*################*         .*####+:                
                           :+####################=          =###+                  
                         -*#####################+.  .-**-.  .*###-                 
                       :*#######################=:-+#####*+-.+###=                 
                     .=###################*+=########*++*#######*.                 
                    -*###################+:  .=+*#*+-.  .-+*#*+-.                  
                  .=####################-                                          
                 .+###################*:                                           
                .+###################*:                                            
               .*###################*:                                             
              .+###################*-                                              
              =####################=                                               
             .*#*+-=##############*.                                               
              ::. .+##############=                                                
                  :*######*=#####*.                                                
                  :######*: =####=                                                 
                  -#####+.  .+###-                                                 
                  -####=     .+**.                                                 
                  :*#*:        ::                                                  
                  .=+:                                                             
                                                                                   
                                                                                   
";

            AnsiConsole.Clear();
            AnsiConsole.WriteLine(spt1);
            AnsiConsole.MarkupLine("[#71d5fb]Star Project by lcfi & 6c0[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("#71d5fb"))
                .Start("Loading...", ctx =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(50);
                        ctx.Refresh();
                    }
                });

            Console.Clear();

            if (!isCurrentVersion())
            {
                HttpClient client = new HttpClient();
                string latestVersion = client.GetStringAsync(checkVersion).Result;
                AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Update").LeftJustified());
                Console.Write("\n");
                AnsiConsole.Markup(
                    $"[#71d5fb]Go on and download last release[/] ・ [link=https://github.com/lcfidev/star/releases/tag/v{latestVersion}]github.com/l3c1d/star/releases/tag/v{latestVersion}[/]");
                Console.ReadKey();
                while (true)
                {
                    Console.Write("\x1b[94mSTAR\x1b[39m ・ Update\n\n");
                    Console.WriteLine("[\x1b[95m!\u001b[39m] \u001b[93mAn update was found !\n");
                    Console.WriteLine("\u001b[94m1\u001b[39m > Install new update");
                    Console.WriteLine("\u001b[94m2\u001b[39m > Update manually\n");
                    Console.Write("[\u001b[95mUPDATE\u001b[39m] Pick an option: ");
                    string options = Console.ReadLine();
                    switch (options)
                    {
                        case "1":
                            Console.Write("Soon");
                            return;
                        case "2":
                            Console.WriteLine(
                                "\n\x1b[95mUPDATE\u001b[39m > \x1b[93mGo on https://github.com/l3c1d/star [Click any key to close]");
                            Console.ReadKey();
                            return;
                        default:
                            Console.WriteLine("\n\u001b[91mERROR\u001b[39m > \u001b[93mChoose a option which exists !");
                            System.Threading.Thread.Sleep(2000);
                            Console.Clear();
                            break;
                    }
                }
            }

            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Choose").LeftJustified());
            Console.Write("\n");

            var selectedLogin = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[[[#71d5fb]+[/]]] Select which MSP you want to use")
                    .PageSize(3)
                    .AddChoices(new[] { "MovieStarPlanet", "MovieStarPlanet 2" })
            );

            if (selectedLogin == "MovieStarPlanet")
                MSP1_Login();
            else
                MSP2_Login();

        }

        static void MSP1_Login()
        {
            Console.Clear();
            bool loggedIn = false;
            while (!loggedIn)
            {
                AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Login MSP").LeftJustified());
                Console.Write("\n");
                var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter username: ")
                    .PromptStyle("#71d5fb"));

                var password = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter password: ")
                    .PromptStyle("#71d5fb")
                    .Secret());

                var choices = Enum.GetValues(typeof(WebServer))
                    .Cast<WebServer>()
                    .Select(ws => (ws.loc3().Item1, ws.loc3().Item2))
                    .ToArray();

                var selectedCountry = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[[[#71d5fb]+[/]]] Select a server: ")
                        .PageSize(15)
                        .MoreChoicesText("[grey](Move up and down to reveal more servers)[/]")
                        .AddChoices(choices.Select(choice => choice.Item1))
                );

                var selectedChoice = choices.First(choice => choice.Item1 == selectedCountry);
                dynamic login = null;
                string server = selectedChoice.Item2;
                if (Msptoolhome.TryGetValue(server, out var allmsptools))
                    AnsiConsole.Status()
                        .SpinnerStyle(Spectre.Console.Style.Parse("#71d5fb"))
                        .Start("Login...", ctx =>
                        {
                            ctx.Refresh();
                            ctx.Spinner(Spinner.Known.Circle);
                            login = AMFConn(server, "MovieStarPlanet.WebService.User.AMFUserServiceWeb.Login",
                                new object[6]
                                {
                                username, password, new object[] {  }, null, null, "MSP1-Standalone:XXXXXX"
                                });
                            Thread.Sleep(1000);
                        });

                if (login == null)
                {
                    Console.WriteLine(
                        "\n\x1b[91mFAILED\u001b[39m > \x1b[93mUnknown [Click any key to return to login]");
                    Console.ReadKey();
                    Console.Clear();
                }

                if (login["loginStatus"]["status"] != "Success")
                {
                    Console.WriteLine(
                        "\n\x1b[91mFAILED\u001b[39m > \x1b[93mLogin failed [Click any key to return to login]");
                    Console.ReadKey();
                    Console.Clear();
                }
                else
                {
                    loggedIn = true;
                    int actorId = login["loginStatus"]["actor"]["ActorId"];
                    string name = login["loginStatus"]["actor"]["Name"];
                    string ticket = login["loginStatus"]["ticket"];
                    string accessToken = login["loginStatus"]["nebulaLoginStatus"]["accessToken"];
                    string profileId = login["loginStatus"]["nebulaLoginStatus"]["profileId"];
                    var th = new JwtSecurityTokenHandler();
                    var jtoken = th.ReadJwtToken(accessToken);
                    var loginId = jtoken.Payload["loginId"].ToString();
                    Console.Clear();

                    while (true)
                    {
                        AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home").LeftJustified().RoundedBorder());
                        Console.Write("\n");
                        foreach (var eachtool in allmsptools)
                        {
                            AnsiConsole.Markup($"[#71d5fb]{eachtool.Key}[/]  > {eachtool.Value}\n");
                        }
                        AnsiConsole.Write(
                            new Rule(
                                    "[slowblink][#71d5fb]lcfi & 6c0[/][/]")
                                .RightJustified().RoundedBorder());
                        var options = AnsiConsole.Prompt(new TextPrompt<string>("\n[[[#71d5fb]+[/]]] Pick an option: ")
                            .PromptStyle("#71d5fb"));

                        switch (options)
                        {
                            case "1":
                                recycleNoneRareClothes(server, actorId, ticket);
                                break;
                            case "2":
                                buyBoonie(server, actorId, ticket);
                                break;
                            case "3":
                                buyAnimation(server, actorId, ticket);
                                break;
                            case "4":
                                buyClothes(server, actorId, ticket);
                                break;
                            case "5":
                                buyEyes(server, actorId, ticket);
                                break;
                            case "6":
                                buyNose(server, actorId, ticket);
                                break;
                            case "7":
                                buyLips(server, actorId, ticket);
                                break;
                            case "8":
                                wearRareSkin(server, actorId, ticket);
                                break;
                            case "9":
                                addToWishlist(server, ticket);
                                break;
                            case "10":
                                customStatus(server, name, actorId, ticket);
                                break;
                            case "11":
                                addSponsors(server, ticket);
                                break;
                            case "12":
                                blockDefaults(server, actorId, ticket);
                                break;
                            case "13":
                                recycleitems(server, actorId, ticket);
                                break;
                            case "14":
                                wheelspins(server, actorId, ticket);
                                break;
                            case "15":
                                lisaHack(server, actorId, ticket);
                                break;
                            case "16":
                                automatedPixeller(server, ticket);
                                break;
                            case "17":
                                query(server, actorId, ticket);
                                break;
                            case "18":
                                usernameChecker();
                                break;
                            case "19":
                                clothesExtractor(server, ticket);
                                break;
                            case "20":
                                usernameToActorid(server);
                                break;
                            case "21":
                                actorIdToUsername(server);
                                break;
                            case "22":
                                itemTracker(server);
                                break;
                            case "23":
                                roomChanger(server, actorId, ticket);
                                break;
                            case "24":
                                animationsExtractor(server, ticket);
                                break;
                            case "25":
                                Console.WriteLine("\n\x1b[97mBYE\u001b[39m > \u001b[93mLogging out...");
                                Console.Clear();
                                loggedIn = false;
                                break;
                            default:
                                Console.WriteLine(
                                    "\n\u001b[91mERROR\u001b[39m > \u001b[93mChoose a option which exists !");
                                System.Threading.Thread.Sleep(2000);
                                Console.Clear();
                                break;
                        }

                        if (!loggedIn)
                            break;
                    }
                }
            }
        }

        static void recycleNoneRareClothes(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Recycle None-Rare Clothes")
                .LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            dynamic cloth = AMFConn(server,
                "MovieStarPlanet.WebService.ActorClothes.AMFActorClothes.GetActorClothesRelMinimals",
                new object[2]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId
                });

            foreach (dynamic obj in cloth)
            {
                object actorClothesRelId = obj["ActorClothesRelId"];

                dynamic clothinfo = AMFConn(server,
                    "MovieStarPlanet.WebService.MovieStar.AMFMovieStarService.GetActorClothesRel",
                    new object[1] { actorClothesRelId });

                string shop_id = clothinfo["Cloth"]["ShopId"].ToString();
                string cloth_name = clothinfo["Cloth"]["Name"] ?? "Unknown";

                if (shop_id != "-100")
                {
                    dynamic recycler = AMFConn(server,
                        "MovieStarPlanet.WebService.Profile.AMFProfileService.RecycleItem",
                        new object[4]
                        {
                                new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                                actorId,
                                actorClothesRelId,
                                0
                        });
                    AnsiConsole.Markup($"[[[#71d5fb]![/]]] Recycled {cloth_name}");
                }
            }

            AnsiConsole.Markup(
                "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Finished recycling[/] [[Click any key to return to Home]][/]");
            Console.ReadKey();
            Console.Clear();
        }

        static void buyBoonie(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Buy Boonie").LeftJustified()
                .RoundedBorder());

            var bonnieOptions = new (string Name, int Value)[]
            {
                    ("Light Side Boonie", 1),
                    ("Dark Side Boonie", 2),
                    ("VIP Boonie", 3),
                    ("FOX", 4),
                    ("DOG", 5),
                    ("PLANT", 6),
                    ("DRAGON", 7),
                    ("Metat Eater", 8),
                    ("Xmas Boonie", 9),
                    ("Valentine Boonie", 10),
                    ("Diamond Boonie", 11),
                    ("Easter Bunny", 12),
                    ("Diamond Squirrel", 13),
                    ("Poodle", 14),
                    ("Summer Boonie", 15),
                    ("Gamer Bunny", 16),
                    ("Brad Pet", 17),
                    ("Magazine Pet", 18),
                    ("Puppy", 19),
                    ("Halloween Boonie", 20),
                    ("Space Boonie", 21),
                    ("Xmax Boonie 2012", 22),
                    ("New Years Boonie 2012", 23),
                    ("Elements 2013 Boonie", 24),
                    ("Valentines 2013 Boonie", 25),
                    ("Australia 2013 Boonie", 26),
                    ("EgmontMagazine1Boonie", 27),
                    ("Easter 2013 Boonie", 29),
                    ("Tutti Frutti 2013 Boonie", 30),
                    ("Birthday 2013 Boonie", 31),
                    ("Mexican 2013 Boonie", 32),
                    ("Fastfood 2013 Boonie", 33),
                    ("Rio 2013 Boonie", 34),
                    ("Night Sky 2013 Boonie", 35),
                    ("Wonderland Boonie", 37),
                    ("Robots 2013", 38),
                    ("Halloween 2013", 39),
                    ("Winter Wonderland 2013", 40),
                    ("Christmas 2013", 41),
                    ("New Year 2013", 42),
                    ("Egmont Mag 10", 43),
                    ("Egmont Mag 2014", 46)
            };

            var selectedBoonie = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[[[#71d5fb]+[/]]] Select a boonie: ")
                    .PageSize(10)
                    .AddChoices(bonnieOptions.Select(choice => choice.Name))
            );

            var selectedChoice = bonnieOptions.First(choice => choice.Name == selectedBoonie);


            dynamic boonie = AMFConn(server,
                "MovieStarPlanet.WebService.Pets.AMFPetService.BuyClickItem",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        selectedChoice.Value
                });
            if (boonie["SkinSWF"] != "femaleskin" && boonie["SkinSWF"] != "maleskin")
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Boonie bought![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void buyAnimation(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Buy Animations").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            int animationId = AnsiConsole.Prompt(
                new TextPrompt<int>("[[[#71d5fb]+[/]]] Enter AnimationId: ")
                    .PromptStyle("#71d5fb"));

            dynamic animation = AMFConn(server,
                "MovieStarPlanet.WebService.Spending.AMFSpendingService.BuyAnimation",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        animationId
                });

            if (animation["Code"] != 0)
            {
                AnsiConsole.Markup("\n[#fa1414]FAILED[/] > [#f7b136][underline]"
                                   + (animation["Description"] ?? "Unknown") +
                                   "[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Animation bought![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void buyClothes(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Buy Clothes").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            int clothId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] Enter ClothesId: ")
                .PromptStyle("#71d5fb"));
            string clothColor = AnsiConsole.Prompt(
                new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter Color: ")
                    .PromptStyle("#71d5fb"));
            try
            {
                dynamic cloth = AMFConn(server, "MovieStarPlanet.WebService.AMFSpendingService.BuyClothes",
                    new object[4]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        new object[]
                        {
                            new
                            {
                                Color = clothColor,
                                y = 0,
                                ActorClothesRelId = 0,
                                ActorId = actorId,
                                ClothesId = clothId,
                                IsWearing = 1,
                                x = 0
                            },
                        },
                        0
                    });

                if (cloth["Code"] != 0)
                {
                    AnsiConsole.Markup("\n[#fa1414]FAILED[/] > [#f7b136][underline]"
                                       + (cloth["Description"] ?? "Unknown") +
                                       "[/] [[Click any key to return to Home]][/]");
                    Console.ReadKey();
                    Console.Clear();
                }
                else
                {
                    AnsiConsole.Markup(
                        "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Clothing bought![/] [[Click any key to return to Home]][/]");
                }
            }
            catch (Exception)
            {
                AnsiConsole.Markup("\n[#fa1414]FAILED[/] > Hidden or Deleted [#f7b136][underline]"
                                   +
                                   "[/] [[Click any key to return to Home]][/]");
            }
            Console.ReadKey();
            Console.Clear();
        }

        static void buyNose(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Buy Nose").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            int noseId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] Enter NoseId: ")
                .PromptStyle("#71d5fb"));

            dynamic nose = AMFConn(server,
                "MovieStarPlanet.WebService.BeautyClinic.AMFBeautyClinicService.BuyManyBeautyClinicItems",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        new object[]
                        {
                            new
                            {
                                IsOwned = false,
                                Type = 4,
                                IsWearing = true,
                                InventoryId = 0,
                                ItemId = noseId,
                                Colors = "",

                            }
                        }
                });
            if (nose[0]["InventoryId"] == 0)
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Nose bought![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void buyLips(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Buy Lips").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            int lipsId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] Enter LipsId: ")
                .PromptStyle("#71d5fb"));
            string lipsColor = AnsiConsole.Prompt(
                new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter Color: ")
                    .PromptStyle("#71d5fb"));

            dynamic lips = AMFConn(server,
                "MovieStarPlanet.WebService.BeautyClinic.AMFBeautyClinicService.BuyManyBeautyClinicItems",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        new object[]
                        {
                            new
                            {
                                IsOwned = false,
                                Type = 3,
                                IsWearing = true,
                                InventoryId = 0,
                                ItemId = lipsId,
                                Colors = lipsColor,

                            }
                        }
                });
            if (lips[0]["InventoryId"] == 0)
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Lips bought![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void buyEyes(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Buy Eyes").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            int eyeId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] Enter EyeId: ")
                .PromptStyle("#71d5fb"));
            string eyeColor = AnsiConsole.Prompt(
                new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter Color: ")
                    .PromptStyle("#71d5fb"));

            dynamic eyes = AMFConn(server,
                "MovieStarPlanet.WebService.BeautyClinic.AMFBeautyClinicService.BuyManyBeautyClinicItems",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        new object[]
                        {
                            new
                            {
                                InventoryId = 0,
                                IsOwned = false,
                                ItemId = eyeId,
                                Colors = eyeColor,
                                Type = 1,
                                IsWearing = true

                            }
                        }
                });
            if (eyes[0]["InventoryId"] == 0)
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Eye bought![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void wearRareSkin(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ RareSkin").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            string skincolor = AnsiConsole.Prompt(
                new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter Color: ")
                    .PromptStyle("#71d5fb"));

            dynamic skin = AMFConn(server,
                "MovieStarPlanet.WebService.BeautyClinic.AMFBeautyClinicService.BuyManyBeautyClinicItems",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        new object[]
                        {
                            new
                            {
                                InventoryId = 0,
                                Type = 5,
                                ItemId = -1,
                                Colors = skincolor,
                                IsWearing = true

                            }
                        }
                });
            if (skin[0]["InventoryId"] == 0)
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Skin bought![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }
        static void customStatus(string server, string name, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Status").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            string statustxt = AnsiConsole.Prompt(
                new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter Status: ")
                    .PromptStyle("#71d5fb"));
            var colorOptions = new (string Name, int Value)[]
            {
                    ("Black", 0),
                    ("Red", 13369344),
                    ("Purple", 6684774),
                    ("Light Purple", 6710988),
                    ("Pink", 13369446),
                    ("Green", 3368448),
                    ("Orange", 16737792),
                    ("Blue", 39372),
                    ("Gray", 11187123)
            };

            var selectedColor = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[[[#71d5fb]+[/]]] Select a color: ")
                    .PageSize(10)
                    .AddChoices(colorOptions.Select(choice => choice.Name))
            );

            var selectedChoice = colorOptions.First(choice => choice.Name == selectedColor);

            dynamic status = AMFConn(server,
                "MovieStarPlanet.WebService.ActorService.AMFActorServiceForWeb.SetMoodWithModerationCall",
                new object[5]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        new
                        {
                            Likes = 0,
                            TextLine = statustxt,
                            TextLineLastFiltered = (object)null,
                            ActorId = actorId,
                            WallPostId = 0,
                            TextLineBlacklisted = "",
                            WallPostLinks = (object)null,
                            FigureAnimation = "Girl Pose",
                            FaceAnimation = "neutral",
                            MouthAnimation = "none",
                            SpeechLine = false,
                            IsBrag = false,
                            TextLineWhitelisted = ""
                        },
                        name,
                        selectedChoice.Value,
                        false
                });
            if (status["FilterTextResult"]["IsMessageOk"])
            {
                if (status["FilterTextResult"]["UnrestrictedPolicy"]["HasFilteredParts"])
                {
                    AnsiConsole.Markup(
                        "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Mood Set But Censored![/] [[Click any key to return to Home]][/]");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Mood Set![/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void addSponsors(string server, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Add Sponsors").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");

            List<int> anchorCharacterList = new List<int>
                { 273, 276, 277, 341, 418, 419, 420, 421, 83417, 83423, 83427, 83424 };

            foreach (int anchorId in anchorCharacterList)
            {
                dynamic anchor = AMFConn(server,
                    "MovieStarPlanet.WebService.AnchorCharacter.AMFAnchorCharacterService.RequestFriendship",
                    new object[2]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        anchorId
                    });
                if (anchor["Code"] != 0)
                {
                    AnsiConsole.Markup(
                        $"\n[#fa1414]FAILED[/] > [#f7b136][underline]{anchor["Description"] ?? "Unknown"}[/][/]"
                    );
                }
                else
                {
                    AnsiConsole.Markup(
                        $"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Anchor {anchorId} has been added!![/][/]"
                    );
                }
            }
            AnsiConsole.Markup(
                "\n[#71d5fb][/] > [#f7b136][underline]Click any key to return to Home[/][/]"
            );
            Console.ReadKey();
            Console.Clear();
        }

        static void blockDefaults(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Block Defaults").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");

            List<int> mspDefaults = new List<int>
                    { 3, 4, 414 };

            foreach (int defaultId in mspDefaults)
            {
                dynamic mspdefaults = AMFConn(server,
                    "MovieStarPlanet.WebService.ActorService.AMFActorServiceForWeb.BlockActor",
                    new object[3]
                    {
                            new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                            actorId,
                            defaultId
                    });
                Console.WriteLine($"Blocked: {defaultId}");

            }
        }

        static void recycleitems(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Status").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");

            Console.Write("Enter Item relid: ");
            int relId = int.Parse(Console.ReadLine());


            dynamic recycleitems = AMFConn(server,
                "MovieStarPlanet.WebService.Profile.AMFProfileService.RecycleItem",
                new object[4]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        actorId,
                        relId,
                        0
                });
        }

        static void wheelspins(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Wheelspins").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");

            dailyAwardTypes(server, ticket, "starwheel", 120, actorId, 4);
            dailyAwardTypes(server, ticket, "starVipWheel", 200, actorId, 4);
            dailyAwardTypes(server, ticket, "advertWheelDwl", 240, actorId, 2);
            dailyAwardTypes(server, ticket, "advertWheelVipDwl", 400, actorId, 2);
        }

        static void dailyAwardTypes(string server, string ticket, string awardType, int awardVal,
            int actorId,
            int count)
        {
            for (int i = 0; i < count; i++)
            {
                dynamic result = AMFConn(server,
                    "MovieStarPlanet.WebService.Awarding.AMFAwardingService.claimDailyAward",
                    new object[4]
                    {
                            new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                            awardType,
                            awardVal,
                            actorId
                    });
                Console.WriteLine("Spinning Wheels...");
                Console.Clear();
            }
        }


        static void addToWishlist(string server, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ WishList").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            int clothId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] Enter ClothesId: ")
                .PromptStyle("#71d5fb"));
            string clothColor = AnsiConsole.Prompt(
                new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter Color: ")
                    .PromptStyle("#71d5fb"));

            dynamic wishlist = AMFConn(server,
                "MovieStarPlanet.WebService.Gifts.AMFGiftsService+Version2.AddItemToWishlist",
                new object[3]
                {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        new object[]
                        {
                            clothId
                        },
                        new object[]
                        {
                            clothColor
                        }
                });
            if (wishlist != 0)
            {
                AnsiConsole.Markup(
                    "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
            }
            else
            {
                AnsiConsole.Markup(
                    "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]An cloth have been added in your wishlist[/] [[Click any key to return to Home]][/]");
            }
        }


        static void lisaHack(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Lisa Hack").LeftJustified()
                .RoundedBorder());
            Console.Write("\n");
            AnsiConsole.Markup(
                "[slowblink][[[#c70000]?![/]]] Use it at your own risk, we are not responsible for your misdeeds.[/]\n");

            bool success = false;

            for (int i = 0; i < 100; i++)
            {
                dynamic lisaFame = AMFConn(server,
                    "MovieStarPlanet.WebService.AMFAwardService.claimDailyAward",
                    new object[4]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        "twoPlayerFame",
                        50,
                        actorId
                    });
                Console.WriteLine("Generated 50 fame");

                dynamic lisaMoney = AMFConn(server,
                    "MovieStarPlanet.WebService.AMFAwardService.claimDailyAward",
                    new object[4]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        "twoPlayerMoney",
                        50,
                        actorId
                    });
                Console.WriteLine("Generated 50 starcoins");

                if (i == 99)
                {
                    success = true;
                }
            }

            if (success)
            {
                AnsiConsole.Markup("\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Stars are out your account has been levelled and has starcoins : )[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void query(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Msp Query").LeftJustified()
                .RoundedBorder());
            var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter username: ")
                .PromptStyle("#71d5fb"));


            dynamic queryUsername = AMFConn(server,
                "MovieStarPlanet.WebService.UserSession.AMFUserSessionService.GetActorIdFromName",
                new object[1] { username });

            if (queryUsername == -1)
            {
                Console.WriteLine(
                    "\n\x1b[91mFAILED\u001b[39m > \x1b[93mThe account doesn't exist or has been deleted [Click any key to return to login]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                double queryactorId = queryUsername;


                dynamic queryprofile = AMFConn(server,
                    "MovieStarPlanet.WebService.Profile.AMFProfileService.LoadProfileSummary",
                    new object[3]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        queryactorId,
                        actorId
                    });

                DateTime qreatedate = queryprofile["Created"];

                dynamic queryprofileinfo = AMFConn(server,
                    "MovieStarPlanet.WebService.AMFActorService.BulkLoadActors",
                    new object[2]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        new object[]
                        {
                            queryactorId
                        }
                    });

                string nebulaProfileId = queryprofileinfo[0]["NebulaProfileId"];
                double qactorId = queryprofileinfo[0]["ActorId"];
                string qusername = queryprofileinfo[0]["Name"];
                int level = queryprofileinfo[0]["Level"];
                double fame = queryprofileinfo[0]["Fame"];
                int starcoins = queryprofileinfo[0]["Money"];
                int diamonds = queryprofileinfo[0]["Diamonds"];
                string skinColor = queryprofileinfo[0]["SkinColor"];
                int eyeId = queryprofileinfo[0]["EyeId"];
                string eyeColors = queryprofileinfo[0]["EyeColors"];
                int noseId = queryprofileinfo[0]["NoseId"];
                int mouthId = queryprofileinfo[0]["MouthId"];
                string mouthColors = queryprofileinfo[0]["MouthColors"];
                DateTime membershiptimeoutdate = queryprofileinfo[0]["MembershipTimeoutDate"];
                DateTime LastLogin = queryprofileinfo[0]["LastLogin"];

                AnsiConsole.MarkupLine("[bold white]Profile Information[/]");
                AnsiConsole.MarkupLine($"[bold blue]NebulaProfileId:[/] {nebulaProfileId}");
                AnsiConsole.MarkupLine($"[bold blue]ActorId:[/] {qactorId}");
                AnsiConsole.MarkupLine($"[bold blue]Name:[/] {qusername}");
                AnsiConsole.MarkupLine($"[bold blue]Level:[/] {level}");
                AnsiConsole.MarkupLine($"[bold blue]Fame:[/] {fame}");
                AnsiConsole.MarkupLine($"[bold blue]Money:[/] {starcoins}");
                AnsiConsole.MarkupLine($"[bold blue]Diamonds:[/] {diamonds}");
                AnsiConsole.MarkupLine($"[bold blue]SkinColor:[/] {skinColor}");
                AnsiConsole.MarkupLine($"[bold blue]EyeId:[/] {eyeId}");
                AnsiConsole.MarkupLine($"[bold blue]EyeColors:[/] {eyeColors}");
                AnsiConsole.MarkupLine($"[bold blue]NoseId:[/] {noseId}");
                AnsiConsole.MarkupLine($"[bold blue]MouthId:[/] {mouthId}");
                AnsiConsole.MarkupLine($"[bold blue]MouthColors:[/] {mouthColors}");
                AnsiConsole.MarkupLine($"[bold blue]Created:[/] {qreatedate}");
                AnsiConsole.MarkupLine($"[bold blue]MembershipTimeoutDate:[/] {membershiptimeoutdate}");
                AnsiConsole.MarkupLine($"[bold blue]LastLogin:[/] {LastLogin}");

                AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Queried {qusername} :)[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void automatedPixeller(string server, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Automated Pixeller").LeftJustified()
                .RoundedBorder());
            AnsiConsole.MarkupLine($"[#71d5fb]Login with second account : )[/]");
            var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] username: ")
                .PromptStyle("#71d5fb"));
            AnsiConsole.MarkupLine($"[#71d5fb]Coming next update : )[/]");

            Console.ReadKey();
            Console.Clear();
        }

        static void usernameChecker()
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Username Checker").LeftJustified()
                .RoundedBorder());
            var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] username: ")
                .PromptStyle("#71d5fb"));
            var loc1 = new string[][]
            {
                new string[] { "US", "CA", "AU", "NZ" },
                new string[] { "GB", "DE", "FR", "TR", "SE", "DK", "FI", "PL", "IE", "ES", "NL", "NO" }
            };

            foreach (var loc2 in loc1)
            {
                foreach (var server in loc2)
                {
                    var usernameChecker = AMFConn(server,
                        "MovieStarPlanet.WebService.AMFActorService.IsActorNameUsed",
                        new object[] { username });

                    bool loc3 = Convert.ToBoolean(usernameChecker);

                    if (loc3)
                    {
                        AnsiConsole.MarkupLine(
                            $"[#FF0000]{server} | {username} | Not available[/]");
                    }
                    else

                    {
                        AnsiConsole.MarkupLine(
                            $"[#00FF00]{server} | {username} | available[/]");
                    }
                }
            }
            AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]checked all servers for username :)[/] [[Click any key to return to Home]][/]");
            Console.ReadKey();
            Console.Clear();
        }

        static void clothesExtractor(string server, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Clothes Extractor").LeftJustified()
                .RoundedBorder());
            var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] username: ")
                .PromptStyle("#71d5fb"));

            dynamic loc1 = AMFConn(server,
                "MovieStarPlanet.WebService.UserSession.AMFUserSessionService.GetActorIdFromName",
                new object[1] { username });

            if (loc1 == -1)
            {
                Console.WriteLine(
                    "\n\x1b[91mFAILED\u001b[39m > \x1b[93mThe account doesn't exist or has been deleted [Click any key to return to login]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                double ceactorId = loc1;

                dynamic loc2 = AMFConn(server,
                    "MovieStarPlanet.WebService.ActorClothes.AMFActorClothes.GetActorClothesRelMinimals",
                    new object[2]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        ceactorId,
                    });

                foreach (var loc3 in loc2)
                {
                    int ActorClothesRelId = Convert.ToInt32(loc3["ActorClothesRelId"]);

                    dynamic loc4 = AMFConn(server,
                        "MovieStarPlanet.WebService.MovieStar.AMFMovieStarService.GetActorClothesRel",
                        new object[1]
                        { ActorClothesRelId });

                    string clothName = loc4["Cloth"]["Name"] ?? "Unknown";
                    int clothId = loc4["ClothesId"];
                    string color = loc4["Color"].ToString();
                    string shopId = loc4["Cloth"]["ShopId"].ToString();
                    int isVip = loc4["Cloth"]["Vip"];
                    int isDiamondItem = loc4["Cloth"]["DiamondsPrice"];

                    string isDiamond = isDiamondItem != 0 ? "Yes" : "No";
                    string IsVip = isVip != 0 ? "Yes" : "No";
                    string isRare = shopId != "-100" ? "Yes" : "No";

                    AnsiConsole.MarkupLine($"[#71d5fb]ActorClothesRelId:[/] {ActorClothesRelId}");
                    AnsiConsole.MarkupLine($"[#71d5fb]ClothesName:[/] {clothName}");
                    AnsiConsole.MarkupLine($"[#71d5fb]ClothesId:[/] {clothId}");
                    if (!string.IsNullOrEmpty(color))
                    {
                        AnsiConsole.MarkupLine($"[#71d5fb]Colors:[/] {color}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[#71d5fb]Colors:[/] None");
                    }

                    AnsiConsole.MarkupLine($"[#71d5fb]IsRareItem:[/] {isRare}");
                    AnsiConsole.MarkupLine($"[#71d5fb]IsVipItem:[/] {IsVip}");
                    AnsiConsole.MarkupLine($"[#71d5fb]IsDiamondItem:[/] {isDiamond}");
                    AnsiConsole.MarkupLine("");

                }
            }
            AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]checked all {username} clothes :)[/] [[Click any key to return to Home]][/]");
            Console.ReadKey();
            Console.Clear();
        }

        static void usernameToActorid(string server)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Username To ActorId").LeftJustified()
                .RoundedBorder());
            var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] username: ")
                .PromptStyle("#71d5fb"));

            dynamic loc1 = AMFConn(server,
                "MovieStarPlanet.WebService.UserSession.AMFUserSessionService.GetActorIdFromName",
                new object[1] { username });

            if (loc1 == -1)
            {
                Console.WriteLine(
                    "\n\x1b[91mFAILED\u001b[39m > \x1b[93mThe account doesn't exist or has been deleted [Click any key to return to login]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                double actorId = loc1;

                AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]ActorId: {actorId} | Username: {username} :)[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void actorIdToUsername(string server)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ ActorId to Username").LeftJustified()
                .RoundedBorder());
            var ActorId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] actorid: ")
                .PromptStyle("#71d5fb"));

            dynamic loc5 = AMFConn(server,
                "MovieStarPlanet.WebService.UserSession.AMFUserSessionService.GetActorNameFromId",
                new object[1]
                    { ActorId });

            string username = loc5;
            AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Username: {username} | ActorId: {ActorId}  :)[/] [[Click any key to return to Home]][/]");
            Console.ReadKey();
            Console.Clear();
        }

        static void itemTracker(string server)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Item Tracker").LeftJustified()
                .RoundedBorder());
            var ActorClothesRelId = AnsiConsole.Prompt(new TextPrompt<int>("[[[#71d5fb]+[/]]] ActorClothesRelId: ")
                .PromptStyle("#71d5fb"));

            dynamic loc4 = AMFConn(server,
                        "MovieStarPlanet.WebService.MovieStar.AMFMovieStarService.GetActorClothesRel",
                        new object[1]
                        { ActorClothesRelId });

            int actorId = loc4["ActorId"];

            dynamic loc5 = AMFConn(server,
                "MovieStarPlanet.WebService.UserSession.AMFUserSessionService.GetActorNameFromId",
                new object[1]
                    { actorId });

            string username = loc5;

            string clothName = loc4["Cloth"]["Name"] ?? "Unknown";
            int clothId = loc4["ClothesId"];
            string color = loc4["Color"].ToString();
            string shopId = loc4["Cloth"]["ShopId"].ToString();
            int isVip = loc4["Cloth"]["Vip"];
            int isDiamondItem = loc4["Cloth"]["DiamondsPrice"];

            string isDiamond = isDiamondItem != 0 ? "Yes" : "No";
            string IsVip = isVip != 0 ? "Yes" : "No";
            string isRare = shopId != "-100" ? "Yes" : "No";

            AnsiConsole.MarkupLine($"[#71d5fb]ActorClothesRelId:[/] {ActorClothesRelId}");
            AnsiConsole.MarkupLine($"[#71d5fb]ClothesName:[/] {clothName}");
            AnsiConsole.MarkupLine($"[#71d5fb]ClothesId:[/] {clothId}");
            if (!string.IsNullOrEmpty(color))
            {
                AnsiConsole.MarkupLine($"[#71d5fb]Colors:[/] {color}");
            }
            else
            {
                AnsiConsole.MarkupLine("[#71d5fb]Colors:[/] None");
            }

            AnsiConsole.MarkupLine($"[#71d5fb]IsRareItem:[/] {isRare}");
            AnsiConsole.MarkupLine($"[#71d5fb]IsVipItem:[/] {IsVip}");
            AnsiConsole.MarkupLine($"[#71d5fb]IsDiamondItem:[/] {isDiamond}");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Item tracked to {username}  :)[/] [[Click any key to return to Home]][/]");
            Console.ReadKey();
            Console.Clear();

        }

        static void roomChanger(string server, int actorId, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ RoomChanger").LeftJustified().RoundedBorder());
            Console.Write("\n");
            AnsiConsole.Markup("[slowblink][[[#c70000]?![/]]] Use it at your own risk, we are not responsible for your misdeeds.[/]\n");
            string urlImage = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter image url: ")
                                          .PromptStyle("#71d5fb"));
            System.Net.WebClient webClient = new System.Net.WebClient();
            byte[] array = webClient.DownloadData(urlImage);

            dynamic room = AMFConn(server,
                "MovieStarPlanet.WebService.Snapshots.AMFGenericSnapshotService.CreateSnapshot",
                new object[5]
                {
                    new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                    actorId,
                    "room",
                    array,
                    "jpg"
                });

            dynamic roomProfile = AMFConn(server,
                "MovieStarPlanet.WebService.Snapshots.AMFGenericSnapshotService.CreateSnapshot",
                new object[5]
                {
                    new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                    actorId,
                    "roomProfile",
                    array,
                    "jpg"
                });
            dynamic roomMedium = AMFConn(server,
                "MovieStarPlanet.WebService.Snapshots.AMFGenericSnapshotService.CreateSnapshot",
                new object[5]
                {
                    new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                    actorId,
                    "roomMedium",
                    array,
                    "jpg"
                });
            if (room && roomProfile && roomMedium)
            {
                AnsiConsole.Markup("\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Room changed[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                AnsiConsole.Markup("\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Click any key to return to Home]][/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void animationsExtractor(string server, string ticket)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Animation Extractor").LeftJustified()
                .RoundedBorder());
            var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] username: ")
                .PromptStyle("#71d5fb"));

            dynamic loc1 = AMFConn(server,
                "MovieStarPlanet.WebService.UserSession.AMFUserSessionService.GetActorIdFromName",
                new object[1] { username });

            if (loc1 == -1)
            {
                Console.WriteLine(
                    "\n\x1b[91mFAILED\u001b[39m > \x1b[93mThe account doesn't exist or has been deleted [Click any key to return to login]");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                double ceactorId = loc1;

                dynamic loc2 = AMFConn(server,
                    "MovieStarPlanet.WebService.Media.AMFMediaService.GetMyAnimations",
                    new object[2]
                    {
                        new TicketHeader { anyAttribute = null, Ticket = actor(ticket) },
                        ceactorId,
                    });

                foreach (var loc3 in loc2)
                {
                    string animationName = loc3["Animation"]["Name"] ?? "Unknown";
                    int animationId = loc3["Animation"]["AnimationId"] ?? -1;
                    int actorAnimationRelid = loc3["ActorAnimationRelId"] ?? -1;

                    AnsiConsole.MarkupLine($"[#71d5fb]Name:[/] {animationName}");
                    AnsiConsole.MarkupLine($"[#71d5fb]AnimationId:[/] {animationId}");
                    AnsiConsole.MarkupLine($"[#71d5fb]ActorAnimationRelId:[/] {actorAnimationRelid}");
                    AnsiConsole.MarkupLine("");
                }
            }
            AnsiConsole.MarkupLine($"\n[#06c70c]SUCCESS[/] > [#f7b136][underline]checked all {username} animations :)[/] [[Click any key to return to Home]][/]");
            Console.ReadKey();
            Console.Clear();
        }

        static async Task MSP2_Login()
        {
            Console.Clear();
            bool loggedIn2 = false;

            while (!loggedIn2)
            {
                AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Login MSP2").LeftJustified());
                Console.Write("\n");
                var username = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter username: ")
                    .PromptStyle("#71d5fb"));

                var password = AnsiConsole.Prompt(new TextPrompt<string>("[[[#71d5fb]+[/]]] Enter password: ")
                    .PromptStyle("#71d5fb")
                    .Secret());

                var choices = Enum.GetValues(typeof(WebServer))
                    .Cast<WebServer>()
                    .Select(ws => (ws.loc3().Item1, ws.loc3().Item2))
                    .ToArray();

                var selectedCountry = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[[[#71d5fb]+[/]]] Select a server: ")
                        .PageSize(15)
                        .MoreChoicesText("[grey](Move up and down to reveal more servers)[/]")
                        .AddChoices(choices.Select(choice => choice.Item1))
                );

                var server = choices.First(choice => choice.Item1 == selectedCountry).Item2;
                var region = new[] { "US", "CA", "AU", "NZ" }.Contains(server) ? "us" : "eu";

                string accessToken = null;
                string profileId = null;

                AnsiConsole.Status()
                    .SpinnerStyle(Spectre.Console.Style.Parse("#71d5fb"))
                    .Start("Login...", ctx =>
                    {
                        ctx.Refresh();
                        ctx.Spinner(Spinner.Known.Circle);

                        var tep = $"https://{region}-secure.mspapis.com/loginidentity/connect/token";

                        using (var msptclient = new WebClient())
                        {
                            var val = new NameValueCollection
                            {
                                ["client_id"] = "unity.client",
                                ["client_secret"] = "secret",
                                ["grant_type"] = "password",
                                ["scope"] = "openid nebula offline_access",
                                ["username"] = $"{server}|{username}",
                                ["password"] = password,
                                ["acr_values"] = "gameId:j68d"
                            };

                            var resp = msptclient.UploadValues(tep, val);
                            var resp1 = Encoding.Default.GetString(resp);
                            dynamic resp2 = JsonConvert.DeserializeObject(resp1);


                            var accessToken_first = resp2["access_token"].ToString();
                            var refreshToken = resp2["refresh_token"].ToString();

                            var th = new JwtSecurityTokenHandler();
                            var jtoken = th.ReadJwtToken(accessToken_first);
                            var loginId = jtoken.Payload["loginId"].ToString();

                            string pid =
                                $"https://{region}.mspapis.com/profileidentity/v1/logins/{loginId}/profiles?&pageSize=100&page=1&filter=region:{server}";
                            msptclient.Headers.Add(HttpRequestHeader.Authorization,
                                "Bearer " + accessToken_first);
                            string resp3 = msptclient.DownloadString(pid);

                            profileId = JArray.Parse(resp3)[0]["id"].ToString();

                            var val2 = new NameValueCollection
                            {
                                ["grant_type"] = "refresh_token",
                                ["refresh_token"] = refreshToken,
                                ["acr_values"] = $"gameId:j68d profileId:{profileId}"
                            };

                            msptclient.Headers.Remove(HttpRequestHeader.Authorization);
                            msptclient.Headers.Add(HttpRequestHeader.Authorization,
                                "Basic dW5pdHkuY2xpZW50OnNlY3JldA==");
                            var resp4 = msptclient.UploadValues(tep, val2);

                            var resp5 = Encoding.Default.GetString(resp4);
                            dynamic resp6 = JsonConvert.DeserializeObject(resp5);

                            accessToken = resp6["access_token"].ToString();

                            Console.Clear();
                        }
                    });
                while (true)
                {
                    loggedIn2 = true;
                    Console.Clear();
                    AnsiConsole.Write(
                        new Rule("[#71d5fb]MSPTOOL[/] ・ Home").LeftJustified().RoundedBorder());
                    Console.Write("\n");
                    AnsiConsole.Markup("[#71d5fb]1[/]  > Mood Changer\n");
                    AnsiConsole.Markup("[#71d5fb]2[/]  > Gender Changer\n");
                    AnsiConsole.Markup("[#71d5fb]3[/]  > Delete Room\n");
                    AnsiConsole.Markup("[#71d5fb]4[/]  > Logout\n\n");
                    AnsiConsole.Write(
                        new Rule(
                                "[slowblink][#71d5fb]lcfi & 6c0[/][/]")
                            .RightJustified().RoundedBorder());
                    var options = AnsiConsole.Prompt(
                        new TextPrompt<string>("\n[[[#71d5fb]+[/]]] Pick an option: ")
                            .PromptStyle("#71d5fb"));

                    switch (options)
                    {
                        case "1":
                            moodChanger(region, accessToken, profileId);
                            Thread.Sleep(2000);
                            break;
                        case "2":
                            genderChanger(region, accessToken, profileId);
                            Thread.Sleep(2000);
                            break;
                        case "3":
                            deleteRoom(region, accessToken, profileId);
                            Thread.Sleep(2000);
                            break;
                        case "4":
                            loggedIn2 = false;
                            break;
                        default:
                            Console.WriteLine(
                                "\n\u001b[91mERROR\u001b[39m > \u001b[93mChoose an option which exists!");
                            System.Threading.Thread.Sleep(2000);
                            Console.Clear();
                            break;
                    }

                    if (!loggedIn2)
                        break;
                };
            }
        }

        static async Task moodChanger(string region, string accessToken, string profileId)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Change Mood").LeftJustified().RoundedBorder());

            var moodOptions = new (string Name, string Value)[]
            {
                ("Bunny", "bunny_hold"),
                ("Ice Skating", "noshoes_skating"),
                ("Swimming", "swim_new"),
                ("Spider Crawl", "2023_spidercrawl_lsz"),
                ("Bubblegum", "bad_2022_teenwalk_dg"),
                ("Like a Frog", "very_2022_froglike_lsz"),
                ("Cool Slide", "cool_slide"),
                ("Like Bambi", "bambislide"),
                ("Freezing", "xmas_2022_freezing_lsz"),
            };

            var selectedMood = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[[[#71d5fb]+[/]]] Select a mood: ")
                    .PageSize(10)
                    .AddChoices(moodOptions.Select(choice => choice.Name))
            );

            var selectedChoice = moodOptions.First(choice => choice.Name == selectedMood);

            using (HttpClient mt2client = new HttpClient())
            {
                mt2client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                mt2client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");

                string moodApi =
                    $"https://{region}.mspapis.com/profileattributes/v1/profiles/{profileId}/games/j68d/attributes";

                HttpResponseMessage resp = await mt2client.GetAsync(moodApi);

                string resp2 = await resp.Content.ReadAsStringAsync();
                JObject moodData = JObject.Parse(resp2);

                moodData["additionalData"]["Mood"] = selectedChoice.Value;

                string loc1 = moodData.ToString();
                HttpContent loc2 = new StringContent(loc1);
                loc2.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage resp3 = await mt2client.PutAsync(moodApi, loc2);
                if (resp3.IsSuccessStatusCode)
                {
                    AnsiConsole.Markup(
                        "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Mood changed[/] [[Auto redirect in 2 seconds]][/]");
                }
                else
                {
                    AnsiConsole.Markup(
                        "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Auto redirect in 2 seconds]][/]");

                }
            }
        }

        static async Task genderChanger(string region, string accessToken, string profileId)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Change Gender").LeftJustified().RoundedBorder());

            var genderOptions = new (string Name, string Value)[]
            {
                ("Girl", "Girl"),
                ("Boy", "Boy"),
            };

            var selectedGender = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[[[#71d5fb]+[/]]] Select a mood: ")
                    .PageSize(10)
                    .AddChoices(genderOptions.Select(choice => choice.Name))
            );

            var selectedChoice = genderOptions.First(choice => choice.Name == selectedGender);

            using (HttpClient mt2client = new HttpClient())
            {
                mt2client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                mt2client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");

                string genderApi =
                    $"https://{region}.mspapis.com/profileattributes/v1/profiles/{profileId}/games/j68d/attributes";

                HttpResponseMessage resp = await mt2client.GetAsync(genderApi);

                string resp2 = await resp.Content.ReadAsStringAsync();
                JObject genderData = JObject.Parse(resp2);

                genderData["additionalData"]["Gender"] = selectedChoice.Value;

                string loc1 = genderData.ToString();
                HttpContent loc2 = new StringContent(loc1);
                loc2.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage resp3 = await mt2client.PutAsync(genderApi, loc2);
                if (resp3.IsSuccessStatusCode)
                {
                    AnsiConsole.Markup(
                        "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Gender changed[/] [[Auto redirect in 2 seconds]][/]");
                }
                else
                {
                    AnsiConsole.Markup(
                        "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Auto redirect in 2 seconds]][/]");
                }
            }
        }

        static async Task deleteRoom(string region, string accessToken, string profileId)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[#71d5fb]MSPTOOL[/] ・ Home ・ Delete Room").LeftJustified().RoundedBorder());

            using (HttpClient mt2client = new HttpClient())
            {
                mt2client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                mt2client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");

                string roomApi =
                    $"https://{region}.mspapis.com/profileattributes/v1/profiles/{profileId}/games/j68d/attributes";

                HttpResponseMessage resp = await mt2client.GetAsync(roomApi);


                string resp1 = await resp.Content.ReadAsStringAsync();
                JObject roomData = JObject.Parse(resp1);

                if (roomData["additionalData"]?["DefaultMyHome"] != null)
                {
                    roomData["additionalData"]["DefaultMyHome"].Parent.Remove();
                }

                string loc1 = roomData.ToString();
                HttpContent loc2 = new StringContent(loc1);
                loc2.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage resp3 = await mt2client.PutAsync(roomApi, loc2);

                if (resp3.IsSuccessStatusCode)
                {
                    AnsiConsole.Markup(
                        "\n[#06c70c]SUCCESS[/] > [#f7b136][underline]Room deleted[/] [[Auto redirect in 2 seconds]][/]");
                }
                else
                {
                    AnsiConsole.Markup(
                        "\n[#fa1414]FAILED[/] > [#f7b136][underline]Unknown[/] [[Auto redirect in 2 seconds]][/]");
                }

            }
        }


        static bool isCurrentVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string latestVersion = client.GetStringAsync(checkVersion).Result;
                    return currentVersion.Trim() == latestVersion.Trim();
                }
                catch (Exception ex)
                {
                    return true;
                }

            }
        }
    }
}