using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RustCommitsSharp.API;

public class CommitUser
{
    /// <summary>
    /// The name of the person.
    /// </summary>
    [JsonProperty("name")]
    public string Name;

    /// <summary>
    /// The avatar URL of the person.
    /// </summary>
    [JsonProperty("avatar")]
    public string Avatar;
}