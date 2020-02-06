﻿using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using DevExpress.Mvvm;
using PrismCommands = Prism.Commands;
using Prism.Events;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ManufacturingInventory.PartsManagment.Internal;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ManufacturingInventory.Infrastructure.Model.Repositories;
using ManufacturingInventory.Infrastructure.Model.Entities;
using ManufacturingInventory.Common.Application;
using Condition = ManufacturingInventory.Infrastructure.Model.Entities.Condition;
using ManufacturingInventory.Application.Boundaries.PartInstanceDetailsEdit;
using ManufacturingInventory.Domain.Enums;

namespace ManufacturingInventory.PartsManagment.ViewModels {

    public enum MessageType {
        ERROR,
        WARNING,
        INFORMATION
    }

    public class PartInstanceDetailsViewModel : InventoryViewModelNavigationBase {

        protected IMessageBoxService MessageBoxService { get => ServiceContainer.GetService<IMessageBoxService>("PartInstanceDetailsMessageService"); }
        protected IDispatcherService DispatcherService { get => ServiceContainer.GetService<IDispatcherService>("PartInstanceDetailDispatcher"); }

        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;
        private IPartInstanceDetailsEditUseCase _editInstance;

        private bool _isEdit = false;
        private bool _isNew = false;
        private bool _isBubbler = false;
        private bool _isInitialized = false;
        private bool _hasPrice;
        private bool _editInProgress = false;
        private int _partId;

        private ObservableCollection<Condition> _conditions;
        private ObservableCollection<Location> _locations;
        private ObservableCollection<PartType> _partTypes;
        private ObservableCollection<Transaction> _transactions;
        private ObservableCollection<Attachment> _attachments;

        private PartInstance _selectedPartInstance;
        private Location _selectedLocation;
        private Condition _selectedCondition;
        private PartType _selectedPartType;
        private AttachmentDataTraveler _instanceAttachmentTraveler;
        private PriceDataTraveler _priceDataTraveler;
        private int _selectedTabIndex;
        private bool _tableLoading;
        private double _measuredWeight;
        private double _weight;
        private double _grossWeight;
        private double _netWeight;
        private double _unitCost;
        private double _totalCost;
        private int _quantity;


        public AsyncCommand InitializeCommand { get; private set; }
        public AsyncCommand SaveCommand { get; private set; }
        public AsyncCommand CancelCommand { get; private set; }

        public AsyncCommand NewPriceCommand { get; private set; }
        public AsyncCommand SelectPriceCommand { get; private set; }
        public AsyncCommand EditPriceCommand { get; private set; }

        public PartInstanceDetailsViewModel(IPartInstanceDetailsEditUseCase editInstance, IEventAggregator eventAggregator,IRegionManager regionManager) {
            this._editInstance = editInstance;
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this.InitializeCommand = new AsyncCommand(this.InitializedHandler);
            this.SaveCommand = new AsyncCommand(this.SaveHandler,this.CanSave);
            this.CancelCommand = new AsyncCommand(this.DiscardHandler);
        }

        public override bool KeepAlive => false;

        public ObservableCollection<Condition> Conditions { 
            get => this._conditions;
            set => SetProperty(ref this._conditions, value);
        }

        public ObservableCollection<Location> Locations { 
            get => this._locations;
            set => SetProperty(ref this._locations, value);
        }

        public ObservableCollection<PartType> PartTypes {
            get => this._partTypes;
            set => SetProperty(ref this._partTypes, value);
        }

        public AttachmentDataTraveler AttachmentDataTraveler {
            get => this._instanceAttachmentTraveler;
            set => SetProperty(ref this._instanceAttachmentTraveler, value);
        }

        public PartInstance SelectedPartInstance { 
            get => this._selectedPartInstance;
            set => SetProperty(ref this._selectedPartInstance, value);
        }

        public Location SelectedLocation { 
            get => this._selectedLocation;
            set => SetProperty(ref this._selectedLocation, value);
        }

        public PartType SelectedPartType { 
            get => this._selectedPartType;
            set => SetProperty(ref this._selectedPartType, value);
        }

        public Condition SelectedCondition {
            get => this._selectedCondition;
            set => SetProperty(ref this._selectedCondition, value);
        }

        public bool IsBubbler {
            get => this._isBubbler;
            set => SetProperty(ref this._isBubbler, value);
        }

        public bool IsEdit {
            get => this._isEdit;
            set => SetProperty(ref this._isEdit, value);
        }

        public double Weight {
            get => this._weight;
            set => this.SetProperty(ref this._weight, value);
        }

        public double Measured {
            get => this._measuredWeight;
            set {
                if (this.SelectedPartInstance != null) {
                    this.SelectedPartInstance.UpdateWeight(value);
                    this.Weight = this.SelectedPartInstance.BubblerParameter.Weight;
                }
                this.SetProperty(ref this._measuredWeight, value);
            }
        }

        public double GrossWeight {
            get => this._grossWeight;
            set {
                if (this.IsBubbler) {
                    this.SelectedPartInstance.BubblerParameter.GrossWeight = value;
                    this.SelectedPartInstance.UpdateWeight();
                    this.Weight = this.SelectedPartInstance.BubblerParameter.Measured;
                    this.SelectedPartInstance.UpdatePrice();
                }
                SetProperty(ref this._grossWeight, value);
            }
        }

        public double NetWeight {
            get => this._netWeight;
            set {
                if (this.IsBubbler) {
                    this.SelectedPartInstance.BubblerParameter.NetWeight = value;
                    this.SelectedPartInstance.UpdateWeight();
                    this.Weight = this.SelectedPartInstance.BubblerParameter.Weight;
                    this.TotalCost = this.SelectedPartInstance.TotalCost;
                }
                SetProperty(ref this._netWeight, value);
            }
        }

        public double UnitCost {
            get => this._unitCost;
            set {
                this.SelectedPartInstance.Price.UnitCost = value;
                this.SelectedPartInstance.UpdatePrice();
                this.TotalCost = this.SelectedPartInstance.TotalCost;
                SetProperty(ref this._unitCost, value);
            }
        }

        public double TotalCost {
            get => this._totalCost;
            set => SetProperty(ref this._totalCost, value);
        }

        public int Quantity {
            get => this._quantity;
            set {
                if (!this.IsBubbler) {
                    this.TotalCost = this.SelectedPartInstance.UnitCost * value;
                }
                SetProperty(ref this._quantity, value);
            }
        }

        public ObservableCollection<Transaction> Transactions { 
            get => this._transactions;
            set => this.SetProperty(ref this._transactions, value);
        }

        public ObservableCollection<Attachment> Attachments { 
            get => this._attachments;
            set => this.SetProperty(ref this._attachments, value);
        }

        public AttachmentDataTraveler InstanceAttachmentTraveler {
            get => this._instanceAttachmentTraveler;
            set => SetProperty(ref this._instanceAttachmentTraveler, value);
        }

        public bool TableLoading { 
            get => this._tableLoading;
            set => SetProperty(ref this._tableLoading, value);
        }

        public PriceDataTraveler PriceDataTraveler { 
            get => this._priceDataTraveler;
            set => SetProperty(ref this._priceDataTraveler, value);
        }

        public int SelectedTabIndex { 
            get => this._selectedTabIndex; 
            set => SetProperty(ref this._selectedTabIndex,value); 
        }

        public async Task InitializedHandler() {
            if (!this._isInitialized) {

                var categories = await this._editInstance.GetCategories();
                var locations = await this._editInstance.GetLocations();
                var transactions = await this._editInstance.GetTransactions(this.SelectedPartInstance.Id);
                
                this.DispatcherService.BeginInvoke(() => {
                    this.Conditions = new ObservableCollection<Condition>(categories.OfType<Condition>());
                    this.PartTypes = new ObservableCollection<PartType>(categories.OfType<PartType>());
                    this.Locations = new ObservableCollection<Location>(locations);
                    this.Transactions = new ObservableCollection<Transaction>(transactions);

                    if (this.SelectedPartInstance != null) {
                        if (this._isNew) {



                        } else {
                            this.AttachmentDataTraveler = new AttachmentDataTraveler(GetAttachmentBy.PARTINSTANCE, this.SelectedPartInstance.Id);
                            if (this.SelectedPartInstance.Condition != null) {
                                this.SelectedCondition = this.Conditions.FirstOrDefault(e => e.Id == this.SelectedPartInstance.ConditionId);
                            }

                            if (this.SelectedPartInstance.CurrentLocation != null) {
                                this.SelectedLocation = this.Locations.FirstOrDefault(e => e.Id == this.SelectedPartInstance.LocationId);
                            }

                            if (this.SelectedPartInstance.PartType != null) {
                                this.SelectedPartType = this.PartTypes.FirstOrDefault(e => e.Id == this.SelectedPartInstance.PartTypeId);
                            }
                        }


                    }
                    this._isInitialized = true;
                });
            }
        }

        public async Task SaveHandler() {

            if (this.SelectedPartType != null) {
                this.SelectedPartInstance.PartTypeId = this.SelectedPartType.Id;
            }

            if (this.SelectedCondition != null) {
                this.SelectedPartInstance.ConditionId = this.SelectedCondition.Id;
            }

            this.SelectedPartInstance.LocationId = this.SelectedLocation.Id;

            PartInstanceDetailsEditInput input = new PartInstanceDetailsEditInput(this.SelectedPartInstance, false);
            var output = await this._editInstance.Execute(input);
            if (output.Success) {
                this.DispatcherService.BeginInvoke(() => {
                    this.MessageBoxService.ShowMessage(output.Message+Environment.NewLine+"Reloading", "Saved", MessageButton.OK, MessageIcon.Information);
                });
                ReloadEventTraveler traveler = new ReloadEventTraveler() {
                    PartId = this.SelectedPartInstance.PartId,
                    PartInstanceId = this.SelectedPartInstance.Id
                };
                this._eventAggregator.GetEvent<ReloadEvent>().Publish(traveler);
                this.IsEdit = false;
            } else {
                this.DispatcherService.BeginInvoke(() => {
                    this.MessageBoxService.ShowMessage("Error Save Part Instance", "Error", MessageButton.OK, MessageIcon.Error);
                });
            }
        }

        public async Task DiscardHandler() {
            await Task.Run(() => {
                ReloadEventTraveler traveler = new ReloadEventTraveler() {
                    PartId = this.SelectedPartInstance.PartId,
                    PartInstanceId = this.SelectedPartInstance.Id
                };
                this.DispatcherService.BeginInvoke(() => {
                    this.MessageBoxService.ShowMessage("","",MessageButton.OK,MessageIcon.Error); 
                    this._eventAggregator.GetEvent<ReloadEvent>().Publish(traveler);
                    this.IsEdit = false;
                });

            });

        }

        private Task NewPriceHandler() {
            this._editInProgress = true;
            //this.CleanupRegions();
            NavigationParameters param = new NavigationParameters();
            param.Add(ParameterKeys.PriceId, this.SelectedPartInstance.PriceId);
            param.Add(ParameterKeys.InstanceId, this.SelectedPartInstance.Id);
            param.Add(ParameterKeys.PartId, this.SelectedPartInstance.PartId);
            param.Add(ParameterKeys.IsEdit, false);
            param.Add(ParameterKeys.IsNew, true);
            this._regionManager.RequestNavigate(LocalRegions.InstancePriceEditDetailsRegion, ModuleViews.PriceDetailsView, param);
            return Task.CompletedTask;
        }

        private Task SelectPriceHandler() {
            this._editInProgress = true;
            //this.CleanupRegions();
            NavigationParameters param = new NavigationParameters();
            param.Add(ParameterKeys.PriceId, this.SelectedPartInstance.PriceId);
            param.Add(ParameterKeys.InstanceId, this.SelectedPartInstance.Id);
            param.Add(ParameterKeys.PartId, this.SelectedPartInstance.PartId);
            this._regionManager.RequestNavigate(LocalRegions.InstancePriceEditDetailsRegion, ModuleViews.SelectPriceView, param);
            return Task.CompletedTask;
        }

        private Task EditPriceHandler() {
            this._editInProgress = true;
            //this.CleanupRegions();
            NavigationParameters param = new NavigationParameters();
            param.Add(ParameterKeys.PriceId, this.SelectedPartInstance.PriceId);
            param.Add(ParameterKeys.InstanceId, this.SelectedPartInstance.Id);
            param.Add(ParameterKeys.PartId, this.SelectedPartInstance.PartId);
            param.Add(ParameterKeys.IsEdit, true);
            param.Add(ParameterKeys.IsNew, false);
            this._regionManager.RequestNavigate(LocalRegions.InstancePriceEditDetailsRegion, ModuleViews.PriceDetailsView, param);
            return Task.CompletedTask;
        }

        public bool CanSave() {
            return this.SelectedLocation != null;
        }

        public MessageResult ShowMessage(MessageType messageType,string message) {
            switch (messageType) {
                case MessageType.ERROR: {
                    return this.MessageBoxService.ShowMessage(message, "Error", MessageButton.OK, MessageIcon.Error);
                }
                case MessageType.WARNING: {
                    return this.MessageBoxService.ShowMessage(message, "Warning", MessageButton.OK, MessageIcon.Warning);
                }
                case MessageType.INFORMATION: {
                    return this.MessageBoxService.ShowMessage(message, "Error", MessageButton.OK, MessageIcon.Information);
                }
                default: return MessageResult.None;
            }
        }

        public void NavigatePrice(bool hasPrice) {
            if (hasPrice) {
                NavigationParameters param = new NavigationParameters();
                param.Add(ParameterKeys.PriceId, this.SelectedPartInstance.PriceId);
                param.Add(ParameterKeys.InstanceId, this.SelectedPartInstance.Id);
                param.Add(ParameterKeys.PartId, this.SelectedPartInstance.PartId);
                param.Add(ParameterKeys.IsEdit, false);
                param.Add(ParameterKeys.IsNew, false);
                this._regionManager.RequestNavigate(LocalRegions.InstancePriceEditDetailsRegion, ModuleViews.PriceDetailsView, param);
            }
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
            //var partInstance = navigationContext.Parameters[ParameterKeys.SelectedPartInstance] as PartInstance;
            //if (partInstance is PartInstance) {
            //    return this.SelectedPartInstance != null && this.SelectedPartInstance.Id != partInstance.Id;
            //} else {
            //    return true;
            //}
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext) {
        }

        public override void OnNavigatedTo(NavigationContext navigationContext) {
            this._isInitialized = false;
            this._isNew = Convert.ToBoolean(navigationContext.Parameters[ParameterKeys.IsNew]);
            this.IsBubbler= Convert.ToBoolean(navigationContext.Parameters[ParameterKeys.IsBubbler]);
            this._partId = Convert.ToInt32(navigationContext.Parameters[ParameterKeys.PartId]);
            if (this._isNew) {
                if (this.IsBubbler) {
                    this.GrossWeight = 0;
                    this.Measured = 0;
                    this.NetWeight = 0;
                }
            } else {
                var partInstance = navigationContext.Parameters[ParameterKeys.SelectedPartInstance] as PartInstance;
                this.IsEdit = (bool)navigationContext.Parameters[ParameterKeys.IsEdit];
                if (partInstance is PartInstance) {
                    this.SelectedPartInstance = partInstance;
                    if (this.IsBubbler) {
                        this.GrossWeight = this.SelectedPartInstance.BubblerParameter.GrossWeight;
                        this.Measured = this.SelectedPartInstance.BubblerParameter.Measured;
                        this.NetWeight = this.SelectedPartInstance.BubblerParameter.NetWeight;
                    }
                    if (this.SelectedPartInstance.Price != null) {
                        this.UnitCost = this.SelectedPartInstance.UnitCost;
                        this.TotalCost = this.SelectedPartInstance.TotalCost;
                        this.NavigatePrice(true);

                    } else {
                        this.UnitCost = 0;
                        this.TotalCost = 0;
                        this.NavigatePrice(false);
                    }
                }
            }
        }
    }
}
