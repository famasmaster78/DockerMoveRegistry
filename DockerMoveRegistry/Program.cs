
// Import docker registry class
using System.Diagnostics;
using DockerMoveRegistry;

DockerMoveRegistry.DockerAPI? oldDocker = null;
DockerMoveRegistry.DockerAPI? newDocker = null;

// Start message
Console.WriteLine("Starting DockerMoveRegistry");

// Tell what this program does
Console.WriteLine("This program moves a Docker registry from one server to another.");

// Check if DockerAPI is defined, otherwise prompt user to define it
void CheckDockerAPI()
{
    if (oldDocker == null)
    {
        Console.WriteLine("Please define the old Docker registry API");
        Console.Write("Enter the URL: ");
        string oldDomain = Console.ReadLine();
        oldDocker = new DockerMoveRegistry.DockerAPI(oldDomain);

        // Do you want to set credentials?
        Console.Write("Do you want to set credentials? (y/n): ");

        string input = Console.ReadLine();

        // If yes, set credentials
        if (input == "y")
        {

            // Credentials already set?
            bool credentialsSet = false;

            // Check if credentials are already set on newDocker
            if (newDocker != null && newDocker.client.DefaultRequestHeaders.Authorization != null)
            {

                // Do you want to use the same credentials as newDocker?
                Console.Write("Do you want to use the same credentials as new Docker Registry? (y/n): ");

                string input2 = Console.ReadLine();

                if (input2 == "y")
                {
                    oldDocker.client.DefaultRequestHeaders.Authorization = newDocker.client.DefaultRequestHeaders.Authorization;
                    credentialsSet = true;
                }

            }

            // If credentials are not set, set them
            if (!credentialsSet)
            {
                (string username, string password) = GetCredentials();
                oldDocker.SetCredentials(username, password);
            }
        }

    }
    if (newDocker == null)
    {
        Console.WriteLine("Please define the new Docker registry API");
        Console.Write("Enter the URL: ");
        string newURL = Console.ReadLine();
        newDocker = new DockerMoveRegistry.DockerAPI(newURL);

        // Do you want to set credentials?
        Console.Write("Do you want to set credentials? (y/n): ");

        string input = Console.ReadLine();

        // If yes, set credentials
        if (input == "y")
        {

            // Credentials already set?
            bool credentialsSet = false;

            // Check if credentials are already set on oldDocker
            if (oldDocker != null && oldDocker.client.DefaultRequestHeaders.Authorization != null)
            {

                // Do you want to use the same credentials as oldDocker?
                Console.Write("Do you want to use the same credentials as new Docker Registry? (y/n): ");

                string input2 = Console.ReadLine();

                if (input2 == "y")
                {
                    newDocker.client.DefaultRequestHeaders.Authorization = oldDocker.client.DefaultRequestHeaders.Authorization;
                    credentialsSet = true;
                }

            }

            // If credentials are not set, set them
            if (!credentialsSet)
            {
                (string username, string password) = GetCredentials();
                newDocker.SetCredentials(username, password);
            }
        }
    }

    // Log docker api
    Console.WriteLine("Old Docker registry: " + oldDocker.DOMAIN);
    Console.WriteLine("New Docker registry: " + newDocker.DOMAIN);
}

// Get credentials
(string,string) GetCredentials()
{
    Console.Write("Enter the username: ");
    string username = Console.ReadLine();
    Console.Write("Enter the password: ");
    string password = Console.ReadLine();
    return (username, password);
}

// Move registry
void MoveRegistry()
{
	// Check if DockerAPI is defined
	CheckDockerAPI();

	// Get all repositories for each registry
	List<string> oldRepos = oldDocker.GetRepositories().Result;
	List<string> newRepos = newDocker.GetRepositories().Result;

	// Pull all repositories from old to new registry
	foreach (string repo in oldRepos)
	{
		// Pull all tags for the repository
		List<string> tags = oldDocker.GetTags(repo).Result;

		// Pull all tags from old to new registry
		foreach (string tag in tags)
		{
			// Console.WriteLine("Pulling " + repo + ":" + tag);
			// Pull image
			// Run command: $"docker pull {oldDocker.DOMAIN}/{repo}:{tag}"
			var stringPullCommand = $"docker pull {oldDocker.DOMAIN}/{repo}:{tag}";

			// Run command
			RunDockerCommand(oldDocker, stringPullCommand);

			// Tagging
			// Console.WriteLine($"Tagging {oldDocker.DOMAIN}/{repo}:{tag} -> {newDocker.DOMAIN}/{repo}:{tag}");

			// Tag image
			// Run command: $"docker tag {oldDocker.DOMAIN}/{repo}:{tag} {newDocker.DOMAIN}/{repo}:{tag}"
			var stringTagCommand = $"docker tag {oldDocker.DOMAIN}/{repo}:{tag} {newDocker.DOMAIN}/{repo}:{tag}";

			// Run command
			RunCommand(stringTagCommand);

			// Pushing
			// Console.WriteLine($"Pushing {newDocker.DOMAIN}/{repo}:{tag}");

			// Push image
			// Run command: $"docker push {newDocker.DOMAIN}/{repo}:{tag}"
			var stringPushCommand = $"docker push {newDocker.DOMAIN}/{repo}:{tag}";

			// Run command
			RunDockerCommand(newDocker, stringPushCommand);

			// readKey
			Console.ReadKey();
		}
	}
}

// Functionto RunDocerCommand, this will login to the registry if credentials are set
void RunDockerCommand(DockerAPI docker, string command)
{
	// Check if credentials are set
	if (docker.client.DefaultRequestHeaders.Authorization != null)
	{
		// Run command
		RunCommand(command);
	}
	else
	{

		// Login to registry
		// Run command: $"docker login {docker.DOMAIN} -u {username} -p {password}"
		var stringLoginCommand = $"docker login {docker.DOMAIN} -u {docker.username} -p {docker.password}";

		// Run command
		RunCommand($"{stringLoginCommand} && {command}");

	}
}

// Function to run command and print output
void RunCommand(string command)
{

	// Log running command
	Console.WriteLine("Running command: " + command);

	// Run command
	var process = new Process
	{
		StartInfo = new ProcessStartInfo
		{
			FileName = "/bin/zsh",
			Arguments = $"-c \"{command}\"",
			UseShellExecute = true,
			// RedirectStandardOutput = true,
			CreateNoWindow = false,
			WindowStyle = ProcessWindowStyle.Normal
		}
	};
	process.Start();
}

// Function to select action
void SelectAction()
{



    // If DockerAPI are defined, print the URL
    if (oldDocker != null && newDocker != null)
    {
        Console.WriteLine("Old Docker registry: " + oldDocker.DOMAIN);
        Console.WriteLine("New Docker registry: " + newDocker.DOMAIN);
    }

    // Show numbered list of possibilities
    Console.WriteLine("What do you want to do?");
    Console.WriteLine("1. Move a registry");
    Console.WriteLine("2. Pull all repositories");
    Console.WriteLine("3. List all repositories");
    Console.WriteLine("4. List all tags for a repository");
    Console.WriteLine("-------------------------------");
    Console.WriteLine("6. Exit");

    // Get user input
    Console.Write("Enter a number: ");
    string input = Console.ReadLine();

    // Check user input
    if (input == "1")
    {
        // Move a registry
		CheckDockerAPI();

		MoveRegistry();
    }
    else if (input == "2")
    {
        // Pull all repositories
        Console.WriteLine("Pull all repositories");

        // Check if DockerAPI is defined
        CheckDockerAPI();

        // Get all repositories for each registry
        List<string> oldRepos = oldDocker.GetRepositories().Result;
        List<string> newRepos = newDocker.GetRepositories().Result;

        // Pull all repositories from old to new registry
        foreach (string repo in oldRepos)
        {
            // Pull all tags for the repository
            List<string> tags = oldDocker.GetTags(repo).Result;

            // Pull all tags from old to new registry
            foreach (string tag in tags)
            {
                Console.WriteLine("Pulling " + repo + ":" + tag);
                // Pull image
                // Run command: $"docker pull {oldDocker.DOMAIN}/{repo}:{tag}"
				var stringPullCommand = $"docker pull {oldDocker.DOMAIN}/{repo}:{tag}";

				// Run command
				RunCommand(stringPullCommand);

            }
        }
    }
    else if (input == "3")
    {
        // Check if DockerAPI is defined
        CheckDockerAPI();

        // Get all repositories for each registry
        List<string> oldRepos = oldDocker.GetRepositories().Result;
        List<string> newRepos = newDocker.GetRepositories().Result;

        // Show repositories
        Console.WriteLine("Old registry repositories:");
        foreach (string repo in oldRepos)
        {
            Console.WriteLine(repo);
        }

        Console.WriteLine();

        Console.WriteLine("New registry repositories:");
        foreach (string repo in newRepos)
        {
            Console.WriteLine(repo);
        }

        // Press any key to continue
        Console.WriteLine("Press any key to continue");
    }
    else if (input == "4")
    {
        // New or old registry?
        Console.Write("Do you want to list tags for the old or new registry? (old/new): ");
        string registry = Console.ReadLine();

        // Check if DockerAPI is defined
        CheckDockerAPI();

        // Check if old or new registry
        if (registry == "old")
        {
            // Get all repositories for old registry
            List<string> oldRepos = oldDocker.GetRepositories().Result;

            // Show repositories with numering
            Console.WriteLine("Old registry repositories:");
            for (int i = 0; i < oldRepos.Count; i++)
            {
                Console.WriteLine(i + ". " + oldRepos[i]);
            }

            // Select repository
            Console.Write("Enter the repository: ");
            string repositoryNumber = Console.ReadLine();

            // Get repository name
            string repository = oldRepos[int.Parse(repositoryNumber)];

            Console.WriteLine(repository);

            // Get all tags for the repository
            List<string> tags = oldDocker.GetTags(repository).Result;

            // Show tags
            Console.WriteLine("Tags for " + repository + ":");
            foreach (string tag in tags)
            {
                Console.WriteLine(tag);
            }
        }
        else if (registry == "new")
        {
            // Get all repositories for new registry
            List<string> newRepos = newDocker.GetRepositories().Result;

            // Show repositories with numbering
            Console.WriteLine("New registry repositories:");
            for (int i = 0; i < newRepos.Count; i++)
            {
                Console.WriteLine(i + ". " + newRepos[i]);
            }

            // Select repository
            Console.Write("Enter the repository: ");
            string repositoryNumber = Console.ReadLine();

            // Get repository name
            string repository = newRepos[int.Parse(repositoryNumber)];

            Console.WriteLine(repository);

            // Get all tags for the repository
            List<string> tags = newDocker.GetTags(repository).Result;

            // Show tags
            Console.WriteLine("Tags for " + repository + ":");
            foreach (string tag in tags)
            {
                Console.WriteLine(tag);
            }
        }else
        {
            // Invalid input
            Console.WriteLine("Invalid input");
        }

    }
    else if (input == "6")
    {
        // Exit
        Console.WriteLine("Exiting DockerMoveRegistry");
        return;
    }
    else
    {
        // Invalid input
        Console.WriteLine("Invalid input");
        // Press any key to continue
        Console.WriteLine("Press any key to continue");
        Console.ReadKey();
        // Clear console
        Console.Clear();
        // Show menu again
        SelectAction();
        return;
    }

    // Press any key to continue
    Console.WriteLine("Press any key to continue");
    Console.ReadKey();

    // Clear console
    Console.Clear();
    SelectAction();
}

// Show menu
SelectAction();

// End message
Console.WriteLine("DockerMoveRegistry ended");

// Read key to exit
Console.ReadKey();

