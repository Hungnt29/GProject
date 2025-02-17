﻿using GProject.Data.DomainClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace GProject.WebApplication.Models
{
    public class BrandDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string SearchCount { get; set; }
        public int Status { get; set; } = 1; // 0: sử dụng || 1: không sử dụng
        public string? Description { get; set; }
        public List<Brand>? BrandList { get; set; }
    }
}
