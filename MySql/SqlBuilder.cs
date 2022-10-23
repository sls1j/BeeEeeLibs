using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeEeeLibs.Mysql
{
  /// <summary>
  /// Defines sql builder helper functions
  /// </summary>
  public class SqlBuilder
  {
    /// <summary>
    /// Generates an insert statement reflected from the object type.
    /// </summary>
    /// <typeparam name="T">The type to reflect to build the insert statement from</typeparam>
    /// <param name="table">The table to insert into</param>
    /// <param name="exclude">The fields to ignore while making the insert statement</param>
    /// <returns>The SQL insert statement</returns>
    public static string GenerateInsertSqlFromObject<T>(string table, params string[] exclude)
    {
      StringBuilder sb = new StringBuilder();

      Type t = typeof(T);

      var columns = t.GetProperties().Select(f => f.Name).Where(x => !exclude.Contains(x));

      sb.Append("INSERT INTO ")
        .Append(table)
        .Append(" (")
        .Append(string.Join(",", columns))
        .Append(") values (")
        .Append(string.Join(",", columns.Select(c => $"@{c}")))
        .AppendLine(");")
        .AppendLine("SELECT LAST_INSERT_ID();");

      return sb.ToString();
    }

    /// <summary>
    /// Create an update statement by relfecting the given object type.
    /// </summary>
    /// <typeparam name="T">The object type to reflect</typeparam>
    /// <param name="table">The table to insert into</param>
    /// <param name="exclude">Fields to ignore from the object</param>
    /// <returns>The generated SQL update statement</returns>
    public static string GenerateUpdateSqlFromObject<T>(string table, params string[] exclude)
    {
      StringBuilder sb = new StringBuilder();

      Type t = typeof(T);

      var fields = t.GetFields().Select(f => f.Name);
      var properties = t.GetProperties().Select(f => f.Name);
      var columns = fields.Union(properties).Where(x => !exclude.Contains(x));
      var sets = columns.Where(c => c != "Id").Select(c => $"{c}=@{c}");

      sb.Append("UPDATE ")
        .Append(table)
        .Append(" SET ")
        .Append(string.Join(",", sets))
        .Append(" WHERE Id=@Id");

      return sb.ToString();
    }

    /// <summary>
    /// Create a DELETE by identifier statment
    /// </summary>
    /// <param name="table">The table to delete from</param>
    /// <param name="idField">The field that holds the identifier</param>
    /// <returns>The generated SQL DELETE statement</returns>
    public static string GenerateDeleteById(string table, string idField)
    {
      return $"DELETE FROM {table} WHERE {idField}=@{idField};";
    }       
    
    /// <summary>
    /// Create a select by identifiers SQL statement
    /// </summary>
    /// <param name="table">The table to retrive records from</param>
    /// <param name="idField">The identifier id</param>
    /// <param name="ids">The list of identifiers to test for</param>
    /// <returns>The generated SQL SELECT statement</returns>
    public static string GenerateSelectByIds(string table, string idField, int[] ids)
    {
      return $"SELECT * FROM {table} WHERE {idField} IN ({string.Join(",", ids)});";
    }
  }
}
