namespace IAM.Application.Permissions.DTOs.Responses;

public record PermissionGroupDto(string Module, string Label, List<PermissionItemDto> Permissions);

