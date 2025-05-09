﻿using perenne.Models;
using System.ComponentModel.DataAnnotations;

public class Group : Entity
{
    [Required, MinLength(4), MaxLength(100)]
    public required string Name { get; set; }
    [MinLength(2), MaxLength(500)]
    public string? Description { get; set; }

    public required ChatChannel ChatChannelId { get; set; }
    public required FeedChannel FeedChannelId { get; set; }
}