namespace ImageDownloader
{
    // 골든 이미지 설정 파일의 프로그램을 나타내는 클래스.
    public class Program
    {
        public string Name { get; set; }
        public IArtifactSource ArtifactSource { get; set; } // 인터페이스로 추상화
        public List<Program> ThirdParty { get; set; } // 의존성 프로그램 목록
    }

    // 아티팩트 소스를 추상화하는 인터페이스.
    // YAML 파싱을 위해 구체적인 클래스에 대한 식별 정보가 필요합니다.
    public interface IArtifactSource
    {
        string Type { get; }
        string ProgramName { get; set; }
    }


}
