using CitizenFX.Core;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;
using static ShurikenLegal.Client.ClientMain;

namespace ShurikenLegal.Client.Jobs
{
    public class DoorClass
    {
        public int DoorHash { get; set; }
        public uint ModelHash { get; set; }
        public Vector3 Coordinates { get; set; }

        public void Initialize()
        {
            AddDoorToSystem((uint)DoorHash, ModelHash, Coordinates.X, Coordinates.Y, Coordinates.Z, false, false, false);

            DoorSystemSetDoorState((uint)DoorHash, 1, false, false);
        }

        public void SetDoorState(int state)
        {
            int tempDoorHash = DoorHash;

            if (!DoorSystemFindExistingDoor(Coordinates.X, Coordinates.Y, Coordinates.Z, (int)ModelHash, ref tempDoorHash))
            {
                AddDoorToSystem((uint)DoorHash, ModelHash, Coordinates.X, Coordinates.Y, Coordinates.Z, false, false, false);
            }
            else
            {
                DoorHash = tempDoorHash;
            }

            DoorSystemSetDoorState((uint)DoorHash, state, false, false);
        }

        public int GetDoorState()
        {
            int tempDoorHash = DoorHash;

            if (!DoorSystemFindExistingDoor(Coordinates.X, Coordinates.Y, Coordinates.Z, (int)ModelHash, ref tempDoorHash))
            {
                AddDoorToSystem((uint)DoorHash, ModelHash, Coordinates.X, Coordinates.Y, Coordinates.Z, false, false, false);
                return 1;
            }

            DoorHash = tempDoorHash;

            return DoorSystemGetDoorState((uint)DoorHash);
        }
    }

    public class JobConfig
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public string MenuTitle { get; set; }

        // Positions
        public Vector3 PosCoffreEntreprise { get; set; }
        public Vector3 PosGestionEntreprise { get; set; }
        public Vector3 PosCloth { get; set; }
        public Vector3 PosGarageSortie { get; set; }
        public Vector3 PosGarageEntrer { get; set; }
        public Vector3 PosHeliportSortie { get; set; }
        public Vector3 PosHeliportEntrer { get; set; }
        public List<Vector3> ParkingSpots { get; set; } = new List<Vector3>();

        // Véhicules disponibles
        public List<string> AvailableVehicles { get; set; } = new List<string>();

        // Tenues disponibles
        public Dictionary<string, ClothingSet> Outfits { get; set; } = new Dictionary<string, ClothingSet>();

        // Fonctionnalités activées
        public bool HasAnnounce { get; set; } = true;
        public bool HasBilling { get; set; } = true;
        public bool HasRecruitment { get; set; } = true;
        public bool HasGarage { get; set; } = true;
        public bool HasChest { get; set; } = true;
        public bool HasClothing { get; set; } = true;
        public bool HasDoors { get; set; } = false;

        // Rangs minimum pour certaines actions
        public int MinRankForRecruitment { get; set; } = 1;
        public int MinRankForAnnounce { get; set; } = 0;

        public List<DoorClass> Doors { get; set; } = new List<DoorClass>();
    }

    public class ClothingSet
    {
        public string Name { get; set; }
        public Dictionary<int, ComponentVariation> Components { get; set; } = new Dictionary<int, ComponentVariation>();
    }

    public class ComponentVariation
    {
        public int ComponentId { get; set; }
        public int DrawableId { get; set; }
        public int TextureId { get; set; }
        public int PaletteId { get; set; } = 2;
    }
}