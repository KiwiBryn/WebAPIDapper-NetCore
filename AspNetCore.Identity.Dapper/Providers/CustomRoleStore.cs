using Microsoft.AspNetCore.Identity;
using System;
using Dapper;
using System.Threading.Tasks;
using System.Threading;
using devMobile.AspNetCore.Identity.Dapper.Models;
using System.Data.Common;
using Microsoft.AspNetCore.Connections;
using System.Data.SqlClient;
using System.Security.Claims;

namespace devMobile.AspNetCore.Identity.Dapper.CustomProvider
{
    public class CustomRoleStore : IRoleStore<ApplicationRole>
    {
        private readonly SqlConnection _connection;

        public CustomRoleStore(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            const string sql = @"
                INSERT INTO [dbo].[AspNetRoles]
                VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp);
            ";
            var rowsInserted = await _connection.ExecuteAsync(sql, new
            {
                role.Id,
                role.Name,
                role.NormalizedName,
                role.ConcurrencyStamp
            });
            //return rowsInserted == 1;
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            const string sql = @"
                DELETE
                FROM [dbo].[AspNetRoles]
                WHERE [Id] = @Id;
            ";
            var rowsDeleted = await _connection.ExecuteAsync(sql, new { Id = role.Id });
            //return rowsDeleted == 1;
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT *
                FROM [dbo].[AspNetRoles]
                WHERE [Id] = @Id;
            ";
            var role = await _connection.QuerySingleOrDefaultAsync<ApplicationRole>(sql, new { Id = roleId });

            return role;
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT *
                FROM [dbo].[AspNetRoles]
                WHERE [NormalizedName] = @NormalizedName;
            ";
            var role = await _connection.QuerySingleOrDefaultAsync<ApplicationRole>(sql, new { NormalizedName = normalizedRoleName });

            return role;
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            const string updateRoleSql = @"
                UPDATE [dbo].[AspNetRoles]
                SET [Name] = @Name, [NormalizedName] = @NormalizedName, [ConcurrencyStamp] = @ConcurrencyStamp
                WHERE [Id] = @Id;
            ";

            using (var transaction = _connection.BeginTransaction())
            {
                await _connection.ExecuteAsync(updateRoleSql, new
                {
                    role.Name,
                    role.NormalizedName,
                    role.ConcurrencyStamp,
                    role.Id
                }, transaction);
                /*
                if (claims?.Count() > 0)
                {
                    const string deleteClaimsSql = @"
                        DELETE
                        FROM [dbo].[AspNetRoleClaims]
                        WHERE [RoleId] = @RoleId;
                    ";
                    await DbConnection.ExecuteAsync(deleteClaimsSql, new
                    {
                        RoleId = role.Id
                    }, transaction);
                    const string insertClaimsSql = @"
                        INSERT INTO [dbo].[AspNetRoleClaims] (RoleId, ClaimType, ClaimValue)
                        VALUES (@RoleId, @ClaimType, @ClaimValue);
                    ";
                    await DbConnection.ExecuteAsync(insertClaimsSql, claims.Select(x => new {
                        RoleId = role.Id,
                        x.ClaimType,
                        x.ClaimValue
                    }), transaction);
                }
                */
                try
                {
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return IdentityResult.Failed();
                }
            }
            return IdentityResult.Success;
        }
    }
}
