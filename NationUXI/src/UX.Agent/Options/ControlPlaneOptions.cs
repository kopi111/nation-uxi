namespace UX.Agent;

public class ControlPlaneOptions
{
    public string BaseUrl { get; set; } = "https://localhost:5001/";
    public int HttpTimeoutSeconds { get; set; } = 15;
    public int CheckinIntervalSeconds { get; set; } = 60;
}
