using Shouldly;

namespace RestExceptions.IntegrationTests.Services;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerClass)]
public class RestExceptionsTests(ApiFactory factory) : ApiTests(factory)
{
    [Test]
    [Arguments(0, "Internal Server Error")]
    [Arguments(400, "Bad Request")]
    [Arguments(401, "Unauthorized")]
    [Arguments(402, "Payment Required")]
    [Arguments(403, "Forbidden")]
    [Arguments(404, "Not Found")]
    [Arguments(405, "Method Not Allowed")]
    [Arguments(406, "Not Acceptable")]
    [Arguments(407, "Proxy Authentication Required")]
    [Arguments(408, "Request Timeout")]
    [Arguments(409, "Conflict")]
    [Arguments(410, "Gone")]
    [Arguments(411, "Length Required")]
    [Arguments(412, "Precondition Failed")]
    [Arguments(413, "Content Too Large")]
    [Arguments(414, "URI Too Long")]
    [Arguments(415, "Unsupported Media Type")]
    [Arguments(416, "Range Not Satisfiable")]
    [Arguments(417, "Expectation Failed")]
    [Arguments(421, "Misdirected Request")]
    [Arguments(422, "Unprocessable Content")]
    [Arguments(423, "Locked")]
    [Arguments(424, "Failed Dependency")]
    [Arguments(426, "Upgrade Required")]
    [Arguments(428, "Precondition Required")]
    [Arguments(429, "Too Many Requests")]
    [Arguments(431, "Request Header Fields Too Large")]
    [Arguments(451, "Unavailable For Legal Reasons")]
    [Arguments(500, "Internal Server Error")]
    [Arguments(501, "Not Implemented")]
    [Arguments(502, "Bad Gateway")]
    [Arguments(503, "Service Unavailable")]
    [Arguments(504, "Gateway Timeout")]
    [Arguments(505, "HTTP Version Not Supported")]
    [Arguments(506, "Variant Also Negotiates")]
    [Arguments(507, "Insufficient Storage")]
    [Arguments(508, "Loop Detected")]
    [Arguments(510, "Not Extended")]
    [Arguments(511, "Network Authentication Required")]
    public async Task Status_Code_To_Http_Error_Title(int statusCode, string expected)
    {
        // Act
        var problemDetails = await GetProblemDetailsAsync("error", statusCode);
        var result = problemDetails?.Title ?? string.Empty;

        // Assert
        result.ShouldBe(expected);
    }
}
