using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RustCommitsSharp.API;

public class Commit
{
    /// <summary>
    /// The ID of the commit;
    /// </summary>
    [JsonProperty("id")]
    public int Id;

    /// <summary>
    /// The full branch path.
    /// </summary>
    [JsonProperty("branch")]
    public string BranchPath;

    /// <summary>
    /// The changeset ID.
    /// </summary>
    [JsonProperty("changeset")]
    public string ChangesetID;

    /// <summary>
    /// The creation time.
    /// </summary>
    [JsonProperty("created")]
    public DateTime? Created;

    /// <summary>
    /// The like count.
    /// </summary>
    [JsonProperty("likes")]
    public int Likes;

    /// <summary>
    /// The dislike count.
    /// </summary>
    [JsonProperty("dislikes")]
    public int Dislikes;

    /// <summary>
    /// The message.
    /// </summary>
    [JsonProperty("message")]
    public string Message;

    /// <summary>
    /// The creator person.
    /// </summary>
    [JsonProperty("user")]
    public CommitUser User;
}