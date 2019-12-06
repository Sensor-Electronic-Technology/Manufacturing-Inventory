﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManufacturingInventory.Common.Model.Entities {
    public class Session {
        public int Id { get; set; }
        public DateTime In { get; set; }
        public DateTime? Out { get; set; }

        public int? UserId { get; set; }
        public virtual User User { get; set; }

        public byte[] RowVersion { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public Session() {
            this.Transactions = new ObservableHashSet<Transaction>();
        }

        public Session(User user) : this() {
            this.In = DateTime.Now;
            this.User = user;
        }
    }
}