﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManufacturingInventory.Common.Model.Entities {
    public class Part {
        private readonly ILazyLoader _lazyLoader;
        private ObservableHashSet<PartInstance> _partInstances;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HoldsBubblers { get; set; }
        public byte[] RowVersion { get; set; }

        public int? OgranizationId { get; set; }
        public virtual Organization Organization { get; set; }

        public int? WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int? UsageId { get; set; }
        public Usage Usage { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
        public ObservableHashSet<PartInstance> PartInstances {
            get => this._lazyLoader.Load(this, ref this._partInstances);
            set => this._partInstances = value;
        }
        public ICollection<PartManufacturer> PartManufacturers { get; set; }

        public Part(ILazyLoader lazyLoader) {
            this._lazyLoader = lazyLoader;
            //this.Attachments = new ObservableHashSet<Attachment>();
            //this.PartInstances = new ObservableHashSet<PartInstance>();
            //this.PartManufacturers = new ObservableHashSet<PartManufacturer>();
        }

        //public Part() {
        //    this.Attachments = new ObservableHashSet<Attachment>();
        //    this.PartInstances = new ObservableHashSet<PartInstance>();
        //    this.PartManufacturers = new ObservableHashSet<PartManufacturer>();
        //}

        public Part(string name, string description, bool holdsBubblers, Organization organization, Warehouse warehouse, Usage usage) {
            this.Name = name;
            this.Description = description;
            this.HoldsBubblers = holdsBubblers;
            this.Organization = organization;
            this.Warehouse = warehouse;
            this.Usage = usage;
        }
    }

    //public class Part {
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public bool HoldsBubblers { get; set; }
    //    public byte[] RowVersion { get; set; }

    //    public int? OgranizationId { get; set; }
    //    public virtual Organization Organization { get; set; }

    //    public int? WarehouseId { get; set; }
    //    public Warehouse Warehouse { get; set; }

    //    public int? UsageId { get; set; }
    //    public Usage Usage { get; set; }

    //    public ICollection<Attachment> Attachments { get; set; }
    //    public ICollection<PartInstance> PartInstances { get; set; }
    //    public ICollection<PartManufacturer> PartManufacturers { get; set; }

    //    //public Part(ILazyLoader lazyLoader) {
    //    //    this._lazyLoader = lazyLoader;
    //    //    this.Attachments = new ObservableHashSet<Attachment>();
    //    //    this.PartInstances = new ObservableHashSet<PartInstance>();
    //    //    this.PartManufacturers = new ObservableHashSet<PartManufacturer>();
    //    //}

    //    public Part() {
    //        this.Attachments = new ObservableHashSet<Attachment>();
    //        this.PartInstances = new ObservableHashSet<PartInstance>();
    //        this.PartManufacturers = new ObservableHashSet<PartManufacturer>();
    //    }

    //    public Part(string name, string description, bool holdsBubblers, Organization organization, Warehouse warehouse, Usage usage):this() {
    //        this.Name = name;
    //        this.Description = description;
    //        this.HoldsBubblers = holdsBubblers;
    //        this.Organization = organization;
    //        this.Warehouse = warehouse;
    //        this.Usage = usage;
    //    }
    //}
}
