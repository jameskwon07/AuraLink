namespace ImageDownloader;

using Microsoft.Extensions.Logging;

public interface IDownloaderFactory
{
    IArtifactDownloader CreateDownloader(ILoggerFactory loggerFactory, IArtifactSource source);
}
