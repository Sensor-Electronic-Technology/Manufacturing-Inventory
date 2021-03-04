using DevExpress.Mvvm.Native;
using ManufacturingInventory.Application.Boundaries.ReportingBoundaries;
using ManufacturingInventory.Domain.DTOs;
using ManufacturingInventory.Domain.Enums;
using ManufacturingInventory.Infrastructure.Model;
using ManufacturingInventory.Infrastructure.Model.Entities;
using ManufacturingInventory.Infrastructure.Model.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManufacturingInventory.Application.UseCases {
    public class MonthlySummaryUseCase : IMonthlyReportUseCase {
        private ManufacturingContext _context;
        private IEntityProvider<PartInstance> _partInstanceProvider;
        private IUnitOfWork _unitOfWork;
        private static double ProductionFactor = .90;
        private static double RnDFactor = .10;

        public MonthlySummaryUseCase(ManufacturingContext context) {
            this._context = context;
            this._partInstanceProvider = new PartInstanceProvider(context);
            this._unitOfWork = new UnitOfWork(context);
        }

        public async Task<MonthlyReportOutput> Execute(MonthlyReportInput input) {
            var dStart = new DateTime(input.StartDate.Year, input.StartDate.Month, input.StartDate.Day, 0, 0, 0, DateTimeKind.Local);
            var dStop = new DateTime(input.StopDate.Year, input.StopDate.Month, input.StopDate.Day, 23, 59, 59, DateTimeKind.Local);
            IEnumerable<PartInstance> partInstances = new List<PartInstance>();
            switch (input.CollectType) {
                case CollectType.OnlyCostReported:
                    partInstances= await this._partInstanceProvider.GetEntityListAsync(instance => instance.CostReported || (instance.IsBubbler && instance.DateInstalled >= dStart && instance.DateInstalled <= dStop && instance.Transactions.Where(e => e.InventoryAction == InventoryAction.RETURNING).Count() == 0));
                    break;
                case CollectType.AllItems:
                    partInstances = await this._partInstanceProvider.GetEntityListAsync();
                    break;
                case CollectType.OnlyCostNotReported:
                    partInstances = await this._partInstanceProvider.GetEntityListAsync(instance => !instance.CostReported);
                    break;
            }
            var monthlyReport = new List<PartSummary>();
            StringBuilder transactionBuffer = new StringBuilder();
            foreach (var partInstance in partInstances) {
                DateTime dateIn;
                var temp=partInstance.Transactions.OrderByDescending(e => e.TimeStamp).FirstOrDefault(e=>e.InventoryAction==InventoryAction.INCOMING);
                dateIn = (temp != null) ? temp.TimeStamp : DateTime.Now;
                var today = DateTime.Now;
                
                var incomingTransactions = from transaction in partInstance.Transactions
                                           where (transaction.TimeStamp >= dStart && transaction.InventoryAction == InventoryAction.INCOMING)
                                           select transaction;

                var outgoingTransactions = from transaction in partInstance.Transactions
                                           where (transaction.TimeStamp >= dStart && transaction.InventoryAction == InventoryAction.OUTGOING)
                                           select transaction;

                var returningTransactions = from transaction in partInstance.Transactions
                                            where (transaction.TimeStamp >= dStart && transaction.InventoryAction == InventoryAction.RETURNING)
                                            select transaction;

                double incomingQtyTotal, incomingQtyRange, outgoingQtyTotal, outgoingQtyRange, currentQty;
                double incomingCostTotal, incomingCostRange, outgoingCostTotal, outgoingCostRange, currentCost;

                if (partInstance.IsBubbler) {
                    incomingQtyTotal = incomingTransactions.Sum(e => e.MeasuredWeight);

                    incomingQtyTotal = incomingTransactions.Sum(e => e.MeasuredWeight);
                    incomingCostTotal = incomingTransactions.Sum(e => e.TotalCost);
                    //incomingCostTotal = incomingTransactions.Sum(e => e.MeasuredWeight * e.UnitCost);

                    incomingQtyRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.MeasuredWeight);
                    incomingCostRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.TotalCost);
                    //incomingCostRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.MeasuredWeight * e.UnitCost);

                    outgoingQtyTotal = outgoingTransactions.Sum(e => e.MeasuredWeight);
                    outgoingCostTotal = outgoingTransactions.Sum(e => e.TotalCost);
                    //outgoingCostTotal = outgoingTransactions.Sum(e => e.MeasuredWeight * e.UnitCost);

                    outgoingQtyRange = outgoingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.MeasuredWeight);
                    outgoingCostRange = outgoingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.MeasuredWeight * e.UnitCost);
                    currentQty = partInstance.BubblerParameter.Measured *partInstance.Quantity;
                    currentCost = partInstance.UnitCost * currentQty;

                    //incomingQtyTotal = incomingTransactions.Sum(e => e.PartInstance.BubblerParameter.Measured);
                    ////incomingCostTotal = incomingTransactions.Sum(e => e.TotalCost);
                    //incomingCostTotal = incomingTransactions.Sum(e => e.PartInstance.BubblerParameter.Measured * e.UnitCost);

                    //incomingQtyRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.PartInstance.BubblerParameter.Measured);
                    ////incomingCostRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.TotalCost);
                    //incomingCostRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.PartInstance.BubblerParameter.Measured * e.UnitCost);

                    //outgoingQtyTotal = outgoingTransactions.Sum(e => e.PartInstance.BubblerParameter.Measured);
                    ////outgoingCostTotal = outgoingTransactions.Sum(e => e.TotalCost);
                    //outgoingCostTotal = outgoingTransactions.Sum(e => e.PartInstance.BubblerParameter.Measured * e.UnitCost);

                    //outgoingQtyRange = outgoingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.PartInstance.BubblerParameter.Measured);
                    //outgoingCostRange = outgoingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.PartInstance.BubblerParameter.Measured * e.UnitCost);
                    //currentQty = partInstance.BubblerParameter.Measured;
                    //currentCost = partInstance.UnitCost*currentQty;
                } else {
                    incomingQtyTotal = incomingTransactions.Sum(e => e.Quantity);
                    incomingCostTotal = incomingTransactions.Sum(e => e.TotalCost);

                    incomingQtyRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.Quantity);
                    incomingCostRange = incomingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.TotalCost);

                    outgoingQtyTotal = outgoingTransactions.Sum(e => e.Quantity);
                    outgoingCostTotal = outgoingTransactions.Sum(e => e.TotalCost);

                    outgoingQtyRange = outgoingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.Quantity);
                    outgoingCostRange = outgoingTransactions.Where(e => e.TimeStamp <= dStop).Sum(e => e.TotalCost);
                    currentQty = partInstance.Quantity;
                    currentCost = partInstance.UnitCost * currentQty;
                }

                double outgoingCost = outgoingCostRange;
                double outgoingQty = outgoingQtyRange;

                PartSummary partSummary = new PartSummary();
                partSummary.PartName = partInstance.Part.Name;
                partSummary.InstanceName = partInstance.Name;
                partSummary.SerialNumber = partInstance.SerialNumber;
                partSummary.BatchNumber = partInstance.BatchNumber;

                partSummary.DateIn = dateIn;
                partSummary.Age = (today - partSummary.DateIn).Days;
                partSummary.Today = today;

                partSummary.StartQuantity = (currentQty - incomingQtyTotal) + outgoingQtyTotal;
                partSummary.StartCost = (currentCost - incomingCostTotal) + outgoingCostTotal;

                partSummary.EndQuantity = (partSummary.StartQuantity + incomingQtyRange) - outgoingQtyRange;
                partSummary.EndCost = (partSummary.StartCost + incomingCostRange) - outgoingCostRange;

                partSummary.IncomingCost = incomingCostRange;
                partSummary.IncomingQuantity = incomingQtyRange;
                
                partSummary.RndOutgoingCost = outgoingCost * MonthlySummaryUseCase.RnDFactor;
                partSummary.RndOutgoingQuantity = outgoingQty * MonthlySummaryUseCase.RnDFactor;

                partSummary.ProductionOutgoingCost = outgoingCost * MonthlySummaryUseCase.ProductionFactor;
                partSummary.ProductionOutgoingQuantity = outgoingQty * MonthlySummaryUseCase.ProductionFactor;

                partSummary.TotalOutgoingCost = outgoingCostRange;
                partSummary.TotalOutgoingQuantity = outgoingQtyRange;

                partSummary.CurrentCost = currentCost;
                partSummary.CurrentQuantity = currentQty;
                monthlyReport.Add(partSummary);

            }
            return new MonthlyReportOutput(monthlyReport, true, "Done");
        }

        public async Task Load() {
            await this._partInstanceProvider.LoadAsync();
        }
    }
}
