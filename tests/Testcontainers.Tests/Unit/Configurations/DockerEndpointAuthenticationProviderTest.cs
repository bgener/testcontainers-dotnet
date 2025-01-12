namespace DotNet.Testcontainers.Tests.Unit
{
  using System;
  using System.Collections.Generic;
  using System.Runtime.InteropServices;
  using DotNet.Testcontainers.Builders;
  using DotNet.Testcontainers.Configurations;
  using Xunit;

  public sealed class DockerEndpointAuthenticationProviderTest
  {
    private const string DockerHost = "tcp://127.0.0.1:2375";

    [Theory]
    [ClassData(typeof(AuthProviderTestData))]
    internal void AuthProviderShouldBeApplicable(IDockerEndpointAuthenticationProvider authProvider, bool isApplicable)
    {
      Assert.Equal(isApplicable, authProvider.IsApplicable());
    }

    [Theory]
    [ClassData(typeof(AuthConfigTestData))]
    internal void AuthConfigShouldGetDockerClientEndpoint(IDockerEndpointAuthenticationConfiguration authConfig, Uri dockerClientEndpoint)
    {
      using (var dockerClientConfiguration = authConfig.GetDockerClientConfiguration())
      {
        Assert.Equal(dockerClientEndpoint, authConfig.Endpoint);
        Assert.Equal(dockerClientEndpoint, dockerClientConfiguration.EndpointBaseUri);
      }
    }

    private sealed class AuthProviderTestData : List<object[]>
    {
      public AuthProviderTestData()
      {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var defaultConfiguration = new PropertiesFileConfiguration(Array.Empty<string>());
        var dockerHostConfiguration = new PropertiesFileConfiguration(new[] { "docker.host=" + DockerHost });
        this.Add(new object[] { new EnvironmentEndpointAuthenticationProvider(defaultConfiguration), false });
        this.Add(new object[] { new EnvironmentEndpointAuthenticationProvider(dockerHostConfiguration), true });
        this.Add(new object[] { new EnvironmentEndpointAuthenticationProvider(Array.Empty<ICustomConfiguration>()), false });
        this.Add(new object[] { new EnvironmentEndpointAuthenticationProvider(defaultConfiguration, dockerHostConfiguration), true });
        this.Add(new object[] { new NpipeEndpointAuthenticationProvider(), isWindows });
        this.Add(new object[] { new UnixEndpointAuthenticationProvider(), !isWindows });
      }
    }

    private sealed class AuthConfigTestData : List<object[]>
    {
      public AuthConfigTestData()
      {
        var dockerHostConfiguration = new PropertiesFileConfiguration(new[] { "docker.host=" + DockerHost });
        this.Add(new object[] { new EnvironmentEndpointAuthenticationProvider(dockerHostConfiguration).GetAuthConfig(), new Uri(DockerHost) });
        this.Add(new object[] { new NpipeEndpointAuthenticationProvider().GetAuthConfig(), new Uri("npipe://./pipe/docker_engine") });
        this.Add(new object[] { new UnixEndpointAuthenticationProvider().GetAuthConfig(), new Uri("unix:/var/run/docker.sock") });
      }
    }
  }
}
