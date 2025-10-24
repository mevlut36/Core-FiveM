using System;
using System.Collections.Generic;

namespace ShurikenLegal.Client.Jobs
{
    public class JobFactory
    {
        private static readonly Dictionary<int, Func<ClientMain, Job>> JobCreators = new Dictionary<int, Func<ClientMain, Job>>
        {
            { 0, client => new Jobless(client) },
            { 1, client => new Police(client) },
            { 2, client => new EMS(client) },
            { 3, client => new Bennys(client) },
            { 4, client => new Transport(client) },
            { 5, client => new BurgerShot(client) },
            { 6, client => new CoffeeShop(client) },
            { 7, client => new Casino(client) },
            { 8, client => new Taquila(client) },
            { 9, client => new Bahama(client) },
            { 10, client => new Immobilier(client) },
            { 11, client => new Unicorn(client) },
            { 12, client => new ConcessAuto(client) },
        };

        public static Job CreateJob(int jobId, ClientMain client)
        {
            if (JobCreators.TryGetValue(jobId, out var creator))
            {
                return creator(client);
            }

            return new Jobless(client);
        }
    }
}