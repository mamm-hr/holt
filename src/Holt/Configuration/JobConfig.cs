using System.Xml.Serialization;

namespace Holt.Configuration;

[XmlRoot("job")]
public class JobConfig
{
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("repositoryUrl")]
    public string RepositoryUrl { get; set; } = string.Empty;

    [XmlElement("branch")]
    public string Branch { get; set; } = "main";

    [XmlElement("localPath")]
    public string LocalPath { get; set; } = string.Empty;

    [XmlElement("intervalMinutes")]
    public int IntervalMinutes { get; set; } = 60;
}
