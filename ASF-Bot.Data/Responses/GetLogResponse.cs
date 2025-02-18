namespace ASF_Bot.Data.Responses;
public sealed record GetLogResponse : AbstractResponse<GetLogResponse.ResultData>
{
    public sealed record ResultData
    {
        public List<string> ?Content { get; set; }
        public int TotalLines { get; set; }
    }
}
