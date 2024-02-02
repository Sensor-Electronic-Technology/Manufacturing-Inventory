using DevExpress.Mvvm;
using ManufacturingInventory.Application.Boundaries.ReportingBoundaries;
using ManufacturingInventory.Common.Application;
using ManufacturingInventory.Common.Application.UI.Services;
using ManufacturingInventory.Infrastructure.Model.Entities;
using ManufacturingInventory.Domain.Extensions;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ManufacturingInventory.Domain.DTOs;
using ManufacturingInventory.Domain.Enums;

namespace ManufacturingInventory.Reporting.ViewModels {
    public class ReportingMonthlySummaryViewModel : InventoryViewModelBase {
        protected IDispatcherService DispatcherService { get => ServiceContainer.GetService<IDispatcherService>("MonthlySummaryDispatcherService"); }
        protected IMessageBoxService MessageBoxService { get => ServiceContainer.GetService<IMessageBoxService>("MonthlySummaryMessageBoxService"); }
        protected IExportService ExportService { get => ServiceContainer.GetService<IExportService>("MonthlySummaryExportService"); }

        private IMonthlyReportUseCase _reportingService;
        private bool _isLoaded = false;

        private ObservableCollection<PartSummary> _reportSnapshot;
        private CollectType _selectedCollectionType;
        private DateTime _start;
        private DateTime _stop;
        private decimal _prodPercent=new Decimal(90.0);
        private decimal _rndPercent=new Decimal(10.0);
        private bool _showTableLoading;

        public AsyncCommand<ExportFormat> ExportTableCommand { get; private set; }
        public AsyncCommand CollectSnapshotCommand { get; private set; }
        public AsyncCommand InitializeCommand { get; private set;}

        public ReportingMonthlySummaryViewModel(IMonthlyReportUseCase reportingService) {
            this._reportingService = reportingService;
            this.CollectSnapshotCommand = new AsyncCommand(this.CollectSummaryHandler);
            this.ExportTableCommand = new AsyncCommand<ExportFormat>(this.ExportTableHandler);
            this.InitializeCommand = new AsyncCommand(this.LoadAsync);
        }

        public override bool KeepAlive => true;
        
        public ObservableCollection<PartSummary> ReportSnapshot {
            get => this._reportSnapshot;
            set => SetProperty(ref this._reportSnapshot, value);
        }

        public DateTime Start {
            get => this._start;
            set => SetProperty(ref this._start, value);
        }

        public DateTime Stop {
            get => this._stop;
            set => SetProperty(ref this._stop, value);
        }
        public decimal ProductionPercent {
            get => this._prodPercent;
            set=>SetProperty(ref this._prodPercent, value);
        }

        public decimal ResearchPercent {
            get => this._rndPercent;
            set =>SetProperty(ref this._rndPercent, value);
            
        }

        public bool ShowTableLoading {
            get => this._showTableLoading;
            set => SetProperty(ref this._showTableLoading, value);
        }

        public CollectType SelectedCollectionType {
            get => this._selectedCollectionType;
            set => SetProperty(ref this._selectedCollectionType, value);
        }

        /*private async Task CollectSummaryHandler() {
            MonthlyReportInput reportInput = new MonthlyReportInput(this._start, this._stop,this._selectedCollectionType);
            this.DispatcherService.BeginInvoke(() => this.ShowTableLoading=true);
            var output=await this._reportingService.Execute(reportInput);
            if (output.Success) {
                this.ReportSnapshot = new ObservableCollection<PartSummary>(output.MonthlyReport);            
            } else {
                this.MessageBoxService.ShowMessage(output.Message,"",MessageButton.OK,MessageIcon.Error);
            }
            this.DispatcherService.BeginInvoke(() => this.ShowTableLoading = false);
        }*/
        
        private async Task CollectSummaryHandler() {
            if ((this._prodPercent + this._rndPercent)==100) {
                MonthlyReportInput reportInput = new MonthlyReportInput(this._start, this._stop,
                    this._selectedCollectionType,(double)this._prodPercent,(double)this._rndPercent);
                this.DispatcherService.BeginInvoke(() => this.ShowTableLoading=true);
                var output=await this._reportingService.Execute(reportInput);
                if (output.Success) {
                    this.ReportSnapshot = new ObservableCollection<PartSummary>(output.MonthlyReport);            
                } else {
                    this.MessageBoxService.ShowMessage(output.Message,"",MessageButton.OK,MessageIcon.Error);
                }
                
            } else {
                this.MessageBoxService.ShowMessage("Production and Research percents" +
                                                   " don't add up to 100","",MessageButton.OK, MessageIcon.Error);
            }
            this.DispatcherService.BeginInvoke(() => this.ShowTableLoading = false);
        }

        private async Task ExportTableHandler(ExportFormat format) {
            await Task.Run(() => {
                this.DispatcherService.BeginInvoke(() => {
                    var path = Path.ChangeExtension(Path.GetTempFileName(), format.ToString().ToLower());
                    using (FileStream file = File.Create(path)) {
                        this.ExportService.Export(file, format);
                    }
                    using (var process = new Process()) {
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.FileName = path;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();
                    }
                });
            });
        }

        private async Task LoadAsync() {
            if (!this._isLoaded) {
                this.DispatcherService.BeginInvoke(() => this.ShowTableLoading = true);
                await this._reportingService.Load();
                this.SelectedCollectionType = CollectType.OnlyCostReported;
                var now = DateTime.Now;
                this.Stop = now;
                this.Start = now;
                this._isLoaded = true;
                this.DispatcherService.BeginInvoke(() => this.ShowTableLoading = false);
            }
        }
    }
}
