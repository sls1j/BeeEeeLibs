using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeEeeLibs.Mysql
{
  /// <summary>
  /// Builds a select statement
  /// </summary>
  public class SelectBuilder
  {
    private List<string> Fields;
    private string MainTable;
    private string MainAlias;
    private List<Join> Joins;

    /// <summary>
    /// The where clause
    /// </summary>
    public string WhereClause;

    /// <summary>
    /// The keys to order by
    /// </summary>
    public List<(string key, bool isAscending)> OrderKeys;

    /// <summary>
    /// The number of records to skip
    /// </summary>
    public int? LimitOffset;

    /// <summary>
    /// The maximum number of records to return
    /// </summary>
    public int? LimitCount;

    /// <summary>
    /// Create a SelectBuilder and set the table and alias
    /// </summary>
    /// <param name="table"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    public static SelectBuilder Start(string table, string alias = null)
    {
      return new SelectBuilder().SetTable(table, alias);
    }

    /// <summary>
    /// The default constructor
    /// </summary>
    public SelectBuilder()
    {
      Fields = new List<string>();
      Joins = new List<Join>();
      WhereClause = null;
      OrderKeys = new List<(string key, bool isAscending)>();
      LimitOffset = null;
      LimitCount = null;
    }

    /// <summary>
    /// Sets the table and alias of the select builder
    /// </summary>
    /// <param name="table">The main table name to build a select statement for</param>
    /// <param name="alias">The alais to use is specified</param>
    /// <returns>The select builder</returns>
    /// <exception cref="InvalidOperationException">If this is called twice</exception>
    public SelectBuilder SetTable(string table, string alias = null)
    {
      if (string.IsNullOrEmpty(MainTable))
      {
        MainTable = table;
        MainAlias = alias;
      }
      else
        throw new InvalidOperationException("Can only set the main table once");

      return this;
    }

    /// <summary>
    /// Add fields by reflection of the given type
    /// </summary>
    /// <typeparam name="T">The type to reflect</typeparam>
    /// <param name="tableAlias">The alias of the table that the object pertains to.</param>
    /// <returns>The select builder</returns>
    public SelectBuilder AddObjectFields<T>(string tableAlias = null)
    {
      Type t = typeof(T);

      Func<string, string, string> makeField = (alias, name) => string.IsNullOrEmpty(alias) ? name : $"{tableAlias}.{name}";

      Func<Type, bool> isSupported = tt =>
       {
         if (tt.IsPrimitive)
           return true;

         if (Nullable.GetUnderlyingType(tt) != null)
         {
           return true;
         }

         if (tt == typeof(string) || tt == typeof(DateTime))
         {
           return true;
         }

         return false;
       };

      foreach (var field in t.GetFields())
      {
        if (isSupported(field.FieldType))
        {
          string sqlField = makeField(tableAlias, field.Name);
          Fields.Add(sqlField);
        }
      }

      foreach (var property in t.GetProperties())
      {
        if (isSupported(property.PropertyType))
        {
          string sqlField = makeField(tableAlias, property.Name);
          Fields.Add(sqlField);
        }
      }

      return this;
    }

    /// <summary>
    /// Removes the given fields
    /// </summary>
    /// <param name="fields">The fields to remove</param>
    /// <returns>The select builder</returns>
    /// <exception cref="InvalidOperationException">If no fields are passed in</exception>
    public SelectBuilder RemoveFields(params string[] fields)
    {
      if (fields == null || fields.Length == 0)
      {
        throw new InvalidOperationException("Must have at least one field");
      }

      Fields.RemoveAll(f => fields.Contains(f));
      return this;
    }

    /// <summary>
    /// Adds fields to the select statement
    /// </summary>
    /// <param name="fields">The fields to add</param>
    /// <returns>The select builder</returns>
    /// <exception cref="InvalidOperationException">If no fields are in the fields array.</exception>
    public SelectBuilder AddFields(params string[] fields)
    {
      if (fields == null || fields.Length == 0)
      {
        throw new InvalidOperationException("Must have at least one field");
      }

      Fields.AddRange(fields);
      return this;
    }

    /// <summary>
    /// Add to the from statement
    /// </summary>
    /// <param name="table">The table to add</param>
    /// <param name="alias">The alias for the table</param>
    /// <param name="onClause">The on clause to use</param>
    /// <param name="type">The type of join</param>
    /// <returns>The select builder</returns>
    public SelectBuilder AddFrom(string table, string alias, string onClause, JoinType type)
    {
      Join j = new Join()
      {
        Alias = alias,
        OnClause = onClause,
        Table = table,
        Type = type
      };

      Joins.Add(j);
      return this;
    }

    /// <summary>
    /// Set's the where cluase
    /// </summary>
    /// <param name="clause">The where clause</param>
    /// <returns>The select builder</returns>
    public SelectBuilder SetWhere(string clause)
    {
      WhereClause = clause;
      return this;
    }

    /// <summary>
    /// Add an order by key whether ascending or descending.
    /// </summary>
    /// <param name="key">The key to sort on</param>
    /// <param name="isAscending">If true then sort ascending, otherwise sort descending</param>
    /// <returns></returns>
    public SelectBuilder AddOrderKey(string key, bool isAscending = true)
    {
      OrderKeys.Add((key, isAscending));
      return this;
    }

    /// <summary>
    /// Set the paging limits
    /// </summary>
    /// <param name="count">The maximum number of records to return</param>
    /// <param name="offset">The number of records to skip</param>
    /// <returns></returns>
    public SelectBuilder SetLimit(int count, int? offset = null)
    {
      LimitCount = count;
      LimitOffset = offset;
      return this;
    }

    /// <summary>
    /// Converts the select builder to valid SQL
    /// </summary>
    /// <returns>The sql</returns>
    /// <exception cref="InvalidOperationException">If there is not main table set.</exception>
    public string ToSql()
    {
      if (string.IsNullOrEmpty(MainTable))
        throw new InvalidOperationException("Must set the main table first");

      StringBuilder sb = new StringBuilder();
      sb.Append("SELECT ");
      if (Fields.Count == 0)
      {
        sb.AppendLine("*");
      }
      else
      {
        sb.AppendLine(string.Join(",", Fields));
      }

      sb.AppendLine("FROM");

      // first table
      sb.AppendLine($"{MainTable} {MainAlias}");

      foreach (var join in Joins)
      {
        sb.Append("  ");
        sb.Append(join.Type.ToString().ToUpper());
        sb.Append(" JOIN ");
        sb.Append($"{join.Table} {join.Alias}");
        sb.Append(" ON ");
        sb.AppendLine(join.OnClause);
      }

      // add the where clause
      if (!string.IsNullOrWhiteSpace(WhereClause))
      {
        sb.AppendLine("WHERE")
          .Append("  ")
          .AppendLine(WhereClause);
      }

      // add the order by
      if (OrderKeys.Count > 0)
      {
        sb.Append("ORDER BY ");
        Func<bool, string> ascToString = x => x ? "ASC" : "DESC";
        string keys = string.Join(',', OrderKeys.Select(k => $"{k.key} {ascToString(k.isAscending)}"));
        sb.AppendLine(keys);
      }

      // add limit
      if (LimitCount != null && LimitCount > 0)
      {
        if (LimitOffset.HasValue)
        {
          sb.AppendLine($"LIMIT {LimitOffset}, {LimitCount}");
        }
        else
        {
          sb.AppendLine($"LIMIT {LimitCount}");
        }
      }

      sb.AppendLine(";");

      return sb.ToString();
    }

    private class Join
    {
      public string Table;
      public string Alias;
      public JoinType Type;
      public string OnClause;
    }
  }

  /// <summary>
  /// The join types.
  /// </summary>
  public enum JoinType
  {
    /// <summary>
    /// INNER join
    /// </summary>
    Inner,

    /// <summary>
    /// LEFT join
    /// </summary>
    Left,
    /// <summary>
    /// Right join
    /// </summary>
    Right,
    /// <summary>
    /// Full join
    /// </summary>
    Full
  };
}
