// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace DataLayer.JsonColumnDb;

public class HeadEntry
{
    public int Id { get; set; }
    public int HeadInt { get; set; }
    public TopJsonMap JsonParts { get; set; }
    public ExtraJson ExtraJsonParts { get; set; }

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(HeadInt)}: {HeadInt}, {nameof(JsonParts)}: {JsonParts}, {nameof(ExtraJsonParts)}: {ExtraJsonParts}";
    }
}

public class ExtraJson
{
    public string ExtraString { get; set; }

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{ExtraString}";
    }
}

public class TopJsonMap
{
    public string TopString { get; set; }
    public MiddleJsonMap MiddleJsonMap { get; set; } = null!;

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{TopString}, {MiddleJsonMap}";
    }
}

public class MiddleJsonMap
{
    public string MiddleJsonString { get; set; }
    public BottomJsonMap BottomJsonMap { get; set; }

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{MiddleJsonString}, {BottomJsonMap}";
    }
}

public class BottomJsonMap
{
    public string BottomJsonString { get; set; }

    /// <summary>
    /// Useful for debugging
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return BottomJsonString;
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