namespace Pooshit.Ocelot.Fields;

/// <summary>
/// field values of a database row
/// </summary>
public interface IRowValues {

    /// <summary>
    /// get a named field
    /// </summary>
    /// <param name="name">name of field</param>
    /// <returns>value of field with the specified name</returns>
    object GetFieldValue(string name);

    /// <summary>
    /// get a named field
    /// </summary>
    /// <param name="name">name of field</param>
    /// <typeparam name="T">type of value to get</typeparam>
    /// <returns>value of field with the specified name</returns>
    T GetFieldValue<T>(string name);
}