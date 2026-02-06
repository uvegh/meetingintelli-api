namespace MeetingIntelli.DTO.Responses;

public class MeetingStatisticsResponse
{
    public List<MonthlyCount> ByMonth { get; set; } = new();
}

public class MonthlyCount
{
    public string Month { get; set; } = string.Empty; //"2024-02"
    public int Count { get; set; }
}