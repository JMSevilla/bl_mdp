using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;
namespace WTW.MdpService.Infrastructure.MemberDb;

public static class DatabaseCommands
{
    public static async Task<T> ExecuteFunction<T>(
        this DatabaseFacade db,
        string name,
        IEnumerable<OracleParameter> parameters,
        OracleParameter returnParameter)
    {
        if (returnParameter.Direction == ParameterDirection.ReturnValue)
            throw new InvalidOperationException("returnParameter with returnValue direction should not be set");

        // Oracle managed driver issue: it's a must to modify direction using property, not constructor
        returnParameter.Direction = ParameterDirection.ReturnValue;

        using var command = (OracleCommand)CreateFunctionCommand(db, name);
        parameters.ToList().ForEach(p => command.Parameters.Add(p));
        command.Parameters.Add(returnParameter);
        command.BindByName = true;

        if (command.Connection.State != ConnectionState.Open)
            await command.Connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
        return (T)returnParameter.Value;
    }

    public static async Task ExecuteSql(
        this DatabaseFacade db,
        string sql,
        IEnumerable<OracleParameter> parameters)
    {
        using var command = (OracleCommand)CreateTextCommand(db, sql);
        parameters.ToList().ForEach(p => command.Parameters.Add(p));

        if (command.Connection.State != ConnectionState.Open)
            await command.Connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<List<T>> ExecuteQuery<T>(
        this DatabaseFacade db,
        string query,
        Func<DbDataReader, T> map,
        params object[] parameters)
    {
        using var command = CreateQueryCommand(db, query, parameters);
        if (command.Connection.State != ConnectionState.Open)
            await command.Connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        var result = new List<T>();
        while (reader.Read())
            result.Add(map(reader));

        return result;
    }

    private static DbCommand CreateFunctionCommand(DatabaseFacade db, string text, params object[] parameters)
    {
        var command = db.GetDbConnection().CreateCommand();
        command.CommandText = text;
        command.CommandType = CommandType.StoredProcedure;

        if (db.CurrentTransaction != null)
            command.Transaction = db.CurrentTransaction.GetDbTransaction();

        return command;
    }

    private static DbCommand CreateTextCommand(DatabaseFacade db, string text, params object[] parameters)
    {
        var command = db.GetDbConnection().CreateCommand();
        command.CommandText = text;
        command.CommandType = CommandType.Text;

        if (db.CurrentTransaction != null)
            command.Transaction = db.CurrentTransaction.GetDbTransaction();

        return command;
    }

    private static DbCommand CreateQueryCommand(DatabaseFacade db, string text, params object[] parameters)
    {
        var command = db.GetDbConnection().CreateCommand();
        command.CommandText = text;
        command.Parameters.AddRange(parameters);

        if (db.CurrentTransaction != null)
            command.Transaction = db.CurrentTransaction.GetDbTransaction();

        return command;
    }
}

