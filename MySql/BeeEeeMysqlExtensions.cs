using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeEeeLibs.Mysql
{
  /// <summary>
  /// Extensions to be used for reading MySql
  /// </summary>
  public static class BeeEeeLibsMysqlExtensions
  {
    /// <summary>
    /// Reads a datetime mysql field as UTC
    /// </summary>
    /// <param name="reader">The mysql reader</param>
    /// <param name="index">The index of the field</param>
    /// <returns>The UTC datetime</returns>
    public static DateTime GetDateTimeUtc(this IDataReader reader, int index)
    {
      DateTime value = reader.GetDateTime(index);
      if (value.Kind == DateTimeKind.Unspecified)
      {
        value = new DateTime(value.Ticks, DateTimeKind.Utc);
      }

      return value;
    }

    /// <summary>
    /// Reads a date from the MySql reader from the given index allowing for NULL to be read.
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <returns>The read DateTime or null</returns>
    public static DateTime? GetDateTimeUtcNullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;

      DateTime value = reader.GetDateTime(index);
      if (value.Kind == DateTimeKind.Unspecified)
      {
        value = new DateTime(value.Ticks, DateTimeKind.Utc);
      }

      return value;
    }

    /// <summary>
    /// Converts a given date to it's equivelant UTC date
    /// </summary>
    /// <param name="dt">The date</param>
    /// <returns>The adjusted date</returns>
    public static DateTime ToUtc(this DateTime dt)
    {
      if (dt.Kind == DateTimeKind.Unspecified)
      {
        dt = new DateTime(dt.Ticks, DateTimeKind.Utc);
      }
      return dt;
    }

    /// <summary>
    /// Guards against NULL values making it possible to read without an if statement.
    /// </summary>
    /// <typeparam name="T">The type to read</typeparam>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <param name="action">The action that will do the reading.  It is only called if the column is not null</param>
    /// <returns></returns>
    public static T GuardNull<T>(this IDataReader reader, int index, Func<IDataReader,int,T> action)
    {
      if (!reader.IsDBNull(index))
      {
        return action(reader, index);
      }

      return default(T);
    }   
    
    /// <summary>
    /// Reads a string that could be null.
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public static string GetStringNullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;
      else
        return reader.GetString(index);
    }

    /// <summary>
    /// Reads an Int16 that could be null
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <returns>The short value or null</returns>
    public static short? GetInt16Nullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;
      else
        return reader.GetInt16(index);
    }

    /// <summary>
    /// Reads an Int32 that could be null
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <returns>The int value or null</returns>
    public static int? GetInt32Nullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;
      else
        return reader.GetInt32(index);
    }

    /// <summary>
    /// Reads an Int64 that could be null
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <returns>The long value or null</returns>
    public static long? GetInt64Nullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;
      else
        return reader.GetInt64(index);      
    }

    /// <summary>
    /// Reads an Float that could be null
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <returns>The float value or null</returns>
    public static float? GetFloatNullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;
      else
        return reader.GetFloat(index);
    }

    /// <summary>
    /// Reads an Double that could be null
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="index">The column index</param>
    /// <returns>The double value or null</returns>
    public static double? GetDoubleNullable(this IDataReader reader, int index)
    {
      if (reader.IsDBNull(index))
        return null;
      else
        return reader.GetDouble(index);
    }
  }
}
