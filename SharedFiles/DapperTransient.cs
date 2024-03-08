//---------------------------------------------------------------------------------
// Copyright (c) June 2021, devMobile Software
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
//	The polly project looks like the best option, similar approach to the Microsoft Enterprise libraries
//		http://www.thepollyproject.org/ (mid June 2021 https was borked)
//		https://github.com/App-vNext/Polly
//
// I still have projects which used the Microsoft Enterprise libraries in production sow as inspired by their "transient" faults list.
//		https://github.com/microsoftarchive/transient-fault-handling-application-block/blob/master/source/Source/TransientFaultHandling.Data/SqlDatabaseTransientErrorDetectionStrategy.cs
//
// Like other authors I started with the EF list of transient faults (essentially a copy n paste)
//		https://raw.githubusercontent.com/aspnet/EntityFrameworkCore/master/src/EFCore.SqlServer/Storage/Internal/SqlServerTransientExceptionDetector.cs
//
//	These Microsoft documents provided more inspiration
//		https://docs.microsoft.com/en-us/dotnet/architecture/microservices/
//		https://docs.microsoft.com/en-us/azure/azure-sql/database/troubleshoot-common-errors-issues
//		https://docs.microsoft.com/en-us/sql/connect/ado-net/step-4-connect-resiliently-sql-ado-net
//
// Ben Hyrman's articles also provided more inspiration
//		https://hyr.mn/dapper-and-polly
//		
//		Different error codes and exception processing done by authors. Need to spend some quality time reviewing
//		Did think about retry for async methods which returned object/dynamic but figured would stick to strongly typed ones
//		Had problems with syntax for BeginTransactionAsync etc. so no retry versions currently
//		May make the number of retries and intervals configurable but don't need that for V1
//---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Polly;
using Polly.Retry;


namespace devMobile.Azure.DapperTransient
{
   public static class DapperExtensions
   {
      private const int RetryCount = 4;

      private static readonly RetryPolicy RetryPolicy = Policy
          .Handle<SqlException>(DapperExtensions.ShouldRetryOn)
          .Or<TimeoutException>()
          .WaitAndRetry(RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

      private static readonly AsyncRetryPolicy RetryPolicyAsnyc = Policy
          .Handle<SqlException>(DapperExtensions.ShouldRetryOn)
          .Or<TimeoutException>()
          .WaitAndRetryAsync(RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));


      public static void OpenWithRetry(this SqlConnection connection) => RetryPolicy.Execute(() => connection.Open());

      public static Task OpenWithRetryAsync(this SqlConnection connection) => RetryPolicyAsnyc.ExecuteAsync(() => connection.OpenAsync());


      public static void CloseWithRetry(this SqlConnection connection) => RetryPolicy.Execute(() => connection.Close());

      public static Task CloseWithRetryAsync(this SqlConnection connection) => RetryPolicyAsnyc.ExecuteAsync(() => connection.CloseAsync());


      public static DataTable GetSchemaWithRetry(this SqlConnection connection) => RetryPolicy.Execute(() => connection.GetSchema());

      public static Task<DataTable> GetSchemaWithRetryAsync(this SqlConnection connection) => RetryPolicyAsnyc.ExecuteAsync(() => connection.GetSchemaAsync());


      public static int ExecuteWithRetry(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicy.Execute(() => connection.Execute(command));

      public static Task<int> ExecuteWithRetryAsync(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteAsync(command));


      public static int ExecuteWithRetry(
           this IDbConnection connection,
           string sql,
           object param = null,
           IDbTransaction transaction = null,
           int? commandTimeout = null,
           CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Execute(sql, param, transaction, commandTimeout, commandType));

      public static Task<int> ExecuteWithRetryAsync(
           this IDbConnection connection,
           string sql,
           object param = null,
           IDbTransaction transaction = null,
           int? commandTimeout = null,
           CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));


      public static T ExecuteScalarWithRetry<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType));

      public static Task<T> ExecuteScalarWithRetryAsync<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType));


      public static IEnumerable<dynamic> QueryWithRetry(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          bool buffered = true, //
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query(sql, param, transaction, buffered, commandTimeout, commandType));

      public static Task<IEnumerable<dynamic>> QueryWithRetryAsync(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(sql, param, transaction, commandTimeout, commandType));


      public static IEnumerable<T> QueryWithRetry<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          bool buffered = true,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType));

      public static Task<IEnumerable<T>> QueryWithRetryAsync<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TReturn> map,
         string splitOn = "Id") => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TReturn>(sql, map, splitOn));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TReturn>(
         this IDbConnection connection,
         CommandDefinition command,
         Func<TFirst, TSecond, TReturn> map,
         string splitOn = "Id") => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TReturn>(command, map, splitOn));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TThird, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TReturn> map,
         string splitOn = "Id") => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, splitOn));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TReturn>(
         this IDbConnection connection,
         CommandDefinition command,
         Func<TFirst, TSecond, TThird, TReturn> map,
         string splitOn = "Id") => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TThird, TReturn>(command, map, splitOn));


      public static IEnumerable<object> QueryWithRetry(
         this IDbConnection connection,
         Type type,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query(type, sql, param, transaction, buffered, commandTimeout, commandType));
      public static Task<IEnumerable<object>> QueryWithRetryAsync(
         this IDbConnection connection,
         Type type,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(type, sql, param, transaction, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
         string splitOn = "Id") => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, splitOn));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
         this IDbConnection connection,
         CommandDefinition command,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
         string splitOn = "Id") => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(command, map, splitOn));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
         string splitOn = "Id") => RetryPolicy.Execute(() => connection.Query(sql, map, splitOn));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
         this IDbConnection connection,
         CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
         string splitOn = "Id") => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(command, map, splitOn));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
         this IDbConnection connection,
         string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
         this IDbConnection connection,
         string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
         object param = null,
         IDbTransaction transaction = null,
         bool buffered = true,
         string splitOn = "Id",
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
         string splitOn = "Id") => RetryPolicy.Execute(() => connection.Query(sql, map, splitOn));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
         this IDbConnection connection,
         CommandDefinition command,
         Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
         string splitOn = "Id") => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(command, map, splitOn));


      public static IEnumerable<TReturn> QueryWithRetry<TFirst, TSecond, TThird, TFourth, TReturn>(
         this IDbConnection connection,
         string sql,
         Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
         string splitOn = "Id") => RetryPolicy.Execute(() => connection.Query(sql, map, splitOn));

      public static Task<IEnumerable<TReturn>> QueryWithRetryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
         this IDbConnection connection,
         CommandDefinition command,
         Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
         string splitOn = "Id") => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(command, map, splitOn));


      public static IEnumerable<dynamic> QueryWithRetry(
         this IDbConnection connection,
         string sql) => RetryPolicy.Execute(() => connection.Query(sql));

      public static Task<IEnumerable<dynamic>> QueryWithRetryAsync(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryAsync(command));


      public static T QueryFirstWithRetry<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QueryFirst<T>(sql, param, transaction, commandTimeout, commandType));

      public static Task<T> QueryFirstWithRetryAsync<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryFirstAsync<T>(sql, param, transaction, commandTimeout, commandType));


      public static IDataReader ExecuteReaderWithRetry(
         this IDbConnection connection,
         CommandDefinition command,
         CommandBehavior commandBehavior) => RetryPolicy.Execute(() => connection.ExecuteReader(command, commandBehavior));

      public static Task<IDataReader> ExecuteReaderWithRetryAsync(
         this IDbConnection connection,
         CommandDefinition command,
         CommandBehavior commandBehavior) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteReaderAsync(command, commandBehavior));


      public static DbDataReader ExecuteReaderWithRetry(
         this DbConnection connection,
         CommandDefinition command) => (DbDataReader)RetryPolicy.Execute(() => connection.ExecuteReader(command));

      // TODO check cast on sync version 
      public static Task<DbDataReader> ExecuteReaderWithRetryAsync(
         this DbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteReaderAsync(command));


      public static IDataReader ExecuteReaderWithRetry(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicy.Execute(() => connection.ExecuteReader(command));

      public static Task<IDataReader> ExecuteReaderWithRetryAsync(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteReaderAsync(command));


      public static IDataReader ExecuteReaderWithRetry(
            this IDbConnection connection,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null) => RetryPolicy.Execute(() => connection.ExecuteReader(sql, param, transaction, commandTimeout, commandType));

      public static Task<IDataReader> ExecuteReaderWithRetryAsync(
            this IDbConnection connection,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.ExecuteReaderAsync(sql, param, transaction, commandTimeout, commandType));


      public static T QueryFirstOrDefaultWithRetry<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType));

      public static Task<T> QueryFirstOrDefaultWithRetryAsync<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType));


      public static T QuerySingleWithRetry<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingle<T>(sql, param, transaction, commandTimeout, commandType));

      public static Task<T> QuerySingleWithRetryAsync<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync<T>(sql, param, transaction, commandTimeout, commandType));


      public static T QuerySingleOrDefaultWithRetry<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingleOrDefault<T>(sql, param, transaction, commandTimeout, commandType));

      public static Task<T> QuerySingleOrDefaultWithRetryAsync<T>(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType));


      public static SqlMapper.GridReader QueryMultipleWithRetry(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QueryMultiple(sql, param, transaction, commandTimeout, commandType));

      public static Task<SqlMapper.GridReader> QueryMultipleWithRetryAsync(
          this IDbConnection connection,
          string sql,
          object param = null,
          IDbTransaction transaction = null,
          int? commandTimeout = null,
          CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QueryMultipleAsync(sql, param, transaction, commandTimeout, commandType));


      public static dynamic QuerySingleWithRetry(
         this IDbConnection connection,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingle(sql, param, transaction, commandTimeout, commandType));

      public static Task<dynamic> QuerySingleWithRetryAsync(
         this IDbConnection connection,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync(sql, param, transaction, commandTimeout, commandType));


      public static object QuerySingleWithRetry(
               this IDbConnection connection,
               Type type,
               string sql,
               object param = null,
               IDbTransaction transaction = null,
               int? commandTimeout = null,
               CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingle(type, sql, param, transaction, commandTimeout, commandType));

      public static Task<object> QuerySingleWithRetryAsync(
         this IDbConnection connection,
         Type type,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync(type, sql, param, transaction, commandTimeout, commandType));


      public static dynamic QuerySingleWithRetryWithRetry<T>(
         this IDbConnection connection,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingle(sql, param, transaction, commandTimeout, commandType));

      public static Task<dynamic> QuerySingleWithRetryWithRetryAsync<T>(
         this IDbConnection connection,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync(sql, param, transaction, commandTimeout, commandType));


      public static object QuerySingleWithRetry(
         this IDbConnection connection,
         Type type,
         string sql) => RetryPolicy.Execute(() => connection.QuerySingle(type, sql));

      public static Task<object> QuerySingleWithRetryAsync(
         this IDbConnection connection,
         Type type,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync(type, command));


      public static T QuerySingleWithRetry<T>(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicy.Execute(() => connection.QuerySingle<T>(command));

      public static Task<T> QuerySingleWithRetryAsync<T>(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync<T>(command));


      public static dynamic QuerySingleWithRetry(
         this IDbConnection connection,
         string sql) => RetryPolicy.Execute(() => connection.QuerySingle(sql));

      public static Task<dynamic> QuerySingleWithRetryAsync(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleAsync(command));


      public static object QuerySingleOrDefaultWithRetry(
         this IDbConnection connection,
         Type type,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingleOrDefault(type, sql, param, transaction, commandTimeout, commandType));

      public static Task<object> QuerySingleOrDefaultWithRetryAsync(
         this IDbConnection connection,
         Type type,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleOrDefaultAsync(type, sql, param, transaction, commandTimeout, commandType));


      public static object QuerySingleOrDefaultWithRetry(
         this IDbConnection connection,
         Type type,
         string sql) => RetryPolicy.Execute(() => connection.QuerySingleOrDefault(type, sql));

      public static Task<object> QuerySingleOrDefaultWithRetryAsync(
         this IDbConnection connection,
         Type type,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleOrDefaultAsync(type, command));


      public static dynamic QuerySingleOrDefaultWithRetry(
         this IDbConnection connection,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicy.Execute(() => connection.QuerySingleOrDefault(sql, param, transaction, commandTimeout, commandType));

      public static Task<dynamic> QuerySingleOrDefaultWithRetryAsync(
         this IDbConnection connection,
         string sql,
         object param = null,
         IDbTransaction transaction = null,
         int? commandTimeout = null,
         CommandType? commandType = null) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));


      public static dynamic QuerySingleOrDefaultWithRetry(
         this IDbConnection connection,
         string sql) => RetryPolicy.Execute(() => connection.QuerySingleOrDefault(sql));

      public static Task<dynamic> QuerySingleOrDefaultWithRetryAsync(
         this IDbConnection connection,
         CommandDefinition command) => RetryPolicyAsnyc.ExecuteAsync(() => connection.QuerySingleOrDefaultAsync(command));


      public static IEnumerable<dynamic> ReadWithRetry(
         this SqlMapper.GridReader gridReader,
         bool buffered = true) => RetryPolicy.Execute(() => gridReader.Read(buffered));

      public static Task<IEnumerable<dynamic>> ReadWithRetryAsync(
         this SqlMapper.GridReader gridReader,
         bool buffered = true) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadAsync(buffered));


      public static IEnumerable<object> ReadWithRetry(
         this SqlMapper.GridReader gridReader,
         Type type,
         bool buffered = true) => RetryPolicy.Execute(() => gridReader.Read(type, buffered));

      public static Task<IEnumerable<object>> ReadWithRetryAsync(
         this SqlMapper.GridReader gridReader,
         Type type,
         bool buffered = true) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadAsync(type, buffered));


      public static IEnumerable<T> ReadWithRetry<T>(
         this SqlMapper.GridReader gridReader,
         bool buffered = true) => RetryPolicy.Execute(() => gridReader.Read<T>(buffered));

      public static Task<IEnumerable<T>> ReadWithRetryAsync<T>(
         this SqlMapper.GridReader gridReader,
         bool buffered = true) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadAsync<T>(buffered));


      public static T ReadFirstWithRetry<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadFirst<T>());

      public static Task<T> ReadFirstWithRetryAsync<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadFirstAsync<T>());


      public static object ReadFirstWithRetry(
         this SqlMapper.GridReader gridReader,
         Type type) => RetryPolicy.Execute(() => gridReader.ReadFirst(type));

      public static Task<object> ReadFirstWithRetryAsync(
         this SqlMapper.GridReader gridReader,
         Type type) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadFirstAsync(type));


      public static dynamic ReadFirstWithRetry(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadFirstAsync());

      public static Task<dynamic> ReadFirstWithRetryAsync(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadFirstAsync());


      public static T ReadFirstOrDefaultWithRetry<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadFirstOrDefault<T>());

      public static Task<T> ReadFirstOrDefaultWithRetryAsync<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadFirstOrDefaultAsync<T>());


      public static dynamic ReadFirstOrDefaultWithRetry(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadFirstOrDefault<dynamic>());

      public static Task<dynamic> ReadFirstOrDefaultWithRetryAsync(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadFirstOrDefaultAsync<dynamic>());


      public static object ReadFirstOrDefaultWithRetry(
            this SqlMapper.GridReader gridReader,
            Type type) => RetryPolicy.Execute(() => gridReader.ReadFirstOrDefault(type));

      public static Task<object> ReadFirstOrDefaultWithRetryAsync(
            this SqlMapper.GridReader gridReader,
            Type type) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadFirstOrDefaultAsync(type));


      public static T ReadSingleWithRetry<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadSingle<T>());

      public static Task<T> ReadSingleWithRetryAsync<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadSingleAsync<T>());


      public static dynamic ReadSingleWithRetry(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadSingle<dynamic>());

      public static Task<dynamic> ReadSingleWithRetryAsync(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadSingleAsync<dynamic>());


      public static object ReadSingleWithRetry(
         this SqlMapper.GridReader gridReader,
         Type type) => RetryPolicy.Execute(() => gridReader.ReadSingle(type));

      public static Task<object> ReadSingleWithRetryAsync(
         this SqlMapper.GridReader gridReader,
         Type type) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadSingleAsync(type));


      public static object ReadSingleOrDefaultWithRetry(
         this SqlMapper.GridReader gridReader,
         Type type) => RetryPolicy.Execute(() => gridReader.ReadSingleOrDefault(type));

      public static Task<object> ReadSingleOrDefaultWithRetryAsync(
         this SqlMapper.GridReader gridReader,
         Type type) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadSingleOrDefaultAsync(type));


      public static dynamic ReadSingleOrDefaultWithRetry(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadSingleOrDefault<dynamic>());

      public static Task<dynamic> ReadSingleOrDefaultWithRetryAsync(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadSingleOrDefaultAsync<dynamic>());


      public static T ReadSingleOrDefaultWithRetry<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicy.Execute(() => gridReader.ReadSingleOrDefault<T>());

      public static Task<T> ReadSingleOrDefaultWithRetryAsync<T>(
         this SqlMapper.GridReader gridReader) => RetryPolicyAsnyc.ExecuteAsync(() => gridReader.ReadSingleOrDefaultAsync<T>());


      /// <summary>
      ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
      ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
      ///     any release. You should only use it directly in your code with extreme caution and knowing that
      ///     doing so can result in application failures when updating to a new Entity Framework Core release.
      /// </summary>
      private static bool ShouldRetryOn(Exception ex)
      {
         if (ex is SqlException sqlException)
         {
            foreach (SqlError err in sqlException.Errors)
            {
               switch (err.Number)
               {
                  // SQL Error Code: 49920
                  // Cannot process request. Too many operations in progress for subscription "%ld".
                  // The service is busy processing multiple requests for this subscription.
                  // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for operation status.
                  // Wait until pending requests are complete or delete one of your pending requests and retry your request later.
                  case 49920:
                  // SQL Error Code: 49919
                  // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
                  // The service is busy processing multiple create or update requests for your subscription or server.
                  // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for pending operations.
                  // Wait till pending create or update requests are complete or delete one of your pending requests and
                  // retry your request later.
                  case 49919:
                  // SQL Error Code: 49918
                  // Cannot process request. Not enough resources to process request.
                  // The service is currently busy.Please retry the request later.
                  case 49918:
                  // SQL Error Code: 41839
                  // Transaction exceeded the maximum number of commit dependencies.
                  case 41839:
                  // SQL Error Code: 41325
                  // The current transaction failed to commit due to a serializable validation failure.
                  case 41325:
                  // SQL Error Code: 41305
                  // The current transaction failed to commit due to a repeatable read validation failure.
                  case 41305:
                  // SQL Error Code: 41302
                  // The current transaction attempted to update a record that has been updated since the transaction started.
                  case 41302:
                  // SQL Error Code: 41301
                  // Dependency failure: a dependency was taken on another transaction that later failed to commit.
                  case 41301:
                  // SQL Error Code: 40613
                  // Database XXXX on server YYYY is not currently available. Please retry the connection later.
                  // If the problem persists, contact customer support, and provide them the session tracing ID of ZZZZZ.
                  case 40613:
                  // SQL Error Code: 40501
                  // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
                  case 40501:
                  // SQL Error Code: 40197
                  // The service has encountered an error processing your request. Please try again.
                  case 40197:
                  // SQL Error Code: 11001
                  // A connection attempt failed
                  case 11001:
                  // SQL Error Code: 10936
                  // Resource ID : %d. The request limit for the elastic pool is %d and has been reached.
                  // See 'http://go.microsoft.com/fwlink/?LinkId=267637' for assistance.
                  case 10936:
                  // SQL Error Code: 10929
                  // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
                  // However, the server is currently too busy to support requests greater than %d for this database.
                  // For more information, see http://go.microsoft.com/fwlink/?LinkId=267637. Otherwise, please try again.
                  case 10929:
                  // SQL Error Code: 10928
                  // Resource ID: %d. The %s limit for the database is %d and has been reached. For more information,
                  // see http://go.microsoft.com/fwlink/?LinkId=267637.
                  case 10928:
                  // SQL Error Code: 10060
                  // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
                  // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
                  // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed
                  // because the connected party did not properly respond after a period of time, or established connection failed
                  // because connected host has failed to respond.)"}
                  case 10060:
                  // SQL Error Code: 10054
                  // A transport-level error has occurred when sending the request to the server.
                  // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
                  case 10054:
                  // SQL Error Code: 10053
                  // A transport-level error has occurred when receiving results from the server.
                  // An established connection was aborted by the software in your host machine.
                  case 10053:
                  // SQL Error Code: 1205
                  // Deadlock
                  case 1205:
                  // SQL Error Code: 233
                  // The client was unable to establish a connection because of an error during connection initialization process before login.
                  // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server;
                  // the server was too busy to accept new connections; or there was a resource limitation (insufficient memory or maximum
                  // allowed connections) on the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by
                  // the remote host.)
                  case 233:
                  // SQL Error Code: 121
                  // The semaphore timeout period has expired
                  case 121:
                  // SQL Error Code: 64
                  // A connection was successfully established with the server, but then an error occurred during the login process.
                  // (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
                  case 64:
                  // DBNETLIB Error Code: 20
                  // The instance of SQL Server you attempted to connect to does not support encryption.
                  case 20:
                     return true;
                     // This exception can be thrown even if the operation completed successfully, so it's safer to let the application fail.
                     // DBNETLIB Error Code: -2
                     // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding. The statement has been terminated.
                     //case -2:
               }
            }

            return false;
         }

         return ex is TimeoutException;
      }
   }
}