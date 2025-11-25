using Instrument.Domain.Enums;

namespace Instrument.Application.Runs.DTOs.Responses;

public record StartRunResponse(int RunId, RunStatus Status, string Message, int ResultCount);

