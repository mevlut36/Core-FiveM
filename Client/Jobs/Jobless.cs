using LemonUI.Menus;

namespace ShurikenLegal.Client.Jobs
{
    public class Jobless : Job
    {
        public Job Metier;
        public ClientMain Client;
        public Jobless(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }
        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 0,
                JobName = "Chômage",
                MenuTitle = "Chômage",
            };
        }

        public override void ShowMenu()
        {
            var job = Client.PlayerInst.Job;
            var menu = new NativeMenu("Chômage", "Menu intéraction")
            {
                TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                Visible = true,
                UseMouse = false,
            };
            Pool.Add(menu);

            var text = new NativeItem("Vous n'avez pas de travail");
            menu.Add(text);
            text.Activated += (sender, e) =>
            {
                Client.SendNotif("Tu as juste a traversé la rue pour trouver un travail");
            };
        }
    }

}
