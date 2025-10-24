using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using LemonUI;
using LemonUI.Menus;
using ShurikenLegal.Client.Jobs;
using Delegate = System.Delegate;
using System.Drawing;
using CitizenFX.Core.Native;
using ShurikenLegal.Shared;
using CitizenFX.Core.UI;

namespace ShurikenLegal.Client
{
    public class ClientMain : BaseScript
    {
        public ObjectPool Pool = new ObjectPool();
        public Transport transport;
        public EMS ems;
        public Bennys bennys;
        public Police police;
        public BurgerShot burgerShot;
        public Jobless jobless;
        public CoffeeShop coffeeShop;
        public Unicorn unicorn;
        public Casino casino;
        public Taquila taquila;
        public Bahama bahama;
        public Immobilier immobilier;
        public ConcessAuto concess;
        public Job Metier;

        public int MyJob;
        public Building ThisBuilding;
        public List<InventoryItem> ChestInfos;
        public bool IsRobbing = false;

        public ClientMain()
        {
            bennys = Metier as Bennys;
            Debug.WriteLine("Hi from ShurikenLegal.Client!");
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientStart);
            EventHandlers.Add("playerSpawned", new Action<Vector3>(OnPlayerSpawned));
            EventHandlers["legal_client:allBuildings"] += new Action<string>(ReceiveAllBuildings);
            EventHandlers["legal_client:colors"] += new Action<string>(OnColor);
            EventHandlers["legal_client:wheels"] += new Action<string>(OnWheels);
            EventHandlers["legal_client:getJob"] += new Action<string>(GetJob);
            EventHandlers["legal_client:getPlayerData"] += new Action<string>(GetPlayerData);
            EventHandlers["legal_client:getCompanyData"] += new Action<string>(GetCompanyData);
            EventHandlers["legal_client:setDoorState"] += new Action<string, int>(SetDoorState);
        }

        public PlayerInstance PlayerInst = new PlayerInstance
        {
            Inventory = new List<InventoryItem>(),
            Money = 0
        };

        public List<Building> Buildings = new List<Building>();
        public Appartment Appartment1 = new Appartment(new Vector3(-30.9f, -595.2f, 79.5f), new Vector3(-38.1f, -589.3f, 78), new Vector3(-12.4f, -569.9f, 78.6f));
        public Appartment Appartment2 = new Appartment(new Vector3(-468, -689.5f, 52.6f), new Vector3(-468.8f, -710.2f, 46.4f), new Vector3(-468.7f, -697.8f, 52.6f));
        public Appartment Appartment3 = new Appartment(new Vector3(-574.5f, -715.7f, 112.5f), new Vector3(-600.7f, -709.5f, 120.8f), new Vector3(-580, -727.1f, 120.8f));

        public void BuildingSystem()
        {
            var playerCoords = Game.Player.Character.Position;

            foreach (var building in Buildings)
            {
                if (playerCoords.DistanceToSquared(building.Door) < 10)
                {
                    World.DrawMarker(MarkerType.HorizontalCircleSkinny, building.Door, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(224, 50, 50), bobUpAndDown: false, rotateY: true);
                    
                    if (playerCoords.DistanceToSquared(building.Door) < 3)
                    {
                        SendTextUI("Appuyer sur ~r~E~s~ pour ouvrir");
                        if (IsControlJustPressed(0, 38))
                        {
                            var menu = new NativeMenu("Immeuble", $"{building.Address}");
                            Pool.Add(menu);
                            menu.Visible = true;
                            menu.UseMouse = false;
                            foreach (var appartment in building.Appartments)
                            {
                                if (appartment.Resident == $"{PlayerInst.Firstname} {PlayerInst.Lastname}")
                                {
                                    var menuOptions = new List<string> { "Rentrer", "Ouvrir/Fermer l'appart" };

                                    var appart = new NativeListItem<string>($"{appartment.Resident} (Vous)", menuOptions.ToArray());
                                    menu.Add(appart);

                                    appart.Activated += (sender, e) =>
                                    {
                                        if (appart.SelectedItem == "Rentrer")
                                        {
                                            if (appartment.IsLocked != true)
                                            {
                                                SendNotif($"Vous êtes rentré dans l'appartement de {appartment.Resident}");
                                                TriggerServerEvent("legal_server:enterAppart", building.Id, appartment.Id);
                                                menu.Visible = false;
                                            }
                                            else
                                            {
                                                SendNotif("~r~L'appartement est fermé");
                                            }
                                        }
                                        else if (appart.SelectedItem == "Ouvrir/Fermer l'appart")
                                        {
                                            TriggerServerEvent("legal_server:stateDoor", building.Id, appartment.Id);
                                            menu.Visible = false;
                                        }
                                    };
                                }
                                else
                                {
                                    var playerInventory = PlayerInst.Inventory;
                                    var pickingTool = playerInventory.FirstOrDefault(i => i.Item == "Outil de crochetage");

                                    var isLocked = appartment.IsLocked ? "~r~Fermé" : "~g~Ouvert";

                                    var menuOptions = new List<string> { isLocked };

                                    if (pickingTool != null)
                                    {
                                        menuOptions.Add("Cambrioler");
                                    }

                                    var appart = new NativeListItem<string>($"{appartment.Resident}", menuOptions.ToArray());
                                    menu.Add(appart);

                                    appart.Activated += async (sender, e) =>
                                    {
                                        if (appart.SelectedItem == $"{isLocked}")
                                        {
                                            if (appartment.IsLocked)
                                            {
                                                SendNotif("~r~L'appartement est fermé");
                                            }
                                            else
                                            {
                                                SendNotif($"Vous êtes rentré dans l'appartement de {appartment.Resident}");
                                                menu.Visible = false;
                                                TriggerServerEvent("legal_server:enterAppart", building.Id, appartment.Id);
                                            }
                                        }
                                        else if (appart.SelectedItem == "Cambrioler")
                                        {
                                            SendNotif("~r~Cambriolage en cours, veuillez patientez...");
                                            PlayAnimation("anim@heists@fleeca_bank@drilling", "drill_straight_start", 8, (AnimationFlags)50);
                                            PlaySoundFrontend(-1, "Drill_Pin_Break", "DLC_HEIST_FLEECA_SOUNDSET", true);
                                            await AddPropToPlayer("hei_prop_heist_drill", 28422, 0, 0, 0, 0, 0, 0, 5000);
                                            StopAnimation("anim@heists@fleeca_bank@drilling", "drill_straight_start");
                                            TriggerServerEvent("legal_server:robAppart", building.Id, appartment.Id);
                                        }
                                    };
                                }
                            }
                        }
                    }
                }

                foreach (var appartment in building.Appartments)
                {
                    NewClothMenu(appartment);
                    Coffre(building, appartment);

                    if (playerCoords.DistanceToSquared(appartment.Interior) < 10)
                    {
                        World.DrawMarker(MarkerType.HorizontalCircleSkinny, appartment.Interior, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(224, 50, 50), bobUpAndDown: false, rotateY: true);

                        if (playerCoords.DistanceToSquared(appartment.Interior) < 3)
                        {
                            if (appartment.Resident == $"{PlayerInst.Firstname} {PlayerInst.Lastname}")
                            {
                                SendTextUI("Appuyer sur ~r~E~s~ pour sortir\n" +
                                    "Appuyer sur ~r~R~s~ pour ouvrir le menu");
                                if (IsControlJustPressed(0, 80))
                                {
                                    var menu = new NativeMenu("Appartement", $"Chez {appartment.Resident}");
                                    Pool.Add(menu);
                                    menu.UseMouse = false;
                                    menu.Visible = true;
                                    var objList = new List<int>();
                                    var openDoor = new NativeItem("Ouvrir/fermer l'appartement");
                                    menu.Add(openDoor);

                                    var decorationsMenu = new NativeMenu("Décorations", "Décorations");
                                    Pool.Add(decorationsMenu);
                                    decorationsMenu.UseMouse = false;
                                    menu.AddSubMenu(decorationsMenu);

                                    int[] decorationNumbers = Enumerable.Range(0, appartment.Decorations.Count).ToArray();
                                    var decorationsPosList = new NativeListItem<int>("Emplacements",
                                        "Choisissez un emplacement pour une déco", decorationNumbers);
                                    decorationsPosList.Activated += (sender, e) =>
                                    {
                                        var decoPosition = appartment.Decorations[decorationsPosList.SelectedItem].Position;
                                        SetEntityCoords(GetPlayerPed(-1), decoPosition.X, decoPosition.Y, decoPosition.Z, false, false, false, false);
                                    };
                                    decorationsMenu.Add(decorationsPosList);

                                    var decorationsList = new NativeListItem<string>("Décorations", "Choisis la déco",
                                        "prop_weed_01", "apa_mp_h_acc_dec_head_01");
                                    decorationsMenu.Add(decorationsList);
                                    decorationsList.Activated += (sender, e) =>
                                    {
                                        if (objList.Count > 0)
                                        {
                                            foreach (var item in objList)
                                            {
                                                if (Function.Call<bool>(Hash.DOES_ENTITY_EXIST, item))
                                                {
                                                    Function.Call(Hash.DELETE_OBJECT, item);
                                                }
                                            }
                                            objList.Clear();
                                        }
                                        var obj = CreateObject(GetHashKey(decorationsList.SelectedItem),
                                           appartment.Decorations[decorationsPosList.SelectedItem].Position.X,
                                           appartment.Decorations[decorationsPosList.SelectedItem].Position.Y,
                                           appartment.Decorations[decorationsPosList.SelectedItem].Position.Z,
                                           false, false, false);

                                        objList.Add(obj);
                                    };

                                    var submit = new NativeItem("Enregistrer");
                                    decorationsMenu.Add(submit);
                                    submit.Activated += (sender, e) =>
                                    {
                                        TriggerServerEvent("legal:addDecoration", building.Id, appartment.Id, 
                                            JsonConvert.SerializeObject(appartment.Decorations[decorationsPosList.SelectedItem].Position),
                                            decorationsList.SelectedItem);
                                    };

                                    decorationsMenu.Closed += (sender, e) =>
                                    {
                                        SetEntityCoords(GetPlayerPed(-1), appartment.Interior.X,
                                            appartment.Interior.Y, appartment.Interior.Z, false, false, false, false);
                                        if (objList.Count > 0)
                                        {
                                            foreach (var item in objList)
                                            {
                                                if (Function.Call<bool>(Hash.DOES_ENTITY_EXIST, item))
                                                {
                                                    Function.Call(Hash.DELETE_OBJECT, item);
                                                }
                                            }
                                            objList.Clear();
                                        }
                                    };

                                    openDoor.Activated += (sender, e) =>
                                    {
                                        TriggerServerEvent("legal_server:stateDoor", building.Id, appartment.Id);
                                        menu.Visible = false;
                                    };
                                }
                                if (IsControlJustPressed(0, 38))
                                {
                                    if (IsRobbing == false)
                                    {
                                        TriggerServerEvent("legal_server:exitAppart", building.Id);
                                        DeleteRobbingObjects();
                                    } else
                                    {
                                        TriggerServerEvent("legal_server:cancelRobbing", building.Id);
                                        DeleteRobbingObjects();
                                    }
                                }
                            } else
                            {
                                SendTextUI("Appuyer sur ~r~E~s~ pour sortir");
                                if (IsControlJustPressed(0, 38))
                                {
                                    TriggerServerEvent("legal_server:exitAppart", building.Id);
                                    DeleteRobbingObjects();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void NewClothMenu(Appartment appartment)
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), true);

            var distance = playerCoords.DistanceToSquared(appartment.Dress);
            if (distance < 10)
            {
                SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                if (IsControlJustPressed(0, 38))
                {
                    var menu = new NativeMenu("Magasin de vêtements", "Dress")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(menu);
                    menu.Visible = true;

                    menu.Closed += (sender, e) =>
                    {
                        BaseScript.TriggerServerEvent("core:setClothes");
                    };

                    var hat = new NativeMenu("Chapeaux, casquettes, bérets", "Chapeaux")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(hat);
                    menu.AddSubMenu(hat);

                    var hatsList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 0)).ToList();
                    NativeListItem<int> itemsHat = new NativeListItem<int>("~h~Chapeaux~s~", hatsList.ToArray());
                    hat.Add(itemsHat);

                    var hatsTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 0, itemsHat.SelectedIndex)).ToList();
                    NativeListItem<int> itemHatTexture = new NativeListItem<int>($"Style", hatsTextureList.ToArray());
                    hat.Add(itemHatTexture);

                    itemsHat.ItemChanged += (sender, e) =>
                    {
                        SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, 0, false);
                        itemHatTexture.SelectedIndex = 0;
                    };

                    itemHatTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, itemHatTexture.SelectedIndex, false);
                    };

                    var hatSubmit = new NativeItem("Acheter", "", "~g~0$");
                    hat.Add(hatSubmit);

                    hatSubmit.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Chapeau", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(0)  // Hat
                            {
                                Drawable = itemsHat.SelectedIndex,
                                Texture = itemHatTexture.SelectedIndex,
                                Palette = 0
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 0, json);
                        }
                    };

                    var top = new NativeMenu("Sous-haut, haut, bras", "Haut")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(top);
                    menu.AddSubMenu(top);

                    // UNDERSHIRT
                    var undershirtList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 8)).ToList();
                    NativeListItem<int> undershirts = new NativeListItem<int>("~h~Sous haut~s~", undershirtList.ToArray());
                    top.Add(undershirts);

                    var undershirtsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 8, undershirts.SelectedIndex)).ToList();
                    NativeListItem<int> undershirtsTexture = new NativeListItem<int>("Style", undershirtsTextureList.ToArray());
                    top.Add(undershirtsTexture);

                    undershirts.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, 0, 1);
                        undershirtsTexture.SelectedIndex = 0;
                    };

                    undershirtsTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, undershirtsTexture.SelectedIndex, 1);
                    };

                    // ARMS
                    var armsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 3)).ToList();
                    NativeListItem<int> arms = new NativeListItem<int>("~h~Bras~s~", armsList.ToArray());
                    top.Add(arms);

                    var armsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 3, arms.SelectedIndex)).ToList();
                    NativeListItem<int> armsTexture = new NativeListItem<int>("Style", armsTextureList.ToArray());
                    top.Add(armsTexture);

                    arms.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, 0, 1);
                        armsTexture.SelectedIndex = 0;
                    };

                    armsTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, armsTexture.SelectedIndex, 1);
                    };

                    // TOP
                    var topsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 11)).ToList();
                    NativeListItem<int> tops = new NativeListItem<int>("~h~Haut~s~", topsList.ToArray());
                    top.Add(tops);

                    var topsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 11, tops.SelectedIndex)).ToList();
                    NativeListItem<int> topsTexture = new NativeListItem<int>("Style", topsTextureList.ToArray());
                    top.Add(topsTexture);

                    tops.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, 0, 1);
                        topsTexture.SelectedIndex = 0;
                    };

                    topsTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, topsTexture.SelectedIndex, 1);
                    };

                    var topSubmit = new NativeItem("Acheter", "", "~g~0$");
                    top.Add(topSubmit);

                    topSubmit.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Habit", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(8)  // Undershirt
                            {
                                Drawable = undershirts.SelectedIndex,
                                Texture = undershirtsTexture.SelectedIndex,
                                Palette = 1
                            });

                            clothingSet.Components.Add(new ClothesComponent(3)  // Torso
                            {
                                Drawable = arms.SelectedIndex,
                                Texture = armsTexture.SelectedIndex,
                                Palette = 1
                            });

                            clothingSet.Components.Add(new ClothesComponent(11)  // Top
                            {
                                Drawable = tops.SelectedIndex,
                                Texture = topsTexture.SelectedIndex,
                                Palette = 1
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 0, json);
                        }
                    };



                    var legs = new NativeMenu("Bas", "Bas")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(legs);
                    menu.AddSubMenu(legs);

                    var legsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 4)).ToList();
                    NativeListItem<int> legsItem = new NativeListItem<int>("~h~Bas~s~", legsList.ToArray());
                    legs.Add(legsItem);

                    var legsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 4, legsItem.SelectedIndex)).ToList();
                    NativeListItem<int> legsTexture = new NativeListItem<int>("Style", legsTextureList.ToArray());
                    legs.Add(legsTexture);

                    legsItem.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, 0, 1);
                        legsTexture.SelectedIndex = 0;
                    };

                    legsTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, legsTexture.SelectedIndex, 1);
                    };

                    var legsSubmit = new NativeItem("Acheter", "", "~g~0$");
                    legs.Add(legsSubmit);

                    legsSubmit.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Habit", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(4)  // Legs
                            {
                                Drawable = legsItem.SelectedIndex,
                                Texture = legsTexture.SelectedIndex,
                                Palette = 1
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 0, json);
                        }
                    };

                    var shoes = new NativeMenu("Chaussures", "Chaussures")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(shoes);
                    menu.AddSubMenu(shoes);

                    var shoesList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 6)).ToList();
                    NativeListItem<int> shoesItem = new NativeListItem<int>("~h~Chaussures~s~", shoesList.ToArray());
                    shoes.Add(shoesItem);

                    var shoesTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 6, shoesItem.SelectedIndex)).ToList();
                    NativeListItem<int> shoesTexture = new NativeListItem<int>("Style", shoesTextureList.ToArray());
                    shoes.Add(shoesTexture);

                    shoesItem.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, 0, 1);
                        shoesTexture.SelectedIndex = 0;
                    };

                    shoesTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, shoesTexture.SelectedIndex, 1);
                    };

                    var shoesSubmit = new NativeItem("Acheter", "", "~g~0$");
                    shoes.Add(shoesSubmit);

                    shoesItem.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Habit", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(6)  // Shoes
                            {
                                Drawable = shoesItem.SelectedIndex,
                                Texture = shoesTexture.SelectedIndex,
                                Palette = 1
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 0, json);
                        }
                    };

                    var glasses = new NativeMenu("Lunettes", "Lunettes")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(glasses);
                    menu.AddSubMenu(glasses);

                    var glassesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 1)).ToList();
                    NativeListItem<int> itemsGlasses = new NativeListItem<int>("\"~h~Chapeaux~s~", glassesList.ToArray());
                    glasses.Add(itemsGlasses);

                    var glassesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex)).ToList();
                    NativeListItem<int> itemGlassesTexture = new NativeListItem<int>($"Style", glassesTextureList.ToArray());
                    glasses.Add(itemGlassesTexture);

                    itemsGlasses.ItemChanged += (sender, e) =>
                    {
                        SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, 0, false);
                        itemGlassesTexture.SelectedIndex = 0;
                    };

                    itemGlassesTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, itemGlassesTexture.SelectedIndex, false);
                    };

                    var glassesSubmit = new NativeItem("Acheter", "", "~g~0$");
                    glasses.Add(glassesSubmit);

                    glassesSubmit.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Lunette", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(1)  // Glasses
                            {
                                Drawable = itemsGlasses.SelectedIndex,
                                Texture = itemGlassesTexture.SelectedIndex,
                                Palette = 0
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 0, json);
                        }
                    };

                    var watches = new NativeMenu("Montres", "Montres")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(watches);
                    menu.AddSubMenu(watches);

                    var watchesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 6)).ToList();
                    NativeListItem<int> itemsWatches = new NativeListItem<int>("\"~h~Chapeaux~s~", watchesList.ToArray());
                    watches.Add(itemsWatches);

                    var watchesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex)).ToList();
                    NativeListItem<int> itemWatchesTexture = new NativeListItem<int>($"Style", watchesTextureList.ToArray());
                    watches.Add(itemWatchesTexture);

                    itemsWatches.ItemChanged += (sender, e) =>
                    {
                        SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, 0, false);
                        itemWatchesTexture.SelectedIndex = 0;
                    };

                    itemWatchesTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, itemWatchesTexture.SelectedIndex, false);
                    };

                    var watchSubmit = new NativeItem("Acheter", "", "~g~0$");
                    watches.Add(watchSubmit);

                    watchSubmit.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Montre", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(6)  // Watches
                            {
                                Drawable = itemsWatches.SelectedIndex,
                                Texture = itemWatchesTexture.SelectedIndex,
                                Palette = 0
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 1, json);
                        }
                    };

                    var masks = new NativeMenu("Masques", "Masques")
                    {
                        UseMouse = false,
                        HeldTime = 100
                    };
                    Pool.Add(masks);
                    menu.AddSubMenu(masks);

                    var masksList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 1)).ToList();
                    NativeListItem<int> masksItem = new NativeListItem<int>("~h~Masque~s~", masksList.ToArray());
                    masks.Add(masksItem);

                    var masksTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 1, masksItem.SelectedIndex)).ToList();
                    NativeListItem<int> masksTexture = new NativeListItem<int>("Style", masksTextureList.ToArray());
                    masks.Add(masksTexture);

                    masksItem.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 1, masksItem.SelectedIndex, 0, 1);
                        masksTexture.SelectedIndex = 0;
                    };

                    masksTexture.ItemChanged += (sender, e) =>
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), 1, masksItem.SelectedIndex, masksTexture.SelectedIndex, 1);
                    };

                    var masksSubmit = new NativeItem("Acheter", "", "~g~0$");
                    masks.Add(masksSubmit);

                    masksItem.Activated += async (sender, e) =>
                    {
                        if (PlayerInst.Money >= 0)
                        {
                            var textInput = await GetUserInput("Donnez un nom", "Habit", 12);

                            var clothingSet = new Shared.ClothingSet
                            {
                                Name = textInput
                            };

                            clothingSet.Components.Add(new ClothesComponent(1)  // masks
                            {
                                Drawable = masksItem.SelectedIndex,
                                Texture = masksTexture.SelectedIndex,
                                Palette = 1
                            });

                            var json = JsonConvert.SerializeObject(clothingSet);
                            BaseScript.TriggerServerEvent("core:buyTopClothes", 0, json);
                        }
                    };
                }
            }
        }

        public void Coffre(Building building, Appartment appartment)
        {   
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            float dist = appartment.Chest.DistanceToSquared(playerCoords);
            if (dist < 8)
            {
                World.DrawMarker(MarkerType.ChevronUpx1, appartment.Chest, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(224, 50, 50), bobUpAndDown: true, rotateY: true);
            }

            if (dist < 3)
            {
                SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le coffre");
                if (IsControlPressed(0, 38))
                {
                    var menu = new NativeMenu("Coffre perso", $"Coffre de {appartment.Resident}")
                    {
                        UseMouse = false
                    };
                    Pool.Add(menu);
                    menu.Visible = true;

                    var pick = new NativeMenu("Retirer", "Retirer")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    };
                    menu.AddSubMenu(pick);

                    var drop = new NativeMenu("Déposer", "Déposer")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    };
                    menu.AddSubMenu(drop);
                    Pool.Add(pick);
                    Pool.Add(drop);

                    foreach (var chest in appartment.ChestInventory)
                    {
                        if (chest.Type == "item")
                        {
                            var item = new NativeItem($"{chest.Item} ({chest.Quantity})");
                            pick.Add(item);
                            item.Activated += async (sender, e) =>
                            {
                                var textInput = await GetUserInput("Quantité", "1", 4);
                                var parsedInput = int.Parse(textInput);
                                if (parsedInput <= chest.Quantity)
                                {
                                    if (chest.Quantity > 0)
                                    {
                                        TriggerServerEvent("legal_server:getItemFromVault", building.Id, appartment.Id, chest.Item, parsedInput, chest.Type);
                                    }
                                }
                                pick.Visible = false;
                            };
                        }
                        else if (chest.Type == "weapon")
                        {
                            var item = new NativeItem($"{chest.Item} ({chest.Quantity})");
                            pick.Add(item);
                            item.Activated += async (sender, e) =>
                            {
                                var textInput = await GetUserInput("Quantité", "1", 4);
                                var parsedInput = int.Parse(textInput);
                                if (parsedInput <= chest.Quantity)
                                {
                                    if (chest.Quantity <= 0)
                                    {
                                        appartment.ChestInventory.Remove(chest);
                                        pick.Remove(item);
                                    }
                                    pick.Visible = false;
                                }
                            };
                        }
                    }

                    const string ItemTypeKey = "item";
                    const string WeaponTypeKey = "weapon";

                    var items = PlayerInst.Inventory;

                    if (items == null) return;

                    foreach (var item in items.Where(item => item?.Item != null && item.Quantity > 0))
                    {
                        NativeItem invItem;
                        int quantity;

                        switch (item.Type)
                        {
                            case ItemTypeKey:
                                invItem = new NativeItem($"{item.Item} ({item.Quantity})");
                                break;
                            case WeaponTypeKey:
                                invItem = new NativeItem($"{item.Item} ({item.Quantity})");
                                quantity = 1;
                                break;
                            default:
                                continue;
                        }

                        drop.Add(invItem);
                        invItem.Activated += async (sender, e) =>
                        {
                            var textInput = await GetUserInput("Quantité", "1", 4);
                            quantity = int.TryParse(textInput, out var result) ? result : 0;
                            TriggerServerEvent("legal_server:setItemInVault", building.Id, appartment.Id, item.Item, quantity, item.Type);
                            drop.Visible = false;
                        };
                    }

                }
            }
        }

        private List<int> createdObjects = new List<int>();

        private void DeleteRobbingObjects()
        {
            if (createdObjects != null)
            {
                foreach (var obj in createdObjects)
                {
                    if (Function.Call<bool>(Hash.DOES_ENTITY_EXIST, obj))
                    {
                        var rainbowSixSiege = obj; // ??
                        DeleteObject(ref rainbowSixSiege);
                    }
                }
                createdObjects.Clear();
            }
        }

        [EventHandler("updateBuilding")]
        public void UpdateBuilding(string json)
        {
            var updatedBuilding = JsonConvert.DeserializeObject<Building>(json);

            var existingBuilding = Buildings.FirstOrDefault(b => b.Address == updatedBuilding.Address);
            if (existingBuilding != null)
            {
                existingBuilding.Appartments = updatedBuilding.Appartments;
            }
            else
            {
                Buildings.Add(updatedBuilding);
            }
        }

        [EventHandler("legal:updateInventory")]
        public void UpdateInventory(string json)
        {
            var inventory = JsonConvert.DeserializeObject<List<InventoryItem>>(json);
            PlayerInst.Inventory = inventory;
        }

        public void ReceiveAllBuildings(string buildingsJson)
        {
            var buildings = JsonConvert.DeserializeObject<List<Building>>(buildingsJson);
            Buildings = buildings;
        }

        private void OnPlayerSpawned([FromSource] Vector3 pos)
        {
            TriggerServerEvent("legal_server:getAllBuildings");
            List<BlipData> blipsData = new List<BlipData>
            {
                new BlipData { Location = new Vector3(-212, -1326, 30), Sprite = BlipSprite.TowTruck, Color = BlipColor.White, Name = "Benny's" },
                new BlipData { Location = new Vector3(125, -1295, 35), Sprite = BlipSprite.StripClub, Color = BlipColor.Red, Name = "Vanilla Unicorn" },
                new BlipData { Location = new Vector3(918, 50, 80), Sprite = BlipSprite.Castle, Color = BlipColor.Green, Name = "Casino" },
                new BlipData { Location = new Vector3(-562, 277, 83), Sprite = BlipSprite.Bar, Color = BlipColor.Yellow, Name = "Taquila-la" },
                new BlipData { Location = new Vector3(431, -982, 30), Sprite = BlipSprite.PoliceStation, Color = BlipColor.Blue, Name = "Police" },
                new BlipData { Location = new Vector3(-437, -347, 34), Sprite = BlipSprite.Hospital, Color = BlipColor.FranklinGreen, Name = "Hopital" },
                new BlipData { Location = new Vector3(318, -1093, 29), Sprite = BlipSprite.Bar, Color = BlipColor.White, Name = "CoffeeShop" },
                new BlipData { Location = new Vector3(-1190, -893, 19), Sprite = (BlipSprite)106, Color = BlipColor.Yellow, Name = "BurgerShot" },
                new BlipData { Location = new Vector3(-1388, -590, 33), Sprite = BlipSprite.Bar, Color = BlipColor.Yellow, Name = "Bahama" },
                new BlipData { Location = new Vector3(59, 100, 78), Sprite = BlipSprite.GarbageTruck, Color = BlipColor.Green, Name = "LS Transport" },
            };

            foreach (var blipData in blipsData)
            {
                Blip myBlip = World.CreateBlip(blipData.Location);
                myBlip.Sprite = blipData.Sprite;
                myBlip.Color = blipData.Color;
                myBlip.Name = blipData.Name;
                myBlip.IsShortRange = true;
            }
        }

        public void OnClientStart(string resourceName)
        {
            RegisterCommand("job", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (args.Count < 3)
                {
                    SendNotif("~r~Usage: /job [id_joueur] [id_job] [rang_job]");
                    SendNotif("Job list:\n1. Police\n2. EMS\n3. Mecano\n4. Transport\n5. BurgerShot\n" +
                        "6. CoffeeShop\n7. Casino\n8. Taquila\n9. Bahama\n10.Agent Immobilier\n11. Unicorn");
                }
                else
                {
                    var playerId = Convert.ToInt32(args[0]);
                    var jobId = Convert.ToInt32(args[1]);
                    var jobRank = Convert.ToInt32(args[2]);

                    TriggerServerEvent("legal_server:setJob", playerId, jobId, jobRank);
                    if (Metier == null)
                    {
                        SendNotif("~r~Job ID invalide.");
                        return;
                    }
                }
            }), false);
        }

        public void SetDoorState(string jsonCoords, int state)
        {
            var playerPosition = JsonConvert.DeserializeObject<Vector3>(jsonCoords);
            DoorClass nearestDoor = Metier.GetNearestDoor(playerPosition);
            nearestDoor.SetDoorState(state);
        }

        [EventHandler("legal_client:stateRobbing")]
        public void StateRobbing()
        {
            IsRobbing = !IsRobbing;
            SendNotif("~r~Début du cambriolage...");
        }

        public void SendBill(string company, int price, string author)
        {
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
            var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());
            if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10 && playerTarget.Handle != LocalPlayer.Handle)
            {
                TriggerServerEvent("legal_server:sendBill", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)), company, price, author);
                TriggerServerEvent("core:requestPlayerData", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
            }
        }

        public void GetJob(string jsonJob)
        {
            try
            {
                var job = JsonConvert.DeserializeObject<JobInfo>(jsonJob);
                if (job != null && job.JobID > 0)
                {
                    AssignJob(jsonJob);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Legal] Error in GetJob: {ex.Message}");
            }
        }

        [EventHandler("legal_client:assignJob")]
        private void AssignJob(string jsonJob)
        {
            PlayerInst.Job = jsonJob;
            var job = JsonConvert.DeserializeObject<JobInfo>(jsonJob);

            Metier = JobFactory.CreateJob(job.JobID, this);
        }

        public void CallServerEvent(string eventName, params object[] args) => TriggerServerEvent(eventName, args);
        public void ShopPnj(Model ped, float x, float y, float z)
        {
            ped.Request();
            var shop = World.CreatePed(ped, new Vector3(x, y, z), 164);
            FreezeEntityPosition(shop.Result.Handle, true);
            SetEntityInvincible(shop.Result.Handle, true);
            SetBlockingOfNonTemporaryEvents(shop.Result.Handle, true);
            TaskStartScenarioInPlace(shop.Result.Handle, "WORLD_HUMAN_COP_IDLES", 0, true);
        }
        public void AddEvent(string key, Delegate value) => this.EventHandlers.Add(key, value);

        public Player GetPlayer()
        {
            return LocalPlayer;
        }

        public async void PlayAnimation(string animDict, string animName, float speed, AnimationFlags flags)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict)) await BaseScript.Delay(50);
            Game.PlayerPed.Task.PlayAnimation(animDict, animName, speed, -1, flags);
        }

        public void StopAnimation(string animDict, string animName)
        {
            if (Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Game.PlayerPed.Handle, animDict, animName, 3))
            {
                Game.PlayerPed.Task.ClearAnimation(animDict, animName);
            }
        }

        public async Task AddPropToPlayer(string prop1, int bone, float off1, float off2, float off3, float rot1, float rot2, float rot3, int duration)
        {
            int player = PlayerPedId();
            Vector3 playerCoords = GetEntityCoords(player, true);

            RequestModel((uint)GetHashKey(prop1));

            int prop = CreateObject(GetHashKey(prop1), playerCoords.X, playerCoords.Y, playerCoords.Z + 0.2f, true, true, true);
            AttachEntityToEntity(prop, player, GetPedBoneIndex(player, bone), off1, off2, off3, rot1, rot2, rot3, true, true, false, true, 1, true);

            await BaseScript.Delay(duration);

            DeleteEntity(ref prop);

            SetModelAsNoLongerNeeded((uint)GetHashKey(prop1));
        }

        public Notification SendNotif(string text, string title = "ShurikenRP", string subtitle = "Legal Sys.", string icon = "CHAR_STRETCH", Color flashColor = new Color(), bool blink = false, NotificationType type = NotificationType.Default, bool showInBrief = true, bool sound = true)
        {
            AddTextEntry("ScaleformUIAdvancedNotification", text);
            BeginTextCommandThefeedPost("ScaleformUIAdvancedNotification");
            AddTextComponentSubstringPlayerName(text);
            SetNotificationBackgroundColor(140);
            if (!flashColor.IsEmpty && !blink)
                SetNotificationFlashColor(flashColor.R, flashColor.G, flashColor.B, flashColor.A);
            if (sound) Audio.PlaySoundFrontend("DELETE", "HUD_DEATHMATCH_SOUNDSET");
            return new Notification(EndTextCommandThefeedPostMessagetext(icon, icon, true, (int)type, title, subtitle));
            //return new Notification(EndTextCommandThefeedPostTicker(blink, showInBrief));
        }

        public sealed class Notification
        {
            #region Fields
            int _handle;
            #endregion

            internal Notification(int handle)
            {
                _handle = handle;
            }
            public void Hide()
            {
                ThefeedRemoveItem(_handle);
            }
        }

        public void SendTextUI(string text)
        {
            SetTextFont(6);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(false);
            SetTextJustification(0);
            SetTextEntry("STRING");
            AddTextComponentString($"{text}");
            int x = 0, y = 0;
            GetScreenActiveResolution(ref x, ref y);
            DrawText(0.50f, 0.80f);
        }

        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // THANKS vMENU
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await BaseScript.Delay(0);

            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3:
                    case 2:
                        return null;
                    case 1:
                        return GetOnscreenKeyboardResult();
                    default:
                        await BaseScript.Delay(0);
                        break;
                }
            }
        }
        public Task Draw3dText(float x, float y, float z, string text, float size = 0.35f, int r = 255, int g = 255, int b = 255, int a = 215)
        {
            float _x = 0;
            float _y = 0;
            var onScreen = World3dToScreen2d(x, y, z, ref _x, ref _y);
            Vector3 pCoords = GetGameplayCamCoords();

            var distance = GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, x, y, z, true);
            var txtScale = (1 / distance) * 2;
            var fov = (1 / GetGameplayCamFov()) * 100;
            var scale = txtScale * fov * size;

            if (onScreen)
            {
                SetTextScale(0.0f, scale);
                SetTextFont(4);
                SetTextProportional(true);
                SetTextColour(r, g, b, a);
                SetTextDropShadow();
                SetTextEdge(0, 0, 0, 0, 150);
                SetTextDropShadow();
                SetTextOutline();
                SetTextEntry("STRING");
                SetTextCentre(true);
                AddTextComponentString(text);
                DrawText(_x, _y);
            }

            return Task.FromResult(0);
        }

        public void SetCloth(int idHat, int idTop, int idTeeshirt, int idArms, int idLegs, int idShoes)
        {
            SetPedPropIndex(GetPlayerPed(-1), 0, idHat, 0, false);
            SetPedComponentVariation(GetPlayerPed(-1), 8, idTeeshirt, 0, 2);
            SetPedComponentVariation(GetPlayerPed(-1), 11, idTop, 0, 2);
            SetPedComponentVariation(GetPlayerPed(-1), 4, idLegs, 0, 2);
            SetPedComponentVariation(GetPlayerPed(-1), 6, idShoes, 0, 2);
            SetPedComponentVariation(GetPlayerPed(-1), 3, idArms, 0, 2);
        }

        public void CanAutodrive(params string[] models)
        {
            var tesla = GetDisplayNameFromVehicleModel((uint)GetEntityModel(GetVehiclePedIsIn(GetPlayerPed(-1), false)));
            var menu = new NativeMenu(tesla, "Interface utilisateur");
            menu.Banner.Color = Color.FromArgb(255, 255, 100, 30);
            menu.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
            menu.UseMouse = false;
            foreach (var model in models)
            {
                if (tesla.Equals(model))
                {
                    menu.Closed += (sender, e) =>
                    {
                        Pool.Remove(menu);
                    };
                    if (IsControlJustPressed(0, 80))
                    {
                        Pool.Add(menu);
                        menu.Visible = true;
                        var autodrive_state = false;
                        var speed = 100;

                        var autodrive = new NativeListItem<string>("Activer le mode conduite auto.", "<u>Comment ca marche ?</u><br>Poser un point sur la carte, puis activer le mode conduite-auto", "OFF", "ON");
                        menu.Add(autodrive);

                        var drive_speed = new NativeListItem<int>("Vitesse", 50, 80, 90, 100, 110, 130);
                        menu.Add(drive_speed);

                        drive_speed.ItemChanged += (sender, e) =>
                        {
                            speed = e.Index - 1;
                        };

                        var player = GetPlayerPed(-1);
                        var player_coord = GetEntityCoords(player, true);
                        var vehicle = GetVehiclePedIsIn(player, false);
                        Vector3 pos = GetBlipInfoIdCoord(GetFirstBlipInfoId(8));

                        if (pos == Vector3.Zero)
                        {
                            return;
                        }
                        var stopRange = 8;

                        var drivingStyle = 524351;
                        autodrive.ItemChanged += (sender, e) =>
                        {
                            if (e.Index == 1) // ON
                            {
                                autodrive_state = true;
                                SetDriverAbility(player, 1);
                                SetDriverAggressiveness(player, 0);
                                SendNotif($"~o~Vitesse changer à {speed} KM/h");
                                TaskVehicleDriveToCoordLongrange(player, vehicle, pos.X, pos.Y, pos.Z, speed, drivingStyle, stopRange);
                            }
                            else // OFF
                            {
                                autodrive_state = false;
                                SendNotif("~r~L'auto-conduite est désactivé");
                                SetDriverAbility(player, 1);
                                SetDriverAggressiveness(player, 0);
                                TaskVehicleDriveToCoordLongrange(player, vehicle, (float)(player_coord.X + 0.5), player_coord.Y, player_coord.Z, speed, drivingStyle, stopRange);
                            }
                        };
                    }
                }
            }
        }

        [Tick]
        public Task OnTick()
        {
            Pool.Process();
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            BuildingSystem();
            
            // CanAutodrive("MODELS-PD", "E-TRONGT");

            // SendTextUI($"{playerCoords}");

            if (Metier != null)
            {
                if (IsControlPressed(0, 167))
                {
                    Metier.ShowMenu();
                }
                Metier.Garage();
                Metier.Coffre();
                Metier.Cloth();
                Metier.Ticked();
            }

            if (IsRobbing)
            {
                if (createdObjects != null)
                {
                    // CreateRobbingObjects();
                }
            }
            else if (!IsRobbing)
            {
                // DeleteRobbingObjects();
            }
            return Task.FromResult(0);
        }

        [EventHandler("legal:createObjects")]
        public void CreateObjects(string json)
        {
            var decorations = JsonConvert.DeserializeObject<List<Decoration>>(json);
            foreach (Decoration decoration in decorations)
            {
                if (decoration.Props != null)
                {
                    int obj = CreateObject(GetHashKey(decoration.Props),
                    decoration.Position.X, decoration.Position.Y, decoration.Position.Z, false, true, false);
                    createdObjects.Add(obj);
                }
            }
        }

        public void OnColor(string txt)
        {
            var benny = Metier as Bennys;
            benny.VehicleColors = JsonConvert.DeserializeObject<List<VehColors>>(txt);
        }

        public void OnWheels(string txt)
        {
            bennys.VehicleWheels = JsonConvert.DeserializeObject<List<VehWheels>>(txt);
        }

        public void GetPlayerData(string json)
        {
            try
            {
                var player = JsonConvert.DeserializeObject<PlayerInstance>(json);

                if (player != null)
                {
                    PlayerInst.License = player.License;
                    PlayerInst.Id = player.Id;
                    PlayerInst.Firstname = player.Firstname;
                    PlayerInst.Lastname = player.Lastname;
                    PlayerInst.Rank = player.Rank;
                    PlayerInst.Job = player.Job;
                    PlayerInst.Money = player.Money;
                    PlayerInst.Bills = player.Bills;
                    PlayerInst.Inventory = player.Inventory ?? new List<InventoryItem>();
                    PlayerInst.Cars = player.Cars;
                    PlayerInst.Clothes = player.Clothes ?? new List<Shared.ClothingSet>();
                }
                if (!string.IsNullOrEmpty(PlayerInst.Job))
                {
                    try
                    {
                        var jobInfo = JsonConvert.DeserializeObject<JobInfo>(PlayerInst.Job);
                        if (jobInfo != null)
                        {
                            AssignJob(PlayerInst.Job);

                            if (jobInfo.JobID != 0)
                            {
                                TriggerServerEvent("legal_server:requestCompanyData", jobInfo.JobID);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Legal] Error assigning job on player data load: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Legal] Error in GetPlayerData: {ex.Message}");
            }
        }

        public void AskPlayerData()
        {
            TriggerServerEvent("core:requestPlayerData");
        }

        public CompanyInstance CompanyInst = new CompanyInstance
        {
            Id = 0,
            Name = "",
            Chest = new List<InventoryItem>(),
            Taxes = ""
        };

        public void GetCompanyData(string json)
        {
            var company = JsonConvert.DeserializeObject<CompanyInstance>(json);
            CompanyInst.Name = company.Name;
            CompanyInst.Chest = company.Chest;
            CompanyInst.Taxes = company.Taxes;
        }

        public void SendVehicleInfo(Vehicle vehicle)
        {
            if (vehicle.Exists())
            {
                VehicleInfo info = new VehicleInfo
                {
                    Model = API.GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash),
                    Plate = vehicle.Mods.LicensePlate,
                    Boot = new List<BootInfo>(),
                    EngineLevel = vehicle.Mods[VehicleModType.Engine].Index,
                    BrakeLevel = vehicle.Mods[VehicleModType.Brakes].Index,
                    ColorPrimary = (int)vehicle.Mods.PrimaryColor,
                    ColorSecondary = (int)vehicle.Mods.SecondaryColor,
                    Spoiler = vehicle.Mods[VehicleModType.Spoilers].Index,
                    Bumber_F = vehicle.Mods[VehicleModType.FrontBumper].Index,
                    Bumber_R = vehicle.Mods[VehicleModType.RearBumper].Index,
                    Skirt = vehicle.Mods[VehicleModType.SideSkirt].Index,
                    Exhaust = vehicle.Mods[VehicleModType.Exhaust].Index,
                    Chassis = vehicle.Mods[VehicleModType.Frame].Index,
                    Grill = vehicle.Mods[VehicleModType.Grille].Index,
                    Bonnet = vehicle.Mods[VehicleModType.Hood].Index,
                    Wing_L = vehicle.Mods[VehicleModType.Fender].Index,
                    Wing_R = vehicle.Mods[VehicleModType.RightFender].Index,
                    Roof = vehicle.Mods[VehicleModType.Roof].Index,
                    Engine = vehicle.Mods[VehicleModType.Engine].Index,
                    Brakes = vehicle.Mods[VehicleModType.Brakes].Index,
                    Gearbox = vehicle.Mods[VehicleModType.Transmission].Index,
                    Horn = vehicle.Mods[VehicleModType.Horns].Index,
                    Suspension = vehicle.Mods[VehicleModType.Suspension].Index,
                    Armour = vehicle.Mods[VehicleModType.Armor].Index,
                    Subwoofer = vehicle.Mods[VehicleModType.Speakers].Index,
                    Hydraulics = vehicle.Mods[VehicleModType.Hydraulics].Index,
                    Wheels = vehicle.Mods[VehicleModType.FrontWheel].Index,
                    WheelsRearOrHydraulics = vehicle.Mods[VehicleModType.RearWheel].Index,
                    PLTHolder = vehicle.Mods[VehicleModType.PlateHolder].Index,
                    PLTVanity = vehicle.Mods[VehicleModType.VanityPlates].Index,
                    Interior1 = vehicle.Mods[VehicleModType.TrimDesign].Index,
                    Interior2 = vehicle.Mods[VehicleModType.Ornaments].Index,
                    Interior3 = vehicle.Mods[VehicleModType.Dashboard].Index,
                    Interior4 = vehicle.Mods[VehicleModType.DialDesign].Index,
                    Interior5 = vehicle.Mods[VehicleModType.DoorSpeakers].Index,
                    Seats = vehicle.Mods[VehicleModType.Seats].Index,
                    Steering = vehicle.Mods[VehicleModType.SteeringWheels].Index,
                    Knob = vehicle.Mods[VehicleModType.ColumnShifterLevers].Index,
                    Plaque = vehicle.Mods[VehicleModType.Plaques].Index,
                    Ice = vehicle.Mods[VehicleModType.Speakers].Index,
                    Trunk = vehicle.Mods[VehicleModType.Trunk].Index,
                    Hydro = vehicle.Mods[VehicleModType.Hydraulics].Index,
                    EngineBay1 = vehicle.Mods[VehicleModType.EngineBlock].Index,
                    EngineBay2 = vehicle.Mods[VehicleModType.Struts].Index,
                    EngineBay3 = vehicle.Mods[VehicleModType.ArchCover].Index,
                    Chassis2 = vehicle.Mods[VehicleModType.Aerials].Index,
                    Chassis3 = vehicle.Mods[VehicleModType.Trim].Index,
                    Chassis4 = vehicle.Mods[VehicleModType.Trim].Index,
                    Chassis5 = vehicle.Mods[VehicleModType.Tank].Index,
                    Door_L = vehicle.Mods[VehicleModType.Windows].Index,
                    Door_R = vehicle.Mods[VehicleModType.Windows].Index,
                    LiveryMod = vehicle.Mods[VehicleModType.Livery].Index
                };

                List<VehicleInfo> cars = new List<VehicleInfo>
                {
                    info
                };

                string json = JsonConvert.SerializeObject(cars);
                TriggerServerEvent("core:sendVehicleInfo", json);
            }
            else
            {
                SendNotif("Vous n'êtes pas dans une voiture");
            }
        }

    }

    public class BlipData
    {
        public Vector3 Location { get; set; }
        public BlipSprite Sprite { get; set; }
        public BlipColor Color { get; set; }
        public string Name { get; set; }
    }

}
