using Docker.DotNet;
using Docker.DotNet.Models;

namespace StateContainer.web.Docker
{
    public class DockerService
    {
        private readonly DockerClient _client;

        public DockerService()
        {
            _client = new DockerClientConfiguration(new Uri("http://localhost:2375")).CreateClient();
        }

        public async Task PullImageAsync(string imageName, string tag)
        {
            await _client.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = imageName,
                    Tag = tag
                },
                new AuthConfig(),
                new Progress<JSONMessage>());
        }

        public async Task<string> CreateContainerAsync(DockerConfig config)
        {
            string imageToCreateWithTag = $"{config.ImageName}:{config.Tag}";
            var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = imageToCreateWithTag,
                Name = config.ContainerName,
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { $"{config.ContainerPort}/tcp", new List<PortBinding> { new PortBinding { HostPort = config.HostPort.ToString() } } }
                }
                }
            });

            return response.ID;
        }

        public async Task<bool> StartContainerAsync(string containerId, ContainerStartParameters? startParams = null)
        {
            return await _client.Containers.StartContainerAsync(containerId, startParams);
        }

        public async Task CaptureLogAsync(string containerId)
        {
            using (var logStream = await _client.Containers.GetContainerLogsAsync(containerId, false, new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Follow = true
            }))
            {
                var stdout = new MemoryStream();
                var stderr = new MemoryStream();

                await logStream.CopyOutputToAsync(null,stdout, stderr, CancellationToken.None);
                stdout.Seek(0, SeekOrigin.Begin);
                stderr.Seek(0, SeekOrigin.Begin);

                using (var stdoutReader = new StreamReader(stdout))
                {
                    string line;
                    while ((line = await stdoutReader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine($"STDOUT: {line}");
                    }
                }

                using (var stderrReader = new StreamReader(stderr))
                {
                    string line;
                    while ((line = await stderrReader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine($"STDERR: {line}");
                    }
                }
            }
        }
    }
}
