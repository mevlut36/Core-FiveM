using System.Collections.Generic;
using System.IO;
using System.Linq;
using CitizenFX.Core;
using ShurikenLegal.DataContext;
using Newtonsoft.Json;
using System;
using static CitizenFX.Core.Native.API;
using ShurikenLegal.Shared;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ShurikenLegal.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from ShurikenLegal.Server!");
        }

        private string GetPlayerJobJson(LegalContext dbContext, int playerId)
        {
            try
            {
                var employment = dbContext.Employement.FirstOrDefault(e => e.PlayerId == playerId);

                if (employment != null)
                {
                    var jobInfo = new JobInfo
                    {
                        JobID = employment.CompanyId,
                        JobRank = employment.Rank
                    };
                    return JsonConvert.SerializeObject(jobInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting employment data: {ex.Message}");
            }

            var defaultJob = new JobInfo
            {
                JobID = 0,
                JobRank = 0
            };
            return JsonConvert.SerializeObject(defaultJob);
        }

        [EventHandler("legal_server:requestCompanyData")]
        public void GetCompanyData([FromSource] Player player, int id)
        {
            try
            {
                using (var dbContext = new LegalContext())
                {
                    if (id != 0)
                    {
                        var existingCompany = dbContext.Company.FirstOrDefault(u => u.Id == id);

                        if (existingCompany == null)
                        {
                            Debug.WriteLine($"[Legal] Company with ID {id} not found");
                            return;
                        }

                        var companyInstance = new CompanyInstance
                        {
                            Id = existingCompany.Id,
                            Name = existingCompany.Name,
                            Chest = JsonConvert.DeserializeObject<List<InventoryItem>>(existingCompany.Chest ?? "[]"),
                            Taxes = existingCompany.Taxes ?? "0"
                        };

                        var json = JsonConvert.SerializeObject(companyInstance);

                        // Envoyer les données à tous les employés de cette entreprise
                        var allPlayers = dbContext.Player.ToList();
                        var employments = dbContext.Employement.Where(e => e.CompanyId == id).ToList();

                        foreach (var employment in employments)
                        {
                            var playerData = allPlayers.FirstOrDefault(p => p.Id == employment.PlayerId);
                            if (playerData != null)
                            {
                                var p = Players.FirstOrDefault(x => x.Identifiers["license"] == playerData.License);
                                if (p != null)
                                {
                                    TriggerClientEvent(p, "legal_client:getCompanyData", json);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("[Legal] Player is unemployed (job id 0)");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Legal] Error in GetCompanyData: {ex.Message}");
            }
        }

        [EventHandler("legal_server:removeItem")]
        public void RemoveItem([FromSource] Player player, string item, int quantity = 1)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var inventory = JsonConvert.DeserializeObject<List<InventoryItem>>(existingPlayer.Inventory ?? "[]");

                    var itemFilter = inventory.FirstOrDefault(i => i.Item == item);
                    if (itemFilter != null && itemFilter.Quantity >= quantity)
                    {
                        itemFilter.Quantity -= quantity;

                        if (itemFilter.Quantity <= 0)
                        {
                            inventory.Remove(itemFilter);
                        }

                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez perdu ~r~{quantity}~w~ de {item}.");
                    }

                    existingPlayer.Inventory = JsonConvert.SerializeObject(inventory);
                    dbContext.SaveChanges();
                    TriggerEvent("core:requestPlayerData");
                }
            }
        }

        [EventHandler("legal_server:setDoorState")]
        public void SetDoorState([FromSource] Player player, string jsonCoords, int state)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonCoords))
                {
                    Debug.WriteLine("[ERROR] SetDoorState received null or empty jsonCoords");
                    return;
                }

                if (state != 0 && state != 1)
                {
                    Debug.WriteLine($"[ERROR] SetDoorState received invalid state: {state}");
                    return;
                }

                TriggerClientEvent("legal_client:setDoorState", jsonCoords, state);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error in SetDoorState: {ex.Message}");
            }
        }

        [EventHandler("legal_server:updateCar")]
        public void UpdateCar([FromSource] Player player, int playerId, string jsonCar)
        {
            var targetPlayer = Players[playerId];
            var car = JsonConvert.DeserializeObject<VehicleInfo>(jsonCar);

            if (targetPlayer != null && car != null)
            {
                using (var dbContext = new LegalContext())
                {
                    var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == targetPlayer.Identifiers["license"]);

                    if (existingPlayer != null)
                    {
                        var existingCar = dbContext.Car.FirstOrDefault(c => c.Plate == car.Plate && c.PlayerId == existingPlayer.Id);

                        if (existingCar != null)
                        {
                            existingCar.Model = car.Model;
                            existingCar.PrimaryColor = car.ColorPrimary;
                            existingCar.SecondaryColor = car.ColorSecondary;
                            existingCar.Boot = JsonConvert.SerializeObject(car.Boot ?? new List<BootInfo>());

                            dbContext.SaveChanges();
                            TriggerClientEvent(player, "core:sendNotif", "~g~Véhicule mis à jour");
                        }
                        else
                        {
                            TriggerClientEvent(player, "core:sendNotif", "~r~Ce joueur n'a pas ce véhicule");
                        }

                        TriggerEvent("core:requestPlayerData");
                    }
                }
            }
        }

        [EventHandler("legal_server:ordering")]
        public async void Ordering([FromSource] Player player, string item, string quantity, string type)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var jobJson = GetPlayerJobJson(dbContext, existingPlayer.Id);
                    var job = JsonConvert.DeserializeObject<JobInfo>(jobJson);

                    int jobId = job.JobID;
                    var existingCompany = dbContext.Company.FirstOrDefault(u => u.Id == jobId);

                    if (existingCompany != null)
                    {
                        var chests = JsonConvert.DeserializeObject<List<InventoryItem>>(existingCompany.Chest ?? "[]");
                        int qty = int.Parse(quantity);

                        var itemFilter = chests.FirstOrDefault(i => i.Item == item);

                        TriggerClientEvent(player, "core:sendNotif", $"~g~Votre commande est en route !");
                        await Delay(10000);

                        if (itemFilter != null)
                        {
                            itemFilter.Quantity += qty;
                        }
                        else
                        {
                            var newItem = new InventoryItem
                            {
                                Item = item,
                                Quantity = qty,
                                Type = type,
                            };
                            chests.Add(newItem);
                        }

                        existingCompany.Chest = JsonConvert.SerializeObject(chests);
                        dbContext.SaveChanges();

                        TriggerClientEvent(player, "core:sendNotif", $"~g~Votre commande est bien arrivée !");
                        GetCompanyData(player, jobId);
                    }
                }
            }
        }

        [EventHandler("legal_server:recruit")]
        public void Recruit([FromSource] Player player, int jobId, int playerId)
        {
            Player playerTarget = Players[playerId];

            if (playerTarget == null)
            {
                TriggerClientEvent(player, "core:sendNotif", "~r~Joueur introuvable");
                return;
            }

            using (var dbContext = new LegalContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == playerTarget.Identifiers["license"]);

                if (existingPlayer != null)
                {
                    var employment = dbContext.Employement.FirstOrDefault(e => e.PlayerId == existingPlayer.Id);

                    if (employment == null)
                    {
                        // Créer un nouvel emploi
                        employment = new EmployementTable
                        {
                            PlayerId = existingPlayer.Id,
                            CompanyId = 0,
                            Rank = 0
                        };
                        dbContext.Employement.Add(employment);
                    }

                    if (employment.CompanyId == jobId)
                    {
                        // Promotion dans la même entreprise
                        if (employment.Rank >= 5)
                        {
                            TriggerClientEvent(playerTarget, "core:sendNotif",
                                $"~r~Vous avez déjà atteint le rang maximum.");
                            TriggerClientEvent(player, "core:sendNotif",
                                $"{existingPlayer.FirstName} {existingPlayer.LastName} " +
                                $"({playerTarget.Name}) a déjà atteint le rang maximum.");
                        }
                        else
                        {
                            employment.Rank += 1;
                            TriggerClientEvent(playerTarget, "core:sendNotif",
                                $"~g~Vous avez été promu. Rang: {employment.Rank}");
                            TriggerClientEvent(player, "core:sendNotif",
                                $"Vous avez bien promu " +
                                $"{existingPlayer.FirstName} {existingPlayer.LastName} ({playerTarget.Name})");
                        }
                    }
                    else
                    {
                        // Nouveau recrutement
                        employment.CompanyId = jobId;
                        employment.Rank = 1;

                        TriggerClientEvent(playerTarget, "core:sendNotif",
                            $"~g~Félicitation pour votre emploi. Rang: {employment.Rank}");
                        TriggerClientEvent(player, "core:sendNotif",
                            $"Vous avez bien recruté " +
                            $"{existingPlayer.FirstName} {existingPlayer.LastName} ({playerTarget.Name})");

                        // Envoyer les données du job au client
                        var newJobJson = GetPlayerJobJson(dbContext, existingPlayer.Id);
                        TriggerClientEvent(playerTarget, "legal_client:assignJob", newJobJson);
                        GetCompanyData(playerTarget, jobId);
                    }

                    dbContext.SaveChanges();
                }
            }
        }

        [EventHandler("legal_server:vehicle_colors")]
        public void VehicleColors([FromSource] Player player)
        {
            var txt_file = File.ReadAllLines("resources/[net]/ShurikenLegal/Server/VehicleColors.json");
            string txt = string.Join("\n", txt_file);
            TriggerClientEvent(player, "legal_client:colors", txt);
        }

        [EventHandler("legal_server:vehicle_wheels")]
        public void VehicleWheels([FromSource] Player player)
        {
            var txt_file = File.ReadAllLines("resources/[net]/ShurikenLegal/Client/Jobs/VehicleWheels.json");
            string txt = string.Join("\n", txt_file);
            TriggerClientEvent(player, "legal_client:wheels", txt);
        }

        [EventHandler("legal_server:sendBill")]
        public void SendBill([FromSource] Player player, int playerServerId, string company, int price, string author)
        {
            Player playerTarget = Players[playerServerId];

            if (playerTarget != null)
            {
                using (var dbContext = new LegalContext())
                {
                    var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == playerTarget.Identifiers["license"]);

                    if (existingPlayer != null)
                    {
                        var bill = new BillsInfo
                        {
                            Company = company,
                            Amount = price,
                            Author = author,
                            Date = DateTime.Now.ToString("dd/MM/yyyy")
                        };

                        List<BillsInfo> existingBills = string.IsNullOrEmpty(existingPlayer.Bills)
                            ? new List<BillsInfo>()
                            : JsonConvert.DeserializeObject<List<BillsInfo>>(existingPlayer.Bills);

                        existingBills.Add(bill);
                        existingPlayer.Bills = JsonConvert.SerializeObject(existingBills);
                        dbContext.SaveChanges();

                        TriggerClientEvent(playerTarget, "core:sendNotif", "Vous avez reçu une facture");
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez envoyé une facture à {playerTarget.Name}");
                    }
                }
            }
        }

        [EventHandler("legal_server:setJob")]
        public void SetJob([FromSource] Player player, int playerId, int jobId, int jobRank)
        {
            Player targetPlayer = Players[playerId];

            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }

            var license = targetPlayer.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                var playerStaff = dbContext.Player.FirstOrDefault(u => u.License == player.Identifiers["license"]);

                if (playerStaff == null || playerStaff.Rank != "staff")
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Tu n'es pas staff");
                    return;
                }

                if (existingPlayer != null)
                {
                    var employment = dbContext.Employement.FirstOrDefault(e => e.PlayerId == existingPlayer.Id);

                    if (employment == null)
                    {
                        employment = new EmployementTable
                        {
                            PlayerId = existingPlayer.Id,
                            CompanyId = jobId,
                            Rank = jobRank
                        };
                        dbContext.Employement.Add(employment);
                    }
                    else
                    {
                        employment.CompanyId = jobId;
                        employment.Rank = jobRank;
                    }

                    dbContext.SaveChanges();

                    var jobJson = GetPlayerJobJson(dbContext, existingPlayer.Id);
                    TriggerClientEvent(targetPlayer, "legal_client:assignJob", jobJson);

                    if (jobId != 0)
                    {
                        GetCompanyData(targetPlayer, jobId);
                    }

                }
            }
        }

        /* APPARTMENT EVENTS */
        [EventHandler("legal_server:getAllBuildings")]
        public void GetBuildings()
        {
            using (var dbContext = new LegalContext())
            {
                var buildingsFromDb = dbContext.Immobilier.ToList();
                var buildings = new List<Building>();

                foreach (var buildingData in buildingsFromDb)
                {
                    var building = new Building(
                        buildingData.Address,
                        JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(buildingData.Door),
                        JsonConvert.DeserializeObject<List<Appartment>>(buildingData.Appartments ?? "[]")
                    );

                    building.Id = buildingData.Id;
                    buildings.Add(building);
                }

                TriggerClientEvent("legal_client:allBuildings", JsonConvert.SerializeObject(buildings));
            }
        }

        [EventHandler("legal_server:getBuilding")]
        public void GetBuilding([FromSource] Player player, string address)
        {
            Task.Run(async () =>
            {
                using (var dbContext = new LegalContext())
                {
                    var existingBuilding = await dbContext.Immobilier.FirstOrDefaultAsync(u => u.Address == address);

                    if (existingBuilding != null)
                    {
                        var building = new Building(
                            existingBuilding.Address,
                            JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(existingBuilding.Door),
                            JsonConvert.DeserializeObject<List<Appartment>>(existingBuilding.Appartments ?? "[]")
                        );

                        TriggerClientEvent("legal_client:building", JsonConvert.SerializeObject(building));
                    }
                }
            });
        }

        [EventHandler("legal_server:addBuilding")]
        public void AddBuilding([FromSource] Player player, string json)
        {
            using (var dbContext = new LegalContext())
            {
                var building = JsonConvert.DeserializeObject<Building>(json);
                var existingBuilding = dbContext.Immobilier.FirstOrDefault(u => u.Address == building.Address);

                if (existingBuilding == null)
                {
                    var newBuilding = new ImmobilierTable
                    {
                        Address = building.Address,
                        Door = JsonConvert.SerializeObject(building.Door),
                        Appartments = JsonConvert.SerializeObject(building.Appartments ?? new List<Appartment>())
                    };

                    dbContext.Immobilier.Add(newBuilding);
                    dbContext.SaveChanges();

                    TriggerClientEvent(player, "core:sendNotif", "L'immeuble a bien été enregistré");
                    TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(building));
                }
                else
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Cet immeuble existe déjà");
                }
            }
        }

        [EventHandler("legal_server:addAppart")]
        public void AddAppart([FromSource] Player player, string address, string jsonAppart)
        {
            using (var dbContext = new LegalContext())
            {
                var appart = JsonConvert.DeserializeObject<Appartment>(jsonAppart);
                var existingBuilding = dbContext.Immobilier.FirstOrDefault(u => u.Address == address);

                if (existingBuilding != null)
                {
                    appart.PlayerLicense = "Null";
                    appart.Resident = "Libre";
                    appart.IsLocked = true;

                    var appartsList = JsonConvert.DeserializeObject<List<Appartment>>(existingBuilding.Appartments ?? "[]");

                    appart.Id = appartsList.Any() ? appartsList.Max(a => a.Id) + 1 : 1;
                    appart.Decorations = new List<Decoration>();
                    appartsList.Add(appart);

                    existingBuilding.Appartments = JsonConvert.SerializeObject(appartsList);
                    dbContext.Entry(existingBuilding).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    var building = new Building(
                        existingBuilding.Address,
                        JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(existingBuilding.Door),
                        appartsList
                    );

                    TriggerClientEvent(player, "core:sendNotif", "L'appartement a bien été enregistré");
                    TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(building));
                }
                else
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'adresse du bâtiment n'existe pas dans la base de données");
                }
            }
        }

        [EventHandler("legal_server:enterAppart")]
        public void EnterAppart([FromSource] Player player, int buildingId, int appartId)
        {
            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Immeuble introuvable");
                    return;
                }

                var existingApparts = JsonConvert.DeserializeObject<List<Appartment>>(building.Appartments ?? "[]");
                var thisAppart = existingApparts.FirstOrDefault(v => v.Id == appartId);

                if (thisAppart == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'appartement n'existe pas dans la base de données");
                    return;
                }

                if (thisAppart.IsLocked)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Le propriétaire vient tout juste de fermer son appartement.");
                }
                else
                {
                    var interior = thisAppart.Interior;
                    SetEntityCoords(player.Character.Handle, interior.X, interior.Y, interior.Z, true, false, true, true);
                    SetPlayerRoutingBucket(player.Handle, thisAppart.Id);

                    if (thisAppart.Decorations != null && thisAppart.Decorations.Any())
                    {
                        TriggerClientEvent(player, "legal:createObjects", JsonConvert.SerializeObject(thisAppart.Decorations));
                    }
                }
            }
        }

        [EventHandler("legal_server:exitAppart")]
        public void ExitAppart([FromSource] Player player, int buildingId)
        {
            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building != null)
                {
                    var doorCoords = JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(building.Door);
                    SetEntityCoords(player.Character.Handle, doorCoords.X, doorCoords.Y, doorCoords.Z, true, false, true, true);
                    SetPlayerRoutingBucket(player.Handle, 0);
                }
            }
        }

        [EventHandler("legal_server:robAppart")]
        public void RobAppart([FromSource] Player player, int buildingId, int appartId)
        {
            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'adresse du bâtiment n'existe pas dans la base de données");
                    return;
                }

                var existingApparts = JsonConvert.DeserializeObject<List<Appartment>>(building.Appartments ?? "[]");
                var thisAppart = existingApparts.FirstOrDefault(v => v.Id == appartId);

                if (thisAppart == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'appartement n'existe pas dans la base de données");
                    return;
                }

                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);

                if (existingPlayer == null) return;

                var pInventory = JsonConvert.DeserializeObject<List<InventoryItem>>(existingPlayer.Inventory ?? "[]");
                var itemFilter = pInventory.FirstOrDefault(i => i.Item == "Outil de crochetage");

                if (itemFilter == null || itemFilter.Quantity <= 0)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Vous n'avez pas d'outil de crochetage");
                    return;
                }

                var random = new Random();

                if (random.Next(1, 3) == 1) // 50% de chance
                {
                    itemFilter.Quantity -= 1;
                    existingPlayer.Inventory = JsonConvert.SerializeObject(pInventory);
                    dbContext.SaveChanges();

                    var interior = thisAppart.Interior;
                    SetEntityCoords(player.Character.Handle, interior.X, interior.Y, interior.Z, true, false, true, true);
                    SetPlayerRoutingBucket(player.Handle, thisAppart.Id);
                    TriggerClientEvent(player, "legal_client:stateRobbing");
                    TriggerEvent("core:requestPlayerData");
                }
                else
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Tentative échouée...");
                }
            }
        }

        [EventHandler("legal_server:cancelRobbing")]
        public void CancelRobbing([FromSource] Player player, int buildingId)
        {
            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'adresse du bâtiment n'existe pas");
                    return;
                }

                var doorCoords = JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(building.Door);
                SetEntityCoords(player.Character.Handle, doorCoords.X, doorCoords.Y, doorCoords.Z, true, false, true, true);
                SetPlayerRoutingBucket(player.Handle, 0);
                TriggerClientEvent("legal_client:stateRobbing");
            }
        }

        [EventHandler("legal_server:assignAppart")]
        public void AssignAppart([FromSource] Player player, int playerId, string address, string json)
        {
            var appart = JsonConvert.DeserializeObject<Appartment>(json);
            Player targetPlayer = Players[playerId];

            if (targetPlayer == null)
            {
                TriggerClientEvent(player, "core:sendNotif", "~r~Joueur introuvable");
                return;
            }

            var license = targetPlayer.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var existingBuilding = dbContext.Immobilier.FirstOrDefault(u => u.Address == address);
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingBuilding != null && existingPlayer != null)
                {
                    var appartsList = JsonConvert.DeserializeObject<List<Appartment>>(existingBuilding.Appartments ?? "[]");
                    var thisAppart = appartsList.FirstOrDefault(v => v.Id == appart.Id);

                    if (thisAppart != null)
                    {
                        thisAppart.PlayerLicense = license;
                        thisAppart.Resident = $"{existingPlayer.FirstName} {existingPlayer.LastName}";
                        existingBuilding.Appartments = JsonConvert.SerializeObject(appartsList);

                        dbContext.Entry(existingBuilding).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        var building = new Building(
                            existingBuilding.Address,
                            JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(existingBuilding.Door),
                            appartsList
                        );

                        TriggerClientEvent(targetPlayer, "core:sendNotif", "~g~L'appartement a bien été assigné");
                        TriggerClientEvent(player, "core:sendNotif",
                            $"Vous avez assigné un appartement au nom de {existingPlayer.FirstName} {existingPlayer.LastName}");
                        TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(building));
                    }
                }
            }
        }

        [EventHandler("legal_server:stateDoor")]
        public void StateDoorId([FromSource] Player player, int buildingId, int appartId)
        {
            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building != null)
                {
                    var existingApparts = JsonConvert.DeserializeObject<List<Appartment>>(building.Appartments ?? "[]");
                    var thisAppart = existingApparts.FirstOrDefault(v => v.Id == appartId);

                    if (thisAppart != null)
                    {
                        thisAppart.IsLocked = !thisAppart.IsLocked;
                        building.Appartments = JsonConvert.SerializeObject(existingApparts);

                        dbContext.Entry(building).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        var doorState = thisAppart.IsLocked ? "fermé" : "ouvert";
                        TriggerClientEvent(player, "core:sendNotif", $"L'appartement est maintenant {doorState}.");

                        var newBuilding = new Building(
                            building.Address,
                            JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(building.Door),
                            existingApparts
                        );

                        TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(newBuilding));
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", "~r~L'appartement n'existe pas");
                    }
                }
                else
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'adresse du bâtiment n'existe pas");
                }
            }
        }

        [EventHandler("legal_server:getItemFromVault")]
        public void GetItemFromVault([FromSource] Player player, int buildingId, int appartId, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building != null)
                {
                    var existingApparts = JsonConvert.DeserializeObject<List<Appartment>>(building.Appartments ?? "[]");
                    var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                    if (existingPlayer == null) return;

                    var thisAppart = existingApparts.FirstOrDefault(v => v.Id == appartId);

                    if (thisAppart == null)
                    {
                        TriggerClientEvent(player, "core:sendNotif", "~r~L'appartement n'existe pas");
                        return;
                    }

                    var invPlayer = JsonConvert.DeserializeObject<List<InventoryItem>>(existingPlayer.Inventory ?? "[]");
                    var chest = thisAppart.ChestInventory ?? new List<InventoryItem>();

                    var itemFilter = chest.FirstOrDefault(i => i.Item == item);
                    var playerItem = invPlayer.FirstOrDefault(i => i.Item == item);

                    if (itemFilter != null && itemFilter.Quantity >= quantity)
                    {
                        itemFilter.Quantity -= quantity;

                        if (itemFilter.Quantity <= 0)
                        {
                            chest.Remove(itemFilter);
                        }

                        if (playerItem != null)
                        {
                            playerItem.Quantity += quantity;
                        }
                        else
                        {
                            var newItem = new InventoryItem
                            {
                                Type = type,
                                Item = item,
                                Quantity = quantity
                            };
                            invPlayer.Add(newItem);
                        }

                        thisAppart.ChestInventory = chest;
                        building.Appartments = JsonConvert.SerializeObject(existingApparts);
                        existingPlayer.Inventory = JsonConvert.SerializeObject(invPlayer);

                        dbContext.Entry(building).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez récupéré {quantity} de {item}");
                        TriggerClientEvent(player, "core:updateInventory", existingPlayer.Inventory);
                        TriggerClientEvent(player, "legal_client:updateInventory", existingPlayer.Inventory);

                        var newBuilding = new Building(
                            building.Address,
                            JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(building.Door),
                            existingApparts
                        );
                        TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(newBuilding));
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~La quantité est trop importante");
                    }
                }
                else
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'adresse du bâtiment n'existe pas");
                }
            }
        }

        [EventHandler("legal_server:setItemInVault")]
        public void SetItemInVault([FromSource] Player player, int buildingId, int appartId, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building != null)
                {
                    var existingApparts = JsonConvert.DeserializeObject<List<Appartment>>(building.Appartments ?? "[]");
                    var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                    if (existingPlayer == null) return;

                    var thisAppart = existingApparts.FirstOrDefault(v => v.Id == appartId);

                    if (thisAppart == null)
                    {
                        TriggerClientEvent(player, "core:sendNotif", "~r~L'appartement n'existe pas");
                        return;
                    }

                    var invPlayer = JsonConvert.DeserializeObject<List<InventoryItem>>(existingPlayer.Inventory ?? "[]");
                    var chest = thisAppart.ChestInventory ?? new List<InventoryItem>();

                    var itemFilter = chest.FirstOrDefault(i => i.Item == item);
                    var playerItem = invPlayer.FirstOrDefault(i => i.Item == item);

                    if (playerItem != null && playerItem.Quantity >= quantity)
                    {
                        playerItem.Quantity -= quantity;

                        if (playerItem.Quantity <= 0)
                        {
                            invPlayer.Remove(playerItem);
                        }

                        if (itemFilter != null)
                        {
                            itemFilter.Quantity += quantity;
                        }
                        else
                        {
                            var newItem = new InventoryItem
                            {
                                Type = type,
                                Item = item,
                                Quantity = quantity
                            };
                            chest.Add(newItem);
                        }

                        thisAppart.ChestInventory = chest;
                        building.Appartments = JsonConvert.SerializeObject(existingApparts);
                        existingPlayer.Inventory = JsonConvert.SerializeObject(invPlayer);

                        dbContext.Entry(building).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez déposé {quantity} de {item}");
                        TriggerClientEvent(player, "core:updateInventory", existingPlayer.Inventory);
                        TriggerClientEvent(player, "legal:updateInventory", existingPlayer.Inventory);

                        var newBuilding = new Building(
                            building.Address,
                            JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(building.Door),
                            existingApparts
                        );
                        TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(newBuilding));
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~La quantité est trop importante");
                    }
                }
                else
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'adresse du bâtiment n'existe pas");
                }
            }
        }

        [EventHandler("legal:addDecoration")]
        public void AddDecoration([FromSource] Player player, int buildingId, int appartId, string jsonPosition, string obj)
        {
            using (var dbContext = new LegalContext())
            {
                var building = dbContext.Immobilier.FirstOrDefault(b => b.Id == buildingId);

                if (building == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Immeuble introuvable");
                    return;
                }

                var existingApparts = JsonConvert.DeserializeObject<List<Appartment>>(building.Appartments ?? "[]");
                var thisAppart = existingApparts.FirstOrDefault(v => v.Id == appartId);

                if (thisAppart == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~L'appartement n'existe pas");
                    return;
                }

                var position = JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(jsonPosition);
                var deco = thisAppart.Decorations?.FirstOrDefault(d => d.Position == position);

                if (deco != null)
                {
                    deco.Props = obj;
                }
                else
                {
                    if (thisAppart.Decorations == null)
                        thisAppart.Decorations = new List<Decoration>();

                    thisAppart.Decorations.Add(new Decoration(obj, position));
                }

                building.Appartments = JsonConvert.SerializeObject(existingApparts);
                dbContext.Entry(building).State = EntityState.Modified;
                dbContext.SaveChanges();

                var newBuilding = new Building(
                    building.Address,
                    JsonConvert.DeserializeObject<CitizenFX.Core.Vector3>(building.Door),
                    existingApparts
                );
                TriggerClientEvent("updateBuilding", JsonConvert.SerializeObject(newBuilding));
            }
        }

        [EventHandler("legal:setRoutingBucket")]
        public void SetRoutingBucket([FromSource] Player player, int targetPlayerId, int bucket)
        {
            var targetPlayer = Players[targetPlayerId];
            if (targetPlayer != null)
            {
                SetPlayerRoutingBucket(targetPlayer.Handle, bucket);
            }
        }

        [EventHandler("legal_server:getItemFromCompany")]
        public void GetItemFromCompany([FromSource] Player player, int companyId, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var existingCompany = dbContext.Company.FirstOrDefault(c => c.Id == companyId);
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);

                if (existingCompany == null || existingPlayer == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Erreur lors de la récupération des données");
                    return;
                }

                var companyChest = JsonConvert.DeserializeObject<List<InventoryItem>>(existingCompany.Chest ?? "[]");
                var playerInventory = JsonConvert.DeserializeObject<List<InventoryItem>>(existingPlayer.Inventory ?? "[]");

                var chestItem = companyChest.FirstOrDefault(i => i.Item == item);
                if (chestItem == null || chestItem.Quantity < quantity)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Quantité insuffisante dans le coffre");
                    return;
                }

                chestItem.Quantity -= quantity;
                if (chestItem.Quantity <= 0)
                {
                    companyChest.Remove(chestItem);
                }

                var playerItem = playerInventory.FirstOrDefault(i => i.Item == item);
                if (playerItem != null)
                {
                    playerItem.Quantity += quantity;
                }
                else
                {
                    playerInventory.Add(new InventoryItem
                    {
                        Item = item,
                        Type = type,
                        Quantity = quantity
                    });
                }

                existingCompany.Chest = JsonConvert.SerializeObject(companyChest);
                existingPlayer.Inventory = JsonConvert.SerializeObject(playerInventory);
                dbContext.SaveChanges();

                TriggerClientEvent(player, "core:sendNotif", $"~g~Vous avez récupéré {quantity} {item}");
                TriggerClientEvent(player, "legal_client:updateInventory", existingPlayer.Inventory);

                GetCompanyData(player, companyId);
            }
        }

        [EventHandler("legal_server:setItemInCompany")]
        public void SetItemInCompany([FromSource] Player player, int companyId, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var existingCompany = dbContext.Company.FirstOrDefault(c => c.Id == companyId);
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);

                if (existingCompany == null || existingPlayer == null)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Erreur lors de la récupération des données");
                    return;
                }

                var companyChest = JsonConvert.DeserializeObject<List<InventoryItem>>(existingCompany.Chest ?? "[]");
                var playerInventory = JsonConvert.DeserializeObject<List<InventoryItem>>(existingPlayer.Inventory ?? "[]");

                var playerItem = playerInventory.FirstOrDefault(i => i.Item == item);
                if (playerItem == null || playerItem.Quantity < quantity)
                {
                    TriggerClientEvent(player, "core:sendNotif", "~r~Vous n'avez pas assez de cet objet");
                    return;
                }

                playerItem.Quantity -= quantity;
                if (playerItem.Quantity <= 0)
                {
                    playerInventory.Remove(playerItem);
                }

                var chestItem = companyChest.FirstOrDefault(i => i.Item == item);
                if (chestItem != null)
                {
                    chestItem.Quantity += quantity;
                }
                else
                {
                    companyChest.Add(new InventoryItem
                    {
                        Item = item,
                        Type = type,
                        Quantity = quantity
                    });
                }

                existingCompany.Chest = JsonConvert.SerializeObject(companyChest);
                existingPlayer.Inventory = JsonConvert.SerializeObject(playerInventory);
                dbContext.SaveChanges();

                TriggerClientEvent(player, "core:sendNotif", $"~g~Vous avez déposé {quantity} {item}");
                TriggerClientEvent(player, "legal_client:updateInventory", existingPlayer.Inventory);
                TriggerClientEvent(player, "core:updateInventory", existingPlayer.Inventory);

                GetCompanyData(player, companyId);
            }
        }

        [EventHandler("police:requestPlayerInventory")]
        public void RequestPlayerInventory([FromSource] Player officer, int targetId)
        {
            var targetPlayer = Players[targetId];
            if (targetPlayer == null)
            {
                TriggerClientEvent(officer, "core:sendNotif", "~r~Joueur introuvable.");
                return;
            }

            var officerLicense = officer.Identifiers["license"];
            var targetLicense = targetPlayer.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var officerData = dbContext.Player.FirstOrDefault(p => p.License == officerLicense);
                if (officerData == null) return;

                var employment = dbContext.Employement.FirstOrDefault(e => e.PlayerId == officerData.Id);
                if (employment == null || employment.CompanyId != 1)
                {
                    TriggerClientEvent(officer, "core:sendNotif", "~r~Vous n'êtes pas autorisé à effectuer cette action.");
                    return;
                }

                var targetData = dbContext.Player.FirstOrDefault(p => p.License == targetLicense);
                if (targetData == null) return;

                var inventory = targetData.Inventory ?? "[]";

                TriggerClientEvent(officer, "police:receivePlayerInventory", inventory, targetId);

                TriggerClientEvent(targetPlayer, "core:sendNotif", "~y~Vous êtes en train d'être fouillé par un agent de police.");
            }
        }

        [EventHandler("police:confiscateItem")]
        public void ConfiscateItem([FromSource] Player officer, int targetId, string itemName, int quantity, string itemType)
        {
            var targetPlayer = Players[targetId];
            if (targetPlayer == null)
            {
                TriggerClientEvent(officer, "core:sendNotif", "~r~Joueur introuvable.");
                return;
            }

            var officerLicense = officer.Identifiers["license"];
            var targetLicense = targetPlayer.Identifiers["license"];

            using (var dbContext = new LegalContext())
            {
                var officerData = dbContext.Player.FirstOrDefault(p => p.License == officerLicense);
                if (officerData == null) return;

                var employment = dbContext.Employement.FirstOrDefault(e => e.PlayerId == officerData.Id);
                if (employment == null || employment.CompanyId != 1 || officerData.Rank != "staff")
                {
                    TriggerClientEvent(officer, "core:sendNotif", "~r~Vous n'êtes pas autorisé à effectuer cette action.");
                    return;
                }

                var targetData = dbContext.Player.FirstOrDefault(p => p.License == targetLicense);
                if (targetData == null) return;

                var targetInventory = JsonConvert.DeserializeObject<List<InventoryItem>>(targetData.Inventory ?? "[]");

                var item = targetInventory.FirstOrDefault(i => i.Item == itemName && i.Type == itemType);
                if (item == null || item.Quantity < quantity)
                {
                    TriggerClientEvent(officer, "core:sendNotif", "~r~L'objet n'est plus disponible en cette quantité.");
                    return;
                }

                item.Quantity -= quantity;

                if (item.Quantity <= 0)
                {
                    targetInventory.Remove(item);
                }

                targetData.Inventory = JsonConvert.SerializeObject(targetInventory);
                dbContext.SaveChanges();

                var officerInventory = JsonConvert.DeserializeObject<List<InventoryItem>>(officerData.Inventory ?? "[]");

                var officerItem = officerInventory.FirstOrDefault(i => i.Item == itemName && i.Type == itemType);
                if (officerItem != null)
                {
                    officerItem.Quantity += quantity;
                }
                else
                {
                    officerInventory.Add(new InventoryItem
                    {
                        Item = itemName,
                        Type = itemType,
                        Quantity = quantity
                    });
                }

                officerData.Inventory = JsonConvert.SerializeObject(officerInventory);
                dbContext.SaveChanges();

                TriggerClientEvent(targetPlayer, "core:sendNotif", $"~r~Un agent de police a confisqué {quantity} {itemName}.");

                TriggerClientEvent(targetPlayer, "legal:updateInventory", targetData.Inventory);
                TriggerClientEvent(targetPlayer, "core:updateInventory", targetData.Inventory);

                TriggerClientEvent(officer, "legal:updateInventory", officerData.Inventory);
                TriggerClientEvent(officer, "core:updateInventory", officerData.Inventory);
            }
        }
    }
}