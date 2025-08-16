namespace ImageBuilder;

using ImageDownloader;

public class ProgramParser
{
    public static List<Program> ParsePrograms(List<object> yamlData)
    {
        var programs = new List<Program>();
        foreach (Dictionary<object, object> programDict in yamlData)
        {
            programs.Add(ParseProgram(programDict));
        }
        return programs;
    }

    public static Program ParseProgram(Dictionary<object, object> programDict)
    {
        var program = new Program
        {
            Name = programDict["name"].ToString(),
            ArtifactSource = ParseArtifactSource(programDict["artifactSource"] as Dictionary<object, object>)
        };

        if (programDict.ContainsKey("thirdParty"))
        {
            var thirdPartyList = new List<Program>();
            var thirdPartyYamlList = programDict["thirdParty"] as List<object>;
            foreach (Dictionary<object, object> thirdPartyDict in thirdPartyYamlList)
            {
                thirdPartyList.Add(ParseProgram(thirdPartyDict));
            }
            program.ThirdParty = thirdPartyList;
        }

        return program;
    }

    public static IArtifactSource ParseArtifactSource(Dictionary<object, object> sourceDict)
    {
        string type = sourceDict["type"].ToString();

        switch (type.ToLower())
        {
            case "jenkins":
                return new JenkinsArtifactSource
                {
                    ProgramName = sourceDict["programName"].ToString(),
                    Address = sourceDict["address"].ToString(),
                    JobName = sourceDict["jobName"].ToString(),
                    JobIdentifier = sourceDict["jobIdentifier"].ToString(),
                    ArtifactPath = sourceDict["artifactPath"].ToString()
                };
            case "github":
                return new GitHubArtifactSource
                {
                    ProgramName = sourceDict["programName"].ToString(),
                    Owner = sourceDict["owner"].ToString(),
                    Repo = sourceDict["repo"].ToString(),
                    Tag = sourceDict["tag"].ToString(),
                    ArtifactName = sourceDict["artifactName"].ToString()
                };
            default:
                throw new NotSupportedException($"지원되지 않는 아티팩트 소스 타입: {type}");
        }
    }
}