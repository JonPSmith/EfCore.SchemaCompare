// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace DataLayer.JsonColumnDb;

public class HeadEntry
{
    public int Id { get; set; }
    public int HeadInt { get; set; }
    public OuterJsonMap JsonParts { get; set; }

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(HeadInt)}: {HeadInt}, {nameof(JsonParts)}: {JsonParts}";
    }
}

public class OuterJsonMap
{
    public int OuterInt { get; set; }
    public string OuterString { get; set; }
    public DateTime OuterDate { get; set; }
    public InnerJsonMap InnerJsonMap { get; set; } = null!;

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{OuterInt}, {OuterString}, {OuterDate}, {InnerJsonMap}";
    }
}

public class InnerJsonMap
{
    public int InnerInt { get; set; }
    public string InnerString { get; set; }
    public DateTime InnerDate { get; set; }

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{InnerInt}, {InnerString}, {InnerDate}";
    }
}

public class Normal
{
    public int Id { get; set; }
    public string NormalString { get; set; }
    public int NormalExtraId { get; set; }

    public NormalExtra NormalExtra { get; set; }
}

public class NormalExtra
{
    public int NormalExtraId { get; set; }
    public string ExtraString { get; set; }
}