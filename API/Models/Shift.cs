﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class Shift
{
    /// <summary>
    /// Id in database, if already exists
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    
    internal ObjectId? Id { get; set; }
    /// <summary>
    /// Start time of the shift
    /// </summary>
    public DateTime Start { get; set; }
    /// <summary>
    /// End time of the shift
    /// </summary>
    public DateTime End { get; set; }
    /// <summary>
    /// Address of the home where the shift is taking place
    /// </summary>
    public string Location { get; set; }
    /// <summary>
    /// Role of the employee working the shift
    /// </summary>
    public string Role { get; set; }
}