//---------------------------------------------------------------------------------
// Copyright (c) June 2022, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
namespace devMobile.Azure.Dapper
{
   using System.Data;
   using System.Data.SqlClient;

   using Microsoft.Extensions.Configuration;

   public interface IDapperContext 
   {
      public IDbConnection ConnectionCreate();
      public IDbConnection ConnectionCreate(string connectionStringName);

      public IDbConnection ConnectionReadCreate();
      public IDbConnection ConnectionWriteCreate();
   }

   public class DapperContext : IDapperContext
   {
      private readonly IConfiguration _configuration;

      public DapperContext(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      public IDbConnection ConnectionCreate()
      { 
         return new SqlConnection(_configuration.GetConnectionString("default"));
      }

      public IDbConnection ConnectionCreate(string connectionStringName)
      {
         return new SqlConnection(_configuration.GetConnectionString(connectionStringName));
      }

      public IDbConnection ConnectionReadCreate()
      {
         return new SqlConnection(_configuration.GetConnectionString("default-read"));
      }

      public IDbConnection ConnectionWriteCreate()
      {
         return new SqlConnection(_configuration.GetConnectionString("default-write"));
      }
   }
}