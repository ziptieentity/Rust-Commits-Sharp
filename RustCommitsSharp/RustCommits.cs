using Newtonsoft.Json;
using RustCommitsSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RustCommitsSharp;

public class RustCommits : IDisposable
{
    #region Fields
    // Const
    private const string URL = "https://commits.facepunch.com/";
    private const string RUST_REPO = "rust_reboot";

    // Public
    public bool IsDisposed { get; private set; }
    /// <summary>
    /// Called after a poll if there are new commits.
    /// </summary>
    public event Action<List<Commit>>? OnCommitPoll;

    // Private
    private HttpClient _httpClient;
    private DateTime _nextPollTime;
    private List<int> _lastCommitPollCache = new List<int>();
    #endregion

    #region Protected Methods
    private async Task<T?> SendAsync<T>(string endpoint, string method = "GET")
    {
        // Create HTTP request.
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(URL + endpoint),
            Method = HttpMethod.Parse(method)
        };

        // Send HTTP request, convert to response into a string and deserialise the JSON string into the specified type.
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }
    #endregion

    #region Main Methods
    /// <summary>
    /// Gets the commit with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the commit.</param>
    /// <returns>A commit.</returns>
    public async Task<Commit?> GetCommitAsync(int id)
    {
        // Fetch the data and check if it is valid.
        var json = await SendAsync<CommitResult>($"{id}?format=json");
        if (json == null)
            return null;

        // Return null if the total of commits is less than 1. 
        if (json.Total < 1)
            return null;
        return json.Results[0];
    }
    /// <summary>
    /// Gets all of the commits on the specified page.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>A list of commits.</returns>
    public async Task<List<Commit>> GetCommitsAsync(int page)
    {
        return await GetCommitsAsync("", page);
    }
    /// <summary>
    /// Gets all of the commits on the specified branch and page.
    /// </summary>
    /// <param name="branch">The path of branch (without leading/trailing slashes).</param>
    /// <param name="page">The page.</param>
    /// <returns>A list of commits.</returns>
    public async Task<List<Commit>> GetCommitsAsync(string branch, int page)
    {
        // Fetch the data and check if it is valid.
        var json = await SendAsync<CommitResult>($"r/{RUST_REPO}/{ValidateBranch(branch)}?p={ValidatePage(page)}&format=json");
        if (json == null)
            return new List<Commit>();

        // Return the results.
        return json.Results;
    }
    /// <summary>
    /// Gets all of the commits on the specified user and page.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="page">The page.</param>
    /// <returns>A list of commits.</returns>
    public async Task<List<Commit>> GetUserCommitsAsync(string username, int page)
    {
        return await GetUserCommitsAsync(username, "", page);
    }
    /// <summary>
    /// Gets all of the commits on the specified user, branch and page.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="branch">The path of branch (without leading/trailing slashes).</param>
    /// <param name="page">The page.</param>
    /// <returns>A list of commits.</returns>
    public async Task<List<Commit>> GetUserCommitsAsync(string username, string branch, int page)
    {
        // Fetch the data and check if it is valid.
        var json = await SendAsync<CommitResult>($"{username}?p={ValidatePage(page)}&format=json/{RUST_REPO}/{ValidateBranch(branch)}");
        if (json == null)
            return new List<Commit>();

        // Return the results.
        return json.Results;
    }
    /// <summary>
    /// Starts polling to the commits website every 5 minutes.
    /// <para> You can set a custom interval with: <seealso cref="StartPollingAsync(TimeSpan)"/>.</para>
    /// </summary>
    public async Task StartPollingAsync()
    {
        await StartPollingAsync(new TimeSpan(0, 5, 0));
    }
    /// <summary>
    /// Starts polling to the commits website at the specified interval.
    /// </summary>
    /// <param name="pollInterval">The interval in which to poll the commits website.</param>
    public async Task StartPollingAsync(TimeSpan pollInterval)
    {
        while (!IsDisposed)
        {
            // Check and update the next poll time.
            if (_nextPollTime > DateTime.UtcNow)
                continue;
            _nextPollTime = DateTime.UtcNow.Add(pollInterval);

            /* Get lastest commits and filter out all of the commits that are present in the last commits cache. 
            This gets all of the commits from between the last poll and now.
            Weird conversions and LINQ, but this was the best way I could think of doing it. Feel free to fork the repo if you have an improvement. */
            var commits = await GetCommitsAsync(1);
            var newCommits = commits.ExceptBy(_lastCommitPollCache, x => x.Id).ToList();

            /* Don't call the event if the last commit poll cache is empty.
            This is so the event isn't called on the initial poll when the last commits cache is empty and only when there are new commits. 
            Also don't call if there are new no commits. */
            if(_lastCommitPollCache.Count > 0 && newCommits.Count > 0)
                OnCommitPoll?.Invoke(newCommits);

            // Set the last commits cache to the latest commits.
            _lastCommitPollCache = commits.Select(x => x.Id).ToList();
        }
    }
    #endregion

    #region Helpers
    private string ValidateBranch(string branch)
    {
        // Remove any leading/trailing slashes (they are already added in the endpoint).
        if (branch.StartsWith("/"))
            branch = branch.Substring(1);
        if (branch.EndsWith("/"))
            branch = branch.Remove(branch.Length - 1);
        return branch;
    }
    private int ValidatePage(int page)
    {
        // Check and fix if the page is less than 0 or greater than the 32-bit interger limit.
        if (page < 1)
            page = 1;
        if (page > int.MaxValue)
            page = int.MaxValue;
        return page;
    }
    #endregion

    #region Virtual Methods
    private void Dispose(bool isDisposing)
    {
        if (IsDisposed)
            return;

        // Dispose of the HTTPClient if we are disposing.
        if (_httpClient != null && isDisposing)
            _httpClient.Dispose();

        IsDisposed = true;
    }
    #endregion

    #region Constructor
    public RustCommits()
    {
        _httpClient = new HttpClient();
    }
    ~RustCommits()
    {
        Dispose(false);
    }
    #endregion

    #region Interface Methods
    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}