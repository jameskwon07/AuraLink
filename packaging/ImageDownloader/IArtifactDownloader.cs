using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

// 이 코드는 아티팩트 다운로더 기능을 추상화하여, 젠킨스 외의 다양한 소스를 지원합니다.
// NuGet 패키지: Microsoft.Extensions.Logging.Abstractions
// dotnet add package Microsoft.Extensions.Logging.Abstractions

// 아티팩트 다운로드 기능을 정의하는 인터페이스.
// 이 인터페이스를 통해 다운로더를 사용하는 모듈(예: ImageBuilder)은
// 어떤 구체적인 소스(Jenkins, GitHub 등)를 사용하는지 알 필요가 없어집니다.
namespace ImageDownloader
{
    public interface IArtifactDownloader
    {
        Task DownloadArtifactAsync(string localSavePath);
    }
        
}
