﻿using API.Constants;
using API.Models.QueryOptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models;

/// <summary>
/// The shift object we receive from the client. Restricts assignment of properties we dont want assigned.
/// </summary>
public class ShiftExternal
{
    public TimeRange ShiftPeriod { get; set; }
    public string Location { get; set; }
    public Role Role { get; set; }
    public string? EmployeeID { get; set; }
}
public class Shift : MongoObject
{
    public Shift() { }
    public Shift(ShiftExternal external)
    {
        ShiftPeriod = external.ShiftPeriod;
        Location = external.Location;
        Role = external.Role;
        EmployeeID = string.IsNullOrEmpty(external.EmployeeID) ? null : ObjectId.Parse(external.EmployeeID);
    }
    private const double MAX_SHIFT_LENGTH_HRS = 16;// According to derron. May need updated.
    private TimeRange _shiftPeriod;
    /// <summary>
    /// When the shift is taking place
    /// </summary>
    public TimeRange ShiftPeriod
    {
        get { return _shiftPeriod; }
        set
        {
            if (value.Duration().TotalHours < 0)
            {
                throw new ArgumentException($"A shift may not start after it ends.");

            }
            if (value.Duration().TotalHours > MAX_SHIFT_LENGTH_HRS)
            {
                throw new ArgumentException($"Shift length is too long, please be sure shifts are less than {MAX_SHIFT_LENGTH_HRS} hours");
            }
            _shiftPeriod = value;
        }
    }

    /// <summary>
    /// Address of the home where the shift is taking place
    /// </summary>
    [Length(0,70)]
    public string Location { get; set; }
    /// <summary>
    /// Role of the employee working the shift
    /// </summary>
    public Role Role { get; set; }
    /// <summary>
    /// Employee assigned to the shift, null or empty if none.
    /// </summary>
    [BsonIgnoreIfNull]
    public ObjectId? EmployeeID { get; set; }
}