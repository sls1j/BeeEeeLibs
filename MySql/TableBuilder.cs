using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BeeEeeLibs.Mysql
{
  /// <summary>
  /// A class to build the SQL for a table
  /// </summary>
  public class TableBuilder
  {
    private string TableName;
    private bool IfNotExists;
    private List<Column> Columns;
    private List<ForeignKey> ForeignKeys;
    private List<SimpleIndex> Indexes;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <param name="ifNotExists">If true then test to make sure the table doesn't already exist</param>
    public TableBuilder(string tableName, bool ifNotExists = true)
    {
      TableName = tableName;
      IfNotExists = ifNotExists;
      Columns = new List<Column>();
      ForeignKeys = new List<ForeignKey>();
      Indexes = new List<SimpleIndex>();
    }

    /// <summary>
    /// Adds a field to the table
    /// </summary>
    /// <param name="name">The name of the column</param>
    /// <param name="type">The type of the column</param>
    /// <param name="isNotNull">If true then the value cannot be null</param>
    /// <param name="isPrimaryKey">If true then the field is the primary key</param>
    /// <param name="isUnique">If true then the column must be unique</param>
    /// <param name="isAutoIncrement">If true then the column is auto-incremented</param>
    /// <param name="custom">An SQL string that is added to the end of the column definition.  Can be null</param>
    /// <param name="defaultValue"></param>
    /// <returns>The table builder</returns>
    public TableBuilder AddField(string name, string type, bool isNotNull = false, bool isPrimaryKey = false, bool isUnique = false, bool isAutoIncrement = false, string custom = null, string defaultValue = null)
    {
      Column f = new Column()
      {
        IsAutoIncrement = isAutoIncrement,
        IsNotNull = isNotNull,
        IsPrimaryKey = isPrimaryKey,
        IsUnique = isUnique,
        Name = name,
        Type = type,
        Custom = custom,
        DefaultValue = defaultValue
      };

      Columns.Add(f);

      return this;
    }

    /// <summary>
    /// Adds a foreign key to the table
    /// </summary>
    /// <param name="name">The name of the foreign key</param>
    /// <param name="column">The name of the column to build the key for</param>
    /// <param name="foreignTable">The foreign table name</param>
    /// <param name="foreignColumn">The foreign column to build the key for</param>
    /// <returns>The table builder</returns>
    public TableBuilder AddForeignKey(string name, string column, string foreignTable, string foreignColumn)
    {
      ForeignKey key = new ForeignKey()
      {
        Name = name,
        Column = column,
        ForeignTable = foreignTable,
        ForeignColumn = foreignColumn
      };

      ForeignKeys.Add(key);

      return this;
    }

    /// <summary>
    /// Adds an index to the given column
    /// </summary>
    /// <param name="name">The name of the index</param>
    /// <param name="columns">The column</param>
    /// <returns>The table builder</returns>
    /// <exception cref="ArgumentException">If one of the columns is null, or empty</exception>
    public TableBuilder AddSimpleIndex(string name, params string[] columns)
    {
      if (columns.Length == 0)
        throw new ArgumentException("Must have some columns specified.");

      SimpleIndex index = new SimpleIndex()
      {
        Columns = columns,
        Name = name
      };

      Indexes.Add(index);

      return this;
    }

    /// <summary>
    /// Adds a unique index to a given column
    /// </summary>
    /// <param name="name">The name of the index</param>
    /// <param name="columns">The name of the column to create an index for</param>
    /// <returns>The table builder</returns>
    /// <exception cref="ArgumentException">If the name or columns are null or empty</exception>
    public TableBuilder AddUniqueIndex(string name, params string[] columns)
    {
      if (columns.Length == 0)
        throw new ArgumentException("Must have some columns specified.");

      SimpleIndex index = new SimpleIndex()
      {
        Columns = columns,
        Name = name,
        IsUnique = true
      };

      Indexes.Add(index);

      return this;
    }

    /// <summary>
    /// Adds a spatial index to a column
    /// </summary>
    /// <param name="name">The name of the index</param>
    /// <param name="column">The name of the column</param>
    /// <returns>The table builder</returns>
    public TableBuilder AddSpatialIndex(string name, string column)
    {
      SimpleIndex index = new SimpleIndex()
      {
        Columns = new string[] { column },
        Name = name,
        Spatial = true
      };

      Indexes.Add(index);
      return this;
    }

    /// <summary>
    /// Translates the added information into SQL for creating a table
    /// </summary>
    /// <returns>The SQL string</returns>
    /// <exception cref="InvalidOperationException">When some of the context is invalid</exception>
    public string ToSql()
    {
      if (string.IsNullOrWhiteSpace(TableName))
        throw new InvalidOperationException("Must define the table name!");

      if (Columns.Count == 0)
        throw new InvalidOperationException("Must define at least one column");

      if (Columns.Count(c => c.IsPrimaryKey) > 1)
        throw new InvalidOperationException("Can only have one primary key");


      StringBuilder sb = new StringBuilder();

      sb.Append($"CREATE TABLE ");
      if (IfNotExists)
      {
        sb.Append("IF NOT EXISTS ");
      }
      sb.AppendLine($"{TableName} (");

      List<string> lines = new List<string>();
      // add columns
      foreach (var column in Columns)
      {
        StringBuilder line = new StringBuilder();
        line.Append($"  {column.Name} {column.Type}");
        if (column.IsNotNull)
          line.Append(" NOT NULL");

        if (column.IsAutoIncrement)
          line.Append(" AUTO_INCREMENT");

        if (column.IsPrimaryKey)
          line.Append(" PRIMARY KEY");
        else if (column.IsUnique)
          line.Append(" UNIQUE KEY");

        if (!string.IsNullOrWhiteSpace(column.DefaultValue))
        {
          line.Append(" DEFAULT ");
          line.Append(column.DefaultValue);
        }

        if (!string.IsNullOrWhiteSpace(column.Custom))
          line.Append(" ")
            .Append(column.Custom);

        lines.Add(line.ToString());
      }

      // add indexes
      foreach (var index in Indexes)
      {
        StringBuilder sql = new StringBuilder("  ");
        if (index.IsUnique)
        {
          sql.Append("UNIQUE KEY");
        }
        else if (index.Spatial)
        {
          sql.Append("SPATIAL INDEX");
        }
        else
        {
          sql.Append("INDEX");
        }

        sql
          .Append(" (")
          .Append(string.Join(",", index.Columns))
          .Append(")");

        lines.Add(sql.ToString());
      }

      // add foreign keys
      foreach (var fkey in ForeignKeys)
      {
        lines.Add($"  FOREIGN KEY {fkey.Name} ({fkey.Column}) REFERENCES {fkey.ForeignTable} ({fkey.ForeignColumn})");
      }

      sb.AppendLine(string.Join(",\r\n  ", lines));

      sb.AppendLine(")");

      string fullSql = sb.ToString();
      return fullSql;
    }

    /// <summary>
    /// Creates a c# class that matches the table definition.
    /// </summary>
    /// <param name="className">The name of the class to generate</param>
    /// <param name="namespace">The namespace of the class to generate</param>
    /// <returns>The c# code</returns>
    public string ToCSharp(string className, string @namespace)
    {
      CodeNamespace code = new CodeNamespace(@namespace);

      CodeTypeDeclaration @class = new CodeTypeDeclaration(className);
      code.Types.Add(@class);

      Func<string, string> cap = (x) => char.ToUpper(x[0]) + x.Substring(1);

      Action<string, string> addMember = (t, n) => @class.Members.Add(new CodeMemberField(t, cap(n)) { Attributes = MemberAttributes.Public });

      foreach (Column field in Columns)
      {
        string type = field.Type.ToLowerInvariant();
        switch (type)
        {
          case "int": addMember("System.Int32", field.Name); break;
          case "bigint": addMember("System.Int64", field.Name); break;
          case "char(36)": addMember("System.Guid", field.Name); break;
          case "tinyint": addMember("System.Boolean", field.Name); break;
          case "geometry": addMember("GeoPrimitive", field.Name); break;
          case "text": addMember("System.String", field.Name); break;
          case "date":
          case "datetime":
          case "timestamp": addMember("System.DateTime", field.Name); break;

          default:
            if (type.StartsWith("nvarchar") || type.StartsWith("char"))
            {
              addMember("System.String", field.Name);
            }
            else
            {
              addMember("System.Object", field.Name);
            }
            break;
        }
      }

      CSharpCodeProvider provider = new CSharpCodeProvider();
      using (StringWriter writer = new StringWriter())
      {
        provider.GenerateCodeFromNamespace(code, writer, new System.CodeDom.Compiler.CodeGeneratorOptions());
        return writer.ToString();
      }
    }


    private class Column
    {
      public string Name;
      public string Type;
      public bool IsNotNull;
      public bool IsUnique;
      public bool IsPrimaryKey;
      public bool IsAutoIncrement;
      public string Custom;
      public string DefaultValue;
    }

    private class ForeignKey
    {
      public string Name;
      public string Column;
      public string ForeignTable;
      public string ForeignColumn;
    }

    private class SimpleIndex
    {
      public string Name;
      public string[] Columns;
      public bool Spatial;
      public bool IsUnique;
    }
  }
}
