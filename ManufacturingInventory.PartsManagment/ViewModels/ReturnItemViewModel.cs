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
using ManufacturingInventory.Application.Boundaries.ReturnItem;
using ManufacturingInventory.Common.Application.UI.Services;

namespace ManufacturingInventory.PartsManagment.ViewModels {
    public class ReturnItemViewModel : InventoryViewModelNavigationBase {

        protected IMessageBoxService MessageBoxService { get => ServiceContainer.GetService<IMessageBoxService>("ReturnItemMessageBoxService"); }
        protected IDispatcherService DispatcherService { get => ServiceContainer.GetService<IDispatcherService>("ReturnItemDispatcherService"); }

        private IReturnItemUseCase _returnItem;
        private IEventAggregator _eventAggregator;

        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<Condition> _conditions;
        private Condition _selectedCondition;
        private Warehouse _selectedWarehouse;

        private Transaction _selectedTransaction;
        private PartInstance _transactionPartInstance;

        private DateTime _timeStamp;
        private double _measuredWeight;
        private double _weight;
        private int _quantity;
        private bool _isBubbler = false;
        private bool _isInitialized = false;

        public AsyncCommand InitializeCommand { get; private set; }

        public ReturnItemViewModel(IReturnItemUseCase returnItem,IEventAggregator eventAggregator) {
            this._returnItem = returnItem;
            this._eventAggregator = eventAggregator;
            this.InitializeCommand = new AsyncCommand(this.LoadHandler);
        } 


        public override bool KeepAlive => false;

        public ObservableCollection<Warehouse> Warehouses { 
            get => this._warehouses;
            set => SetProperty(ref this._warehouses, value);
        }

        public ObservableCollection<Condition> Conditions { 
            get => this._conditions;
            set => SetProperty(ref this._conditions, value);
        }

        public Condition SelectedCondition { 
            get => this._selectedCondition;
            set => SetProperty(ref this._selectedCondition, value);
        }

        public Warehouse SelectedWarehouse { 
            get => this._selectedWarehouse;
            set => SetProperty(ref this._selectedWarehouse, value);
        }

        public DateTime TimeStamp { 
            get => this._timeStamp;
            set => SetProperty(ref this._timeStamp, value);
        }

        public double MeasuredWeight {
            get => this._measuredWeight;
            set => SetProperty(ref this._measuredWeight, value);
        }

        public double Weight { 
            get => this._weight;
            set => SetProperty(ref this._weight, value);
        }

        public int Quantity { 
            get => this._quantity;
            set => SetProperty(ref this._quantity, value);
        }

        public bool IsBubbler { 
            get => this._isBubbler;
            set => SetProperty(ref this._isBubbler, value);
        }

        public bool IsInitialized { 
            get => this._isInitialized;
            set => SetProperty(ref this._isInitialized, value);
        }

        public Transaction SelectedTransaction { 
            get => this._selectedTransaction;
            set => SetProperty(ref this._selectedTransaction, value);
        }

        public PartInstance TransactionPartInstance { 
            get => this._transactionPartInstance;
            set => SetProperty(ref this._transactionPartInstance, value);
        }

        private async Task LoadHandler() {
            if (!this._isInitialized) {
                var warehouses = await this._returnItem.GetWarehouses();
                var conditions = await this._returnItem.GetConditions();
                //var partInstance = await this._returnItem.GetPartInstance(this.SelectedTransaction.PartInstanceId);
                var wareHouseId = await this._returnItem.GetPartWarehouseId(this.SelectedTransaction.PartInstance.PartId);
                this.DispatcherService.BeginInvoke(() => {
                    this.Warehouses = new ObservableCollection<Warehouse>(warehouses);
                    this.Conditions = new ObservableCollection<Condition>(conditions);
                    this.Quantity = this.SelectedTransaction.Quantity;
                    //this.TransactionPartInstance = partInstance;
                    this.SelectedWarehouse = this.Warehouses.FirstOrDefault(e => e.Id == wareHouseId);
                    this._isInitialized = true;
                });
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext) {
            var transaction = navigationContext.Parameters[ParameterKeys.SelectedTransaction] as Transaction;
            if(transaction is Transaction) {
                this.SelectedTransaction = transaction;
            }
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext) {

        }


    }
}
