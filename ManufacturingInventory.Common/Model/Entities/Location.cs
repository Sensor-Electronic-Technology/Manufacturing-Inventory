﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManufacturingInventory.Common.Model.Entities {
    public abstract class Location {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] RowVersion { get; set; }
        public virtual ICollection<PartInstance> ItemsAtLocation { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        public Location() {
            this.ItemsAtLocation = new ObservableHashSet<PartInstance>();
            this.Transactions = new ObservableHashSet<Transaction>();
        }

        public void Set(Location location) {
            this.Name = location.Name;
            this.Description = location.Description;
        }
    }

    public class Warehouse : Location {
        public ICollection<Part> StoredParts { get; set; }
        //public ICollection<IncomingTransaction> IncomingTransactions { get; set; }

        public Warehouse() {
            this.StoredParts = new ObservableHashSet<Part>();
            //this.IncomingTransactions = new ObservableHashSet<IncomingTransaction>();
        }
    }

    public class Consumer : Location {
        //public ICollection<OutgoingTransaction> OutgoingTransactions { get; set; }

        public Consumer() {
            //this.OutgoingTransactions = new ObservableHashSet<OutgoingTransaction>();
        }
    }
}
