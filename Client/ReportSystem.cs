using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Core.Shared;

namespace Core.Client
{
    public class ReportSystem
    {
        public List<ReportClass> reports = new List<ReportClass>();
        private object lockObject = new object();

        public void AddReport(ReportClass report)
        {
            lock (lockObject)
            {
                report.Id = reports.Count + 1;
                reports.Add(report);
            }
        }

        public IEnumerable<ReportClass> GetUnresolvedReports()
        {
            lock (lockObject)
            {
                return reports.Where(report => !report.IsResolved).ToList();
            }
        }

        public void MarkReportAsResolved(ReportClass report)
        {
            lock (lockObject)
            {
                report.IsResolved = true;
            }
        }
    }

}
