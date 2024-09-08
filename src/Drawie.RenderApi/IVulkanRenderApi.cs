namespace Drawie.RenderApi;

public interface IVulkanRenderApi : IRenderApi
{
    public new IReadOnlyCollection<IVulkanWindowRenderApi> WindowRenderApis { get; }
}