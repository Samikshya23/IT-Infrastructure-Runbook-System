using Dapper;
using EmployeeAccessSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Repositories
{
    public class ProductSetupRepositories : IProductSetupRepositories
    {
        private readonly string _connectionString;

        public ProductSetupRepositories(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Get all records
        public async Task<IEnumerable<ProductSetup>> GetAllAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETALL");

                return await conn.QueryAsync<ProductSetup>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading records.", ex);
            }
        }

        // Get active records
        public async Task<IEnumerable<ProductSetup>> GetActiveAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETACTIVE");

                return await conn.QueryAsync<ProductSetup>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading active records.", ex);
            }
        }

        // Get record by id
        public async Task<ProductSetup> GetByIdAsync(int id)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETBYID");
                parameters.Add("ProductId", id);

                return await conn.QueryFirstOrDefaultAsync<ProductSetup>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading record.", ex);
            }
        }

        // Save new record
        public async Task<int> AddAsync(ProductSetup productSetup)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "ADD");
                parameters.Add("ProductName", productSetup.ProductName);
                parameters.Add("IsActive", productSetup.IsActive);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving record.", ex);
            }
        }

        // Update existing record
        public async Task<int> UpdateAsync(ProductSetup productSetup)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "UPDATE");
                parameters.Add("ProductId", productSetup.ProductId);
                parameters.Add("ProductName", productSetup.ProductName);
                parameters.Add("IsActive", productSetup.IsActive);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating record.", ex);
            }
        }

        // Delete record
        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETE");
                parameters.Add("ProductId", id);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting record.", ex);
            }
        }

        // Activate or deactivate record
        public async Task<int> ToggleAsync(int id)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "TOGGLE");
                parameters.Add("ProductId", id);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductSetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating status.", ex);
            }
        }
    }
}