namespace IAM.Application.Permissions.DTOs
{
    public record PermissionItemDto(string Key, string Label);
    public record PermissionGroupDto(string Module, string Label, List<PermissionItemDto> Permissions);
}
