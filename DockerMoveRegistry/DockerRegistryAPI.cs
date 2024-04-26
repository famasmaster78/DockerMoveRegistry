using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace DockerMoveRegistry
{
    public class DockerAPI
    {

        // HTTP Client
        public HttpClient client;

        // Constructor
        public DockerAPI(string URL)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(URL);
        }

        // Set login credentials
        public void SetCredentials(string username, string password)
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        // Get all repositories
        public async Task<List<string>> GetRepositories()
        {
            List<string> repositories = new List<string>();
            HttpResponseMessage response = await client.GetAsync("/v2/_catalog");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JArray repos = (JArray)json["repositories"];
                foreach (string repo in repos)
                {
                    repositories.Add(repo);
                }
            }
            return repositories;
        }

        // Get all tags for a repository
        public async Task<List<string>> GetTags(string repository)
        {
            List<string> tags = new List<string>();
            HttpResponseMessage response = await client.GetAsync("/v2/" + repository + "/tags/list");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JArray tagsArray = (JArray)json["tags"];
                foreach (string tag in tagsArray)
                {
                    tags.Add(tag);
                }
            }
            return tags;
        }

    }
}
