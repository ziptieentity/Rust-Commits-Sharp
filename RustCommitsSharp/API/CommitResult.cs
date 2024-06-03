using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RustCommitsSharp.API;

public class CommitResult
{
    /// <summary>
    /// The total number of commits.
    /// </summary>
    [JsonProperty("total")]
    public int Total;

    /// <summary>
    /// The list of commits.
    /// </summary>
    [JsonProperty("results")]
    public List<Commit> Results;
}