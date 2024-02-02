using ManufacturingInventory.Application.Boundaries.Authentication;
using ManufacturingInventory.Domain.Enums;
using ManufacturingInventory.Infrastructure.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManufacturingInventory.Application.Boundaries.ReportingBoundaries {

    public class MonthlyReportInput {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public CollectType CollectType { get; set; }
        public double ProductionPercent { get; set; }
        public double ResearchPercent { get; set; }

        public MonthlyReportInput() {
            this.StartDate = DateTime.Now;
            this.StopDate = DateTime.Now;
            this.CollectType = CollectType.OnlyCostReported;
            this.ProductionPercent = 90;
            this.ResearchPercent = 10;
        }

        public MonthlyReportInput(DateTime start,DateTime stop,CollectType collectType) {
            this.StartDate = start;
            this.StopDate = stop;
            this.CollectType = collectType;
            this.ProductionPercent = 90;
            this.ResearchPercent = 10;
        }
        
        public MonthlyReportInput(DateTime start,DateTime stop,CollectType collectType,double pPer,double rPer) {
            this.StartDate = start;
            this.StopDate = stop;
            this.CollectType = collectType;
            this.ProductionPercent = pPer;
            this.ResearchPercent = rPer;
        }
    }


    public class TransactionSummaryInput {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public CollectType CollectType { get; set; }

        public TransactionSummaryInput() {
            this.StartDate = DateTime.Now;
            this.StopDate = DateTime.Now;
            this.CollectType = CollectType.OnlyCostReported;
        }

        public TransactionSummaryInput(DateTime start, DateTime stop,CollectType collectType) {
            this.StartDate = start;
            this.StopDate = stop;
            this.CollectType = collectType;
        }
    }

    public class CurrentInventoryInput {
        public CollectType CollectType { get; set; }

        public CurrentInventoryInput(CollectType collectType) {
            this.CollectType = collectType;
        }

        public CurrentInventoryInput() {
            this.CollectType = CollectType.OnlyCostReported;
        }
    }
}
