﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ManufacturingInventory.Application.UseCases;
using ManufacturingInventory.Domain.DTOs;

namespace ManufacturingInventory.Application.Boundaries.AlertManagmentUseCase {
    public interface IAlertsAvailableUseCase:IUseCase<AlertsAvailableInput,AlertsAvailableOutput> {
        Task Load();
        Task<IEnumerable<AlertDto>> GetAvailableAlerts(int userId);    
    }
}
