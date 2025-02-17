﻿using API.Constants;
using API.Errors;
using API.Models.QueryOptions;
using API.Models.Scheduling.Coverage;
using API.Models.Shifts;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Services.QueryExecuters;

public interface IShiftQueryExecuter
{
    List<Shift> GetShifts(ShiftQueryOptions options);
    Shift GetShift(ObjectId id);
    List<ObjectId> GetOpenShiftIDs(ShiftQueryOptions options);
    List<Shift> GetOpenShifts(ShiftQueryOptions options);
}
public class ShiftQueryExecuter(ICollectionsProvider provider) : IShiftQueryExecuter
{
    ICollectionsProvider _collectionsProvider = provider;
    private FilterDefinition<Shift> BuildFilter(IOpenShiftQueryOptions options, in FilterDefinitionBuilder<Shift> builder)
    {
        FilterDefinition<Shift> filter = builder.Empty;
        // Shift start should be gte the shift start of filter
        if (options.TimeFilter != null)
        {
            filter = filter & builder.Gte(shift => shift.ShiftPeriod.Start, options.TimeFilter.Start) & builder.Lte(shift => shift.ShiftPeriod.Start, options.TimeFilter.End);
        }
        else
        {
            filter = filter & builder.Gte(shift => shift.ShiftPeriod.Start, DateTime.Now);

        }
        return filter;

    }
    private FilterDefinition<Shift> BuildFilter(IShiftQueryOptions options, in FilterDefinitionBuilder<Shift> builder)
    {
        FilterDefinition<Shift> filter = builder.Empty;
        if (options.EmployeeIDFilter != null)
        {
            filter = filter & builder.Eq(shift => shift.EmployeeID, options.EmployeeIDFilter);
        }
        if (options.RequiredRoleFilter != null)
        {
            filter = filter & builder.Eq(shift => shift.RequiredRole, options.RequiredRoleFilter);
        }
        filter = filter & BuildFilter((IOpenShiftQueryOptions)options, builder);
        return filter;

    }
    public List<ObjectId> GetOpenShiftIDs(ShiftQueryOptions options)
    {

        return GetOpenShifts(options).Select(shift => shift.Id ?? throw new Exception("ID should never be null when reading from database.")).ToList();
    }
    public List<Shift> GetOpenShifts(ShiftQueryOptions options)
    {
        var openToPickupShiftIDs = _collectionsProvider.CoverageRequests
            .Find(req => req.CoverageType != CoverageOptions.TradeOnly)
            .ToList()
            .Select(req => (ObjectId?)req.ShiftID).ToList();
        var builder = Builders<Shift>.Filter;
        var filter = options == null ? builder.Empty : BuildFilter(options, builder);
        filter = filter & (builder.Where(shift => shift.EmployeeID == null) | builder.In(shift => shift.Id, openToPickupShiftIDs));
        var unassignedShifts = _collectionsProvider.Shifts.Find(filter).ToList();
        return unassignedShifts.ToList();
    }

    public List<Shift> GetShifts(ShiftQueryOptions options)
    {
        var filter = BuildFilter(options, Builders<Shift>.Filter);
        return _collectionsProvider.Shifts.Find(filter).ToList();
    }

    public Shift GetShift(ObjectId id)
    {
        var result = _collectionsProvider.Shifts.Find(shift => shift.Id == id).FirstOrDefault() ?? throw new DCCApiException(ErrorConstants.ObjectDoesNotExistError);
        return result;
    }
}
