﻿using Domain.Models;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ICategoryRepo
    {
        Task<Category> GetCategory(uint id);
    }
}