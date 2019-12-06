﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManufacturingInventory.Common.Model.Entities {
    public class Price {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime? VaildFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsCurrent { get; set; }
        public double Amount { get; set; }
        public int MinOrder { get; set; }
        public double LeadTime { get; set; }
        public byte[] RowVersion { get; set; }

        public int? DistributorId { get; set; }
        public virtual Distributor Distributor { get; set; }

        public int? PartInstanceId { get; set; }
        public virtual PartInstance PartInstance { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }

        public Price() {
            this.Attachments = new ObservableHashSet<Attachment>();
        }
    }
}